using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.Client;
using EllieMae.Encompass.Query;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EncompassLibrary
{
    internal class LoanLogicQC
    {
        public bool MarkLoansForQC(Session session, int randomPercentToSelect,List<string> loanFolders, out List<string> allLoansFromPreFundingReport, out List<string> loansMarkedForQCOut)
        {
            List<string> randomLoans = new List<string>();
            allLoansFromPreFundingReport = GetLoansByPreFundingQCReport(session, loanFolders);
            loansMarkedForQCOut = new List<string>();
            if (allLoansFromPreFundingReport.Count == 0)
            {
                return true;
            }
            else
            {
                Random r = new Random();
                var randomPercent = (int)Math.Round(allLoansFromPreFundingReport.Count * (.01 * randomPercentToSelect));
                randomLoans = allLoansFromPreFundingReport.OrderBy(x => r.Next()).Take(randomPercent).ToList();
                randomLoans = randomLoans.Distinct().ToList();
                //There might some loans selected multiple times randomly
                while (randomLoans.Count < randomPercent)
                {
                    List<string> randomLoansNextParse = allLoansFromPreFundingReport.OrderBy(x => r.Next()).Take(randomPercent).Distinct().ToList();
                    foreach (var loan in randomLoansNextParse)
                    {
                        if (!randomLoans.Contains(loan))
                        {
                            randomLoans.Add(loan);
                            if (randomLoans.Count == randomPercent)
                                break;
                        }
                    }
                }
                loansMarkedForQCOut = randomLoans;
                return BatchUpdateLoansForQC(randomLoans, session, "X");
            }
        }

        public bool UnMarkLoansSelectedForQC(Session session, out List<string> loansUnmarkedForQCOut)
        {
            loansUnmarkedForQCOut = new List<string>();
            List<string> loans = GetLoansMarkedForQC(session);
            if (loans.Count > 0)
            {
                try
                {
                    bool updateSucceeded = BatchUpdateLoansForQC(loans, session, string.Empty);
                    if (updateSucceeded)
                    {
                        loansUnmarkedForQCOut = loans;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
            return true;
        }

        private bool BatchUpdateLoansForQC(List<string> loanNumbers, Session session, string value)
        {
            QueryCriterion loanNumberJointCriterion = null;
            foreach (var loan in loanNumbers)
            {
                StringFieldCriterion loanCriterion = new StringFieldCriterion();
                loanCriterion.FieldName = "Loan.LoanNumber";
                loanCriterion.Value = loan;
                if (loanNumberJointCriterion == null)
                {
                    loanNumberJointCriterion = loanCriterion;
                }
                else
                {
                    loanNumberJointCriterion = loanNumberJointCriterion.Or(loanCriterion);
                }
            }
            BatchUpdate batch = new BatchUpdate(loanNumberJointCriterion);
            batch.Fields.Add("CX.QC.LOANSELECTEDFORQC", value);
            try
            {
                session.Loans.SubmitBatchUpdate(batch);
            }
            catch (Exception)
            {
                throw;
            }
            return true;
        }
        private List<string> GetLoansMarkedForQC(Session session)
        {
            List<string> loans = new List<string>();
            try
            {
                StringFieldCriterion qcCriterion = new StringFieldCriterion("Fields.CX.QC.LOANSELECTEDFORQC", "X", StringFieldMatchType.Exact, true);
                PipelineCursor pipelineCursor = session.Loans.QueryPipeline(qcCriterion, PipelineSortOrder.LastName);
                foreach (PipelineData data in pipelineCursor)
                {
                    loans.Add(data["LoanNumber"].ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
            return loans;
        }

        private List<string> GetLoansByPreFundingQCReport(Session session, List<string> loanFolders)
        {
            DateFieldCriterion estClosingDateCriterion = new DateFieldCriterion("Fields.763", DateTime.Now, OrdinalFieldMatchType.Equals, DateFieldMatchPrecision.Month);
            StringFieldCriterion loanInfoChannelCriterion = new StringFieldCriterion("Fields.2626", "Banked", StringFieldMatchType.Contains, true);
            StringFieldCriterion lockStatusCriterion1 = new StringFieldCriterion("Loan.LockStatus", "Locked", StringFieldMatchType.Exact, true);
            StringFieldCriterion lockStatusCriterion2 = new StringFieldCriterion("Loan.LockStatus", "Expired", StringFieldMatchType.Exact, true);
            QueryCriterion lockStatusJointCriterion = lockStatusCriterion1.Or(lockStatusCriterion2);
            DateFieldCriterion approvedMileStoneDateCriterion = new DateFieldCriterion("Loan.DateApprovalReceived", DateTime.MinValue, OrdinalFieldMatchType.Equals, DateFieldMatchPrecision.Exact);
            StringFieldCriterion loanActiveCriterion = new StringFieldCriterion("Fields.1393", "Active Loan", StringFieldMatchType.Exact, true);
            //Loan Folder Criterions
            QueryCriterion loanFolderJointCriterion = null;
            foreach (var currentFolder in loanFolders)
            {
                StringFieldCriterion loanFolderCriterion = new StringFieldCriterion("LoanFolder", currentFolder, StringFieldMatchType.Exact, true);
                if (loanFolderJointCriterion == null)
                {
                    loanFolderJointCriterion = loanFolderCriterion;
                }
                else
                {
                    loanFolderJointCriterion = loanFolderJointCriterion.Or(loanFolderCriterion);
                }
            }
            QueryCriterion jointCriterion = loanActiveCriterion.And(approvedMileStoneDateCriterion)
                .And(lockStatusJointCriterion).And(loanInfoChannelCriterion).And(estClosingDateCriterion).And(loanFolderJointCriterion);
            

            PipelineCursor cursor = session.Loans.QueryPipeline(jointCriterion, PipelineSortOrder.LastName);
            List<string> preFundingLoanNumbers = new List<string>();
            try
            {
                foreach (PipelineData data in cursor)
                {
                    preFundingLoanNumbers.Add(data["LoanNumber"].ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                cursor.Close();
            }
            return preFundingLoanNumbers;
        }
    }
}
