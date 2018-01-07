using FGMC.LoanLogicsQC.Web.Constants;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace FGMC.LoanLogicsQC.Web.Helper
{
    public class LoanDocExtractionHelper
    {
      
        public static List<SelectListItem> LoadSheetsFromExcel(HttpPostedFileBase browsefile,string clientId)
        {
            List<SelectListItem> selectsheetlist = new List<SelectListItem>();
            string filename = Path.GetFileName(browsefile.FileName);
            //string clientId = "rnarayan";//Convert.ToString(Session[EncompassUtilityConstants.CURRENTLYLOGGEDINUSER]);
            filename = string.Format("{0}_{1}", clientId, filename);
            string[] xlsfiles = Directory.GetFiles(Path.GetTempPath(), string.Concat(clientId, LoanLogicsQCConstants.STRXLS));
            string[] xlsxfiles = Directory.GetFiles(Path.GetTempPath(), string.Concat(clientId, LoanLogicsQCConstants.STRXLSX));
            foreach (string file in xlsfiles)
            {
                System.IO.File.Delete(file);
            }
            foreach (string file in xlsxfiles)
            {
                System.IO.File.Delete(file);
            }
            //_logger.Info(string.Concat(clientId, LoanDocExtractorConstants.STR_SELECT_FILE, filename));
            if (filename != null)
            {
                string filepath = Path.Combine(Path.GetTempPath(), Path.GetFileName(filename));
                browsefile.SaveAs(filepath);
                //_logger.Info(string.Concat(clientId, EncompassUtilityConstants.STR_SAVINGFILE, filepath));
                selectsheetlist = GetSheetNamesFromExcel(filepath);
            }
            return selectsheetlist;
        }

        public static DataTable OpenExcelSheet(string Filename,string Seletedsheet)
        {
            using (
               System.Data.OleDb.OleDbConnection objConn =
                   new OleDbConnection(string.Format(LoanLogicsQCConstants.STR_OLEDB_CONNECTION, Filename)))
            {
                OleDbDataAdapter da = new OleDbDataAdapter(string.Format(LoanLogicsQCConstants.STR_OLEDB_SELECTQUERY, Seletedsheet), objConn);
                DataTable dt = new DataTable();
                da.Fill(dt);
                DataTable dataTable = RemoveEmptyRowsFromDataTable(dt);
                objConn.Close();
                return dataTable;
            }
        }

        public static DataTable RemoveEmptyRowsFromDataTable(DataTable dt)
        {
            int columnCount = dt.Columns.Count;
            for (int i = dt.Rows.Count - 1; i >= 0; i--)
            {
                bool allNull = true;
                for (int j = 0; j < columnCount; j++)
                {
                    if (dt.Rows[i][j] != DBNull.Value && !string.IsNullOrWhiteSpace(dt.Rows[i][j].ToString()))
                    {
                        allNull = false;
                        break;
                    }
                }
                if (allNull)
                {
                    dt.Rows[i].Delete();
                }
            }
            dt.AcceptChanges();
            return dt;
        }

        public static string ExcelDataToComaSepertedString(string Filename, string Seletedsheet)
        {
           DataTable dtExcelData=  OpenExcelSheet(Filename, Seletedsheet);
            if(dtExcelData.Columns.Contains("Loan Number"))
            {
                return String.Join(",", dtExcelData.AsEnumerable().Select(row => row.Field<string>("Loan Number").ToString()));
               
            }
            else
            {
                return "Loan Number Column Doesn't Exists";
            }
            
        }

        private static List<SelectListItem> GetSheetNamesFromExcel(string fileName)
        {
            List<SelectListItem> selectsheetlist = new List<SelectListItem>();
            List<SelectListItem> Sheetnames = new List<SelectListItem>();
            using (OleDbConnection connection = new OleDbConnection(string.Format(LoanLogicsQCConstants.STR_OLEDB_CONNECTION, fileName)))
            {
                if (connection.State == System.Data.ConnectionState.Open)
                {
                    connection.Close();
                }
                connection.Open();
                DataTable Sheets = connection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                foreach (DataRow Sheet in Sheets.Rows)
                {
                    string sheetName = Sheet[2].ToString().Replace("'", "");
                    if (!sheetName.EndsWith("$"))
                        continue;
                    sheetName = sheetName.TrimEnd('$');
                    SelectListItem item = new SelectListItem();
                    item.Text = sheetName;
                    item.Value = sheetName;
                    selectsheetlist.Add(item);
                }
                connection.Close();
            }

            return selectsheetlist;
        }
        public static string GetServerFilePath(string excelFilename, string clientId)
        {
            string fileName = Path.GetFileName(excelFilename);
            //string clientId = Convert.ToString(Session[EncompassUtilityConstants.CURRENTLYLOGGEDINUSER]);
            fileName = string.Format("{0}_{1}", clientId, fileName);
            string serverfilepath = Path.Combine(Path.GetTempPath(), fileName);
            return serverfilepath;
        }
        public static DataTable ExcelData(string Filename, string Seletedsheet)
        {
            DataTable dtExcelData = OpenExcelSheet(Filename, Seletedsheet);
            if (dtExcelData.Columns.Contains("Loan Number"))
            {
                return dtExcelData;

            }
            else
            {
                return null;
            }

        }


    }


}