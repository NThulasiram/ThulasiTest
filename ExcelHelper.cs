using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGMC.LoanLogicsQC.Web.Helper
{
    public class ExcelHelper
    {
        public static Stream ReturnExcelAsStream(DataTable table)
        {
            using (var wb = new XLWorkbook())
            {
                wb.Worksheets.Add(table, table.TableName);
                var memoryStream = new MemoryStream();
                wb.SaveAs(memoryStream);
                return memoryStream;
            }
        }
        public static string SaveExcelAsFile(DataTable table, string path, string fileName)
        {
            var filepath = "";
            using (var wb = new XLWorkbook())
            {
                wb.Worksheets.Add(table, table.TableName);
                filepath = Path.Combine(path, fileName);
                wb.SaveAs(filepath);
            }
            return filepath;
        }
    }
}
