using FGMC.LoanLogicsQC.Web.Constants;
using FGMC.LoanLogicsQC.Web.FGMCQCServiceReference;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGMC.LoanLogicsQC.Web.Helper
{
   public static class DataTableExtension
    {
        public static DataTable ToDataTable<T>(this List<T> list)
        {
            var entityType = typeof(T);
            var dataTable = new DataTable(entityType.Name);
            var propertyDescriptorCollection = TypeDescriptor.GetProperties(entityType);
            foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
            {
                var propertyType = Nullable.GetUnderlyingType(propertyDescriptor.PropertyType) ?? propertyDescriptor.PropertyType;
                dataTable.Columns.Add(propertyDescriptor.Name, propertyType);
            }
            foreach (T item in list)
            {
                var row = dataTable.NewRow();
                foreach (PropertyDescriptor propertyDescriptor in propertyDescriptorCollection)
                {
                    var value = propertyDescriptor.GetValue(item);
                    row[propertyDescriptor.Name] = value ?? DBNull.Value;
                }
                dataTable.Rows.Add(row);
            }
            return dataTable;
        }

    }
    public class DataGenerator
    {
        public static DataTable FillDataTable(List<FGMCQCServiceReference.LoanLogicsReport> reportData, string reportType)
        {
            var dataTable = new DataTable();

            if (reportType == LoanLogicsQCConstants.PRECLOSEREPORT)
                dataTable.TableName = "LoanLogics PreClose Report";
            else if (reportType == LoanLogicsQCConstants.POSTCLOSEREPORT)
                dataTable.TableName = "LoanLogics PostClose Report";
            else if (reportType == LoanLogicsQCConstants.ADVERSEREPORT)
                dataTable.TableName = "LoanLogics Adverse Report";




            dataTable.Columns.Add("AmortizationType");
            dataTable.Columns.Add("AnnualPercentageRate");
            dataTable.Columns.Add("ApplicationDate");
            dataTable.Columns.Add("AppraisedValue");
            dataTable.Columns.Add("Borrower1FirstName");
            dataTable.Columns.Add("Borrower1LastName");
            dataTable.Columns.Add("Borrower1SSN");
            dataTable.Columns.Add("Borrower2FirstName");
            dataTable.Columns.Add("Borrower2LastName");
            dataTable.Columns.Add("Borrower2SSN");
            dataTable.Columns.Add("Borrower3FirstName");
            dataTable.Columns.Add("Borrower3LastName");
            dataTable.Columns.Add("Borrower3SSN");
            dataTable.Columns.Add("Borrower4FirstName");
            dataTable.Columns.Add("Borrower4LastName");
            dataTable.Columns.Add("Borrower4SSN");
            dataTable.Columns.Add("Borrower1MiddleInitial");
            dataTable.Columns.Add("Borrower2MiddleInitial");
            dataTable.Columns.Add("InterestRate");
            dataTable.Columns.Add("LoanOfficerName");
            dataTable.Columns.Add("LoanNumber");
            dataTable.Columns.Add("LTV");
            dataTable.Columns.Add("MortgageAppliedFor");
            dataTable.Columns.Add("PropertyUnitNo");
            dataTable.Columns.Add("PropertyCity");
            dataTable.Columns.Add("PropertyState");
            dataTable.Columns.Add("PropertyStreet1");
            dataTable.Columns.Add("PropertyType");
            dataTable.Columns.Add("PropertyWillBe");
            dataTable.Columns.Add("PropertyZipcode");
            dataTable.Columns.Add("PurchasePrice");
            dataTable.Columns.Add("PurposeofLoan");
            dataTable.Columns.Add("TotalLoanAmount");
            dataTable.Columns.Add("UnderwriterName");
            dataTable.Columns.Add("UnpaidPrincipalBalance");
            if (reportType == LoanLogicsQCConstants.POSTCLOSEREPORT)
            {
                dataTable.Columns.Add("Borrower3MiddleInitial");
                dataTable.Columns.Add("Borrower4MiddleInitial");
                dataTable.Columns.Add("ClosingDate");
                dataTable.Columns.Add("MINNumber");
                dataTable.Columns.Add("CaseNumber");
                dataTable.Columns.Add("PaymentAmount");
                dataTable.Columns.Add("FirstPaymentDate");
                dataTable.Columns.Add("MaturityDate");
                dataTable.Columns.Add("PMI");
                dataTable.Columns.Add("DisbursementDate");
                dataTable.Columns.Add("StreamlineType");
                dataTable.Columns.Add("LoanTerm");
                dataTable.Columns.Add("CLTV");
                dataTable.Columns.Add("BackRatio");
                dataTable.Columns.Add("PrimaryFICO");
                dataTable.Columns.Add("BranchName");
                dataTable.Columns.Add("CloserName");
                dataTable.Columns.Add("ProcessorName");
                dataTable.Columns.Add("OriginationCompany");
                dataTable.Columns.Add("Name1EthnicityBox");
                dataTable.Columns.Add("Name2EthnicityBox");
                dataTable.Columns.Add("Name1RaceBox");
                dataTable.Columns.Add("Name2RaceBox");
                dataTable.Columns.Add("Name1SexBox");
                dataTable.Columns.Add("Name2SexBox");
                dataTable.Columns.Add("FrontRatio");
                dataTable.Columns.Add("Investor");
                dataTable.Columns.Add("TPO");
                dataTable.Columns.Add("Channel");
                dataTable.Columns.Add("Borrower1SelfEmployed");
                dataTable.Columns.Add("Borrower2SelfEmployed");

            }
            Func<string, string> check = NullCheck;
            foreach (var data in reportData)
            {
                var row = dataTable.NewRow();
                row["AmortizationType"] = check(data.Amortization);
                row["AnnualPercentageRate"] = check(data.AnnualPercentageRate);
                row["ApplicationDate"] = check(data.ApplicationDate);
                row["AppraisedValue"] = check(data.AppraisedValue);
                row["Borrower1FirstName"] = check(data.Borrower1FirstName);
                row["Borrower1LastName"] = check(data.Borrower1LastName);
                row["Borrower1MiddleInitial"] = check(data.Borrower1MiddleInitial);
                row["Borrower1SSN"] = check(data.Borrower1SSN);
                row["Borrower2FirstName"] = check(data.Borrower2FirstName);
                row["Borrower2LastName"] = check(data.Borrower2LastName);
                row["Borrower2MiddleInitial"] = check(data.Borrower2MiddleInitial);
                row["Borrower2SSN"] = check(data.Borrower2SSN);
                row["Borrower3FirstName"] = check(data.Borrower3FirstName);
                row["Borrower3LastName"] = check(data.Borrower3LastName);
                row["Borrower3SSN"] = check(data.Borrower3SSN);
                row["Borrower4FirstName"] = check(data.Borrower4FirstName);
                row["Borrower4LastName"] = check(data.Borrower4LastName);
                row["Borrower4SSN"] = check(data.Borrower4SSN);
                row["InterestRate"] = check(data.InterestRate);
                row["LoanNumber"] = check(data.LoanNumber);
                row["LoanOfficerName"] = check(data.LoanOfficerName);
                row["LTV"] = check(data.LTV);
                row["MortgageAppliedFor"] = check(data.MortgageAppliedFor);
                row["PropertyCity"] = check(data.PropertyCity);
                row["PropertyState"] = check(data.PropertyState);
                row["PropertyStreet1"] = check(data.PropertyStreet1);
                row["PropertyType"] = check(data.PropertyType);
                row["PropertyUnitNo"] = check(data.PropertyUnitNo);
                row["PropertyWillBe"] = check(data.PropertyWillBe);
                row["PropertyZipcode"] = check(data.PropertyZipcode);
                row["PurchasePrice"] = check(data.PurchasePrice);
                row["PurposeofLoan"] = check(data.PurposeofLoan);
                row["TotalLoanAmount"] = check(data.TotalLoanAmount);
                row["UnderwriterName"] = check(data.UnderwriterName);
                row["UnpaidPrincipalBalance"] = check(data.UnpaidPrincipalBalance);
                if (reportType == LoanLogicsQCConstants.POSTCLOSEREPORT)
                {
                    var postCloseData = new PostCloseReport();
                    row["ClosingDate"] = check(postCloseData.ClosingDate);
                    row["MINNumber"] = check(postCloseData.MINNumber);
                    row["CaseNumber"] = check(postCloseData.CaseNumber);
                    row["PaymentAmount"] = check(postCloseData.PaymentAmount);
                    row["FirstPaymentDate"] = check(postCloseData.FirstPaymentDate);
                    row["MaturityDate"] = check(postCloseData.MaturityDate);
                    row["PMI"] = check(postCloseData.PMI);
                    row["DisbursementDate"] = check(postCloseData.DisbursementDate);
                    row["StreamlineType"] = check(postCloseData.StreamlineType);
                    row["LoanTerm"] = check(postCloseData.LoanTerm);
                    row["CLTV"] = check(postCloseData.CLTV);
                    row["BackRatio"] = "";
                    row["PrimaryFICO"] = check(postCloseData.PrimaryFICO);
                    row["BranchName"] = check(postCloseData.BranchName);
                    row["CloserName"] = check(postCloseData.CloserName);
                    row["ProcessorName"] = check(postCloseData.ProcessorName);
                    row["OriginationCompany"] = check(postCloseData.OriginationCompany);
                    row["Name1EthnicityBox"] = check(postCloseData.Name1EthnicityBox);
                    row["Name2EthnicityBox"] = check(postCloseData.Name2EthnicityBox);
                    row["Borrower3MiddleInitial"] = "";
                    row["Borrower4MiddleInitial"] = "";
                    row["Name1RaceBox"] = "";
                    row["Name2RaceBox"] = "";
                    row["Name1SexBox"] = check(postCloseData.Name1SexBox);
                    row["Name2SexBox"] = check(postCloseData.Name2SexBox);
                    row["FrontRatio"] = check(postCloseData.FrontRatio);
                    row["Investor"] = check(postCloseData.Investor);
                    row["TPO"] = check(postCloseData.TPO);
                    row["Channel"] = check(postCloseData.Channel);
                    row["Borrower1SelfEmployed"] = "";
                    row["Borrower2SelfEmployed"] = "";

                }
                dataTable.Rows.Add(row);

            }


            return dataTable;


        }

        public static DataTable FillDataTableForPostClose(List<FGMCQCServiceReference.PostCloseReport> reportData, string reportType)
        {
            var dataTable = new DataTable();

			if (reportType == LoanLogicsQCConstants.PRECLOSEREPORT)
			{
				dataTable.TableName = "LoanLogics PreClose Report";
			}

			else if (reportType == LoanLogicsQCConstants.POSTCLOSEREPORT)
			{
				dataTable.TableName = "LoanLogics PostClose Report";
			} 
               
            else if (reportType == LoanLogicsQCConstants.ADVERSEREPORT)
			{
				dataTable.TableName = "LoanLogics Adverse Report";
			}
                




            dataTable.Columns.Add("AmortizationType");
            dataTable.Columns.Add("AnnualPercentageRate");
            dataTable.Columns.Add("ApplicationDate");
            dataTable.Columns.Add("AppraisedValue");
            dataTable.Columns.Add("Borrower1FirstName");
            dataTable.Columns.Add("Borrower1LastName");
            dataTable.Columns.Add("Borrower1SSN");
            dataTable.Columns.Add("Borrower2FirstName");
            dataTable.Columns.Add("Borrower2LastName");
            dataTable.Columns.Add("Borrower2SSN");
            dataTable.Columns.Add("Borrower3FirstName");
            dataTable.Columns.Add("Borrower3LastName");
            dataTable.Columns.Add("Borrower3SSN");
            dataTable.Columns.Add("Borrower4FirstName");
            dataTable.Columns.Add("Borrower4LastName");
            dataTable.Columns.Add("Borrower4SSN");
            dataTable.Columns.Add("Borrower1MiddleInitial");
            dataTable.Columns.Add("Borrower2MiddleInitial");
            dataTable.Columns.Add("InterestRate");
            dataTable.Columns.Add("LoanOfficerName");
            dataTable.Columns.Add("LoanNumber");
            dataTable.Columns.Add("LTV");
            dataTable.Columns.Add("MortgageAppliedFor");
            dataTable.Columns.Add("PropertyUnitNo");
            dataTable.Columns.Add("PropertyCity");
            dataTable.Columns.Add("PropertyState");
			if (!reportType.Equals(LoanLogicsQCConstants.POSTCLOSEREPORT))
				dataTable.Columns.Add("PropertyStreet1");
            if (reportType.Equals(LoanLogicsQCConstants.POSTCLOSEREPORT))
                dataTable.Columns.Add("PropertyAddressline1");
            dataTable.Columns.Add("PropertyType");
            dataTable.Columns.Add("PropertyWillBe");
            dataTable.Columns.Add("PropertyZipcode");
            dataTable.Columns.Add("PurchasePrice");
            dataTable.Columns.Add("PurposeofLoan");
            dataTable.Columns.Add("TotalLoanAmount");
            dataTable.Columns.Add("UnderwriterName");
            dataTable.Columns.Add("UnpaidPrincipalBalance");
            if (reportType == LoanLogicsQCConstants.POSTCLOSEREPORT)
            {
                dataTable.Columns.Add("Borrower3MiddleInitial");
                dataTable.Columns.Add("Borrower4MiddleInitial");
                dataTable.Columns.Add("ClosingDate");
                dataTable.Columns.Add("MINNumber");
                dataTable.Columns.Add("CaseNumber");
                dataTable.Columns.Add("PaymentAmount");
                dataTable.Columns.Add("FirstPaymentDate");
                dataTable.Columns.Add("MaturityDate");
                dataTable.Columns.Add("PMI");
                dataTable.Columns.Add("DisbursementDate");
                dataTable.Columns.Add("StreamlineType");
                dataTable.Columns.Add("LoanTerm");
                dataTable.Columns.Add("CLTV");
                dataTable.Columns.Add("BackRatio");
                dataTable.Columns.Add("PrimaryFICO");
                dataTable.Columns.Add("UnderwritingStandard(typeofUWmethod,notdecision)");
                dataTable.Columns.Add("BranchName");
                dataTable.Columns.Add("CloserName");
                dataTable.Columns.Add("ProcessorName");
                dataTable.Columns.Add("OriginationCompany");
                dataTable.Columns.Add("Name1EthnicityBox");
                dataTable.Columns.Add("Name2EthnicityBox");
                dataTable.Columns.Add("Name1RaceBox");
                dataTable.Columns.Add("Name2RaceBox");
                dataTable.Columns.Add("Name1SexBox");
                dataTable.Columns.Add("Name2SexBox");
                dataTable.Columns.Add("FrontRatio");
                dataTable.Columns.Add("Investor");
                dataTable.Columns.Add("TPO");
                dataTable.Columns.Add("Channel");
                dataTable.Columns.Add("Borrower1SelfEmployed");
                dataTable.Columns.Add("Borrower2SelfEmployed");

            }
            Func<string, string> check = NullCheck;
            foreach (var data in reportData)
            {
                var row = dataTable.NewRow();
                row["AmortizationType"] = check(data.Amortization);
                row["AnnualPercentageRate"] = check(data.AnnualPercentageRate);
                row["ApplicationDate"] = check(data.ApplicationDate);
                row["AppraisedValue"] = check(data.AppraisedValue);
                row["Borrower1FirstName"] = check(data.Borrower1FirstName);
                row["Borrower1LastName"] = check(data.Borrower1LastName);
                row["Borrower1MiddleInitial"] = check(data.Borrower1MiddleInitial);
                row["Borrower1SSN"] = check(data.Borrower1SSN);
                row["Borrower2FirstName"] = check(data.Borrower2FirstName);
                row["Borrower2LastName"] = check(data.Borrower2LastName);
                row["Borrower2MiddleInitial"] = check(data.Borrower2MiddleInitial);
                row["Borrower2SSN"] = check(data.Borrower2SSN);
                row["Borrower3FirstName"] = check(data.Borrower3FirstName);
                row["Borrower3LastName"] = check(data.Borrower3LastName);
                row["Borrower3SSN"] = check(data.Borrower3SSN);
                row["Borrower4FirstName"] = check(data.Borrower4FirstName);
                row["Borrower4LastName"] = check(data.Borrower4LastName);
                row["Borrower4SSN"] = check(data.Borrower4SSN);
                row["InterestRate"] = check(data.InterestRate);
                row["LoanNumber"] = check(data.LoanNumber);
                row["LoanOfficerName"] = check(data.LoanOfficerName);
                row["LTV"] = check(data.LTV);
                row["MortgageAppliedFor"] = check(data.MortgageAppliedFor);
                row["PropertyCity"] = check(data.PropertyCity);
                row["PropertyState"] = check(data.PropertyState);
                row["PropertyAddressline1"] = check(data.PropertyStreet1);
                row["PropertyType"] = check(data.PropertyType);
                row["PropertyUnitNo"] = check(data.PropertyUnitNo);
                row["PropertyWillBe"] = check(data.PropertyWillBe);
                row["PropertyZipcode"] = check(data.PropertyZipcode);
                row["PurchasePrice"] = check(data.PurchasePrice);
                row["PurposeofLoan"] = check(data.PurposeofLoan);
                row["TotalLoanAmount"] = check(data.TotalLoanAmount);
                row["UnderwriterName"] = check(data.UnderwriterName);
                row["UnpaidPrincipalBalance"] = check(data.UnpaidPrincipalBalance);
                row["ClosingDate"] = check(data.ClosingDate);
                row["MINNumber"] = check(data.MINNumber);
                row["CaseNumber"] = check(data.CaseNumber);
                row["PaymentAmount"] = check(data.PaymentAmount);
                row["FirstPaymentDate"] = check(data.FirstPaymentDate);
                row["MaturityDate"] = check(data.MaturityDate);
                row["PMI"] = check(data.PMI);
                row["DisbursementDate"] = check(data.DisbursementDate);
                row["StreamlineType"] = check(data.StreamlineType);
                row["LoanTerm"] = check(data.LoanTerm);
                row["CLTV"] = check(data.CLTV);
                row["BackRatio"] = check(data.BackRatio);
                row["PrimaryFICO"] = check(data.PrimaryFICO);
                row["UnderwritingStandard(typeofUWmethod,notdecision)"] = check(data.UnderwritingStandard);
                row["BranchName"] = check(data.BranchName);
                row["CloserName"] = check(data.CloserName);
                row["ProcessorName"] = check(data.ProcessorName);
                row["OriginationCompany"] = check(data.OriginationCompany);
                row["Name1EthnicityBox"] = check(data.Name1EthnicityBox);
                row["Name2EthnicityBox"] = check(data.Name2EthnicityBox);
                row["Borrower3MiddleInitial"] = "";
                row["Borrower4MiddleInitial"] = "";
                row["Name1RaceBox"] = check(data.Name1RaceBox);
                row["Name2RaceBox"] = check(data.Name2RaceBox);
                row["Name1SexBox"] = check(data.Name1SexBox);
                row["Name2SexBox"] = check(data.Name2SexBox);
                row["FrontRatio"] = check(data.FrontRatio);
                row["Investor"] = check(data.Investor);
                row["TPO"] = check(data.TPO);
                row["Channel"] = check(data.Channel);
                row["Borrower1SelfEmployed"] = check(data.Borrower1SelfEmployed);
				row["Borrower2SelfEmployed"] = check(data.Borrower2SelfEmployed);
                dataTable.Rows.Add(row);
            }


            return dataTable;


        }
        private static string NullCheck(string valueToCheck)
        {
            return !string.IsNullOrEmpty(valueToCheck) ? valueToCheck : string.Empty;
        }
    }

}
