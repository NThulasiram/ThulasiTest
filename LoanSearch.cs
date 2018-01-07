using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using EllieMae.Encompass.Automation;
using EllieMae.Encompass.BusinessObjects.Loans;
using EllieMae.Encompass.Forms;
using FGMCDocumentCopy.Constants;
using FGMCDocumentCopy.Data;
using FGMCDocumentCopy.Helpers;
using FGMCDocumentCopy.Models;
using Button = EllieMae.Encompass.Forms.Button;
using TextBox = EllieMae.Encompass.Forms.TextBox;

namespace FGMCDocumentCopy.Forms
{
    class LoanSearch : EllieMae.Encompass.Forms.Form
    {
        #region Controls

        private Button _btnSearch = null;
        private TextBox _oldLoanTxt = null;
        private TextBox _serverLaonTxt = null;
        private TextBox _borrowerSsnTxt = null;
        private MultilineTextBox _multilineTextBox = null;

        private System.Windows.Forms.Button _btnGetDoc;
        private System.Windows.Forms.DataGridView _grdSourceDocuments;
        private System.Windows.Forms.DataGridView _grdDestDocuments;
        private System.Windows.Forms.Button _btnMoveSourToDest;
        private System.Windows.Forms.Button _btnMoveDestToSour;
        private System.Windows.Forms.Button _btnCopyDoc;
        private System.Windows.Forms.CheckBox _chkSelectAll;
        private System.Windows.Forms.CheckBox _chkCurrentVer;
        private System.Windows.Forms.TextBox _txtSearchDoc;
        private System.Windows.Forms.Button _btnSearchDoc;
        private System.Windows.Forms.Button _btnCancel;
        private System.Windows.Forms.Label _lblProgressPercentage;
        private System.Windows.Forms.ProgressBar _docCopyProgressBar;
        private System.Windows.Forms.Button _btnClear;
        BackgroundWorker _bw;
        FgmcLoanHelper _loanHelper = new FgmcLoanHelper();
        System.Windows.Forms.DataGridView _gridOldLoanDetails;
        DocumentForm _gridFrm;

        List<LoanTemplate> _loanTemplate = new List<LoanTemplate>();
        List<AttachmentTemplate> _attachmentTemplate = new List<AttachmentTemplate>();
        List<DocumentFile> _getDocumetsList = new List<DocumentFile>();
        List<AttachmentTemplate> _attachmentList = new List<AttachmentTemplate>();
        string _loanNumber = string.Empty;

        #endregion

        public override void CreateControls()
        {
            FindControls();
        }

        private void FindControls()
        {
            try
            {
                WriteLog("Finding Controls", null);
                this.Load += LoanSearch_Load; // Encompass form load
                this._oldLoanTxt = FindControl(FgmcDocumentCopyConstants.ControlsoldLoanTxt) as TextBox;
                if (_oldLoanTxt != null) _oldLoanTxt.FocusOut += OldLoanTxt_FocusOut;
                this._serverLaonTxt = FindControl(FgmcDocumentCopyConstants.ControlsserverLaonTxt) as TextBox;
                if (_serverLaonTxt != null) _serverLaonTxt.FocusOut += ServerLaonTxt_FocusOut;
                this._borrowerSsnTxt = FindControl(FgmcDocumentCopyConstants.ControlsborrowerSsnTxt) as TextBox;
                if (_borrowerSsnTxt != null) _borrowerSsnTxt.FocusOut += BorrowerSSNTxt_FocusOut;
                this._btnSearch = FindControl(FgmcDocumentCopyConstants.ControlsbtnSearch) as Button;
                if (_btnSearch != null) _btnSearch.Click += BtnSearch_Click;
                this._multilineTextBox = FindControl(FgmcDocumentCopyConstants.ControlsMultitxtTracker) as MultilineTextBox;

                InitializeDocumentFormControls();
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        private void LoanSearch_Load(object sender, EventArgs e)
        {
            WriteLog("Creating Session.txt for handle btnSearch enable and dissable.", null);
            try
            {
                if (Directory.Exists(Path.Combine(EllieMae.Encompass.Client.Session.EncompassDataDirectory, "session.txt")))
                {

                    string readOldText =
                        File.ReadAllText(EllieMae.Encompass.Client.Session.EncompassDataDirectory + @"\" + "session.txt");
                    if (!readOldText.Contains(EncompassApplication.CurrentLoan.Session.ID.ToString()))
                    {
                        File.Delete(Path.Combine(EllieMae.Encompass.Client.Session.EncompassDataDirectory, "session.txt"));
                        File.WriteAllText(Path.Combine(EllieMae.Encompass.Client.Session.EncompassDataDirectory, "session.txt"), EncompassApplication.CurrentLoan.Session.ID.ToString());

                    }
                    else
                    {
                        if (!readOldText.Contains(EncompassApplication.CurrentLoan.LoanNumber.ToString()))
                        {

                            EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value = "true";
                        }
                    }
                }
                else
                {
                    File.WriteAllText(Path.Combine(EllieMae.Encompass.Client.Session.EncompassDataDirectory, "session.txt"), EncompassApplication.CurrentLoan.Session.ID.ToString());

                }

                if (EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value == "true")
                {
                    this._btnSearch.Enabled = true;
                }
                if (EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value == "")
                {
                    this._btnSearch.Enabled = true;
                }
                if (EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value == "false")
                {
                    this._btnSearch.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        #region DocumentCopy

        List<DocumentFile> _grdattachmentList = new List<DocumentFile>();
        List<Loan> _resultantLoans = new List<Loan>();

        private void InitializeDocumentFormControls()
        {
            try
            {
                WriteLog("InitializeDocumentFormControls Started.", null);
                _gridFrm = new DocumentForm();
                this._btnGetDoc = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsBtnGetDoc] as System.Windows.Forms.Button;
                this._grdSourceDocuments = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsGrdSourceDocuments] as System.Windows.Forms.DataGridView;
                this._grdDestDocuments = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsGrdDestDocuments] as System.Windows.Forms.DataGridView;
                this._btnMoveSourToDest = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsBtnMoveSourToDest] as System.Windows.Forms.Button;
                this._btnMoveDestToSour = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsBtnMoveDestToSour] as System.Windows.Forms.Button;
                this._btnCopyDoc = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsBtnCopyDoc] as System.Windows.Forms.Button;
                this._chkSelectAll = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsChkSelectAll] as System.Windows.Forms.CheckBox;
                this._chkCurrentVer = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsChkCurrentVer] as System.Windows.Forms.CheckBox;
                this._txtSearchDoc = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsTxtSearchDoc] as System.Windows.Forms.TextBox;
                this._btnSearchDoc = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsBtnSearchDoc] as System.Windows.Forms.Button;
                this._btnCancel = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsBtnCancel] as System.Windows.Forms.Button;
                this._gridOldLoanDetails = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsGrdLoanInfo] as System.Windows.Forms.DataGridView;
                this._docCopyProgressBar = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsDocCopyProgressBar] as System.Windows.Forms.ProgressBar;
                this._lblProgressPercentage = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsLblProgressPercentage] as System.Windows.Forms.Label;
                this._btnClear = _gridFrm.Controls[FgmcDocumentCopyConstants.ControlsBtnClear] as System.Windows.Forms.Button;


                _btnGetDoc.Click -= BtnGetDoc_Click;
                _btnGetDoc.Click += BtnGetDoc_Click;
                _btnMoveSourToDest.Click -= BtnMoveSourToDest_Click;
                _btnMoveSourToDest.Click += BtnMoveSourToDest_Click;
                _btnMoveDestToSour.Click -= BtnMoveDestToSour_Click;
                _btnMoveDestToSour.Click += BtnMoveDestToSour_Click;
                _btnCopyDoc.Click -= BtnCopyDoc_Click;
                _btnCopyDoc.Click += BtnCopyDoc_Click;
                _btnSearchDoc.Click -= BtnSearchDoc_Click;
                _btnSearchDoc.Click += BtnSearchDoc_Click;
                _btnCancel.Click -= BtnCancel_Click;
                _btnCancel.Click += BtnCancel_Click;
                _chkSelectAll.CheckedChanged -= ChkSelectAll_CheckedChanged;
                _chkSelectAll.CheckedChanged += ChkSelectAll_CheckedChanged;
                _chkCurrentVer.CheckedChanged -= ChkCurrentVer_CheckedChanged;
                _chkCurrentVer.CheckedChanged += ChkCurrentVer_CheckedChanged;
                _btnClear.Click += _btnClear_Click;

                _gridFrm.FormClosed += GridFrm_FormClosed;
                _gridFrm.FormClosing += _gridFrm_FormClosing;

                _gridOldLoanDetails.CellValueChanged -= GridOldLoanDetails_CellValueChanged;
                _gridOldLoanDetails.CellValueChanged += GridOldLoanDetails_CellValueChanged;
                _gridOldLoanDetails.CurrentCellDirtyStateChanged += GridOldLoanDetails_CurrentCellDirtyStateChanged;

                _grdSourceDocuments.CellValueChanged += GrdSourceDocuments_CellValueChanged;
                _grdDestDocuments.CellValueChanged += GrdDestDocuments_CellValueChanged;
                _grdSourceDocuments.CurrentCellDirtyStateChanged -= GrdSourceDocuments_CurrentCellDirtyStateChanged;
                _grdSourceDocuments.CurrentCellDirtyStateChanged += GrdSourceDocuments_CurrentCellDirtyStateChanged;
                _grdDestDocuments.CurrentCellDirtyStateChanged -= GrdDestDocuments_CurrentCellDirtyStateChanged;
                _grdDestDocuments.CurrentCellDirtyStateChanged += GrdDestDocuments_CurrentCellDirtyStateChanged;
                _txtSearchDoc.KeyDown += TxtSearchDoc_KeyDown;

                _chkCurrentVer.Enabled = false;
                _txtSearchDoc.Enabled = false;
                _btnSearchDoc.Enabled = false;
                _grdSourceDocuments.Enabled = false;
                _grdDestDocuments.Enabled = false;
                _chkSelectAll.Enabled = false;
                _btnMoveSourToDest.Enabled = false;
                _btnMoveDestToSour.Enabled = false;
                _btnCopyDoc.Enabled = false;
                _btnCancel.Enabled = false;
                _btnClear.Enabled = false;
            }
            catch (Exception ex)
            {
                WriteLog("", ex);
                Macro.Alert(FgmcDocumentCopyConstants.TechnicalError);
                return;
            }

        }


        private void _gridFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (_bw != null)
                {
                    if (_bw.IsBusy)
                    {
                        DialogResult result = MessageBox.Show(FgmcDocumentCopyConstants.CancelMessage,
                   FgmcDocumentCopyConstants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (result == DialogResult.No)
                        {
                            e.Cancel = true;
                        }
                        else
                        {
                            _bw.CancelAsync();
                            e.Cancel = false;
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                WriteLog("", ex);
                MessageBox.Show(FgmcDocumentCopyConstants.TechnicalError, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }


        }


        private void GridFrm_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                InitializeDocumentFormControls();
                this._btnSearch.Enabled = true;
                EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value = "true";
            }
            catch (Exception ex)
            {
                WriteLog("", ex);
                MessageBox.Show(FgmcDocumentCopyConstants.TechnicalError, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }

        }

        private void TxtSearchDoc_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                BtnSearchDoc_Click((object)sender, (EventArgs)e);
            }
        }

        private void BtnGetDoc_Click(object sender, EventArgs e)
        {

            WriteLog("Getting Document Process Started.", null);
            StringCipher stringCipher = new StringCipher();
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                var oldLoanRow = (from DataGridViewRow oldLoanDetail in _gridOldLoanDetails.Rows
                                  where (Convert.ToBoolean(oldLoanDetail.Cells[FgmcDocumentCopyConstants.Columnchkcol].Value) == true)
                                  select oldLoanDetail).FirstOrDefault();
                if (oldLoanRow == null)
                    return;

                _loanNumber = oldLoanRow.Cells[FgmcDocumentCopyConstants.ColumnLoanId].Value.ToString();
                EllieMae.Encompass.Client.Session session1 = EncompassApplication.Session;

                #region SearchCriteria

                SearchCriteria searchCriteria = new SearchCriteria();

                searchCriteria.LoanCriteria = string.Join(FgmcDocumentCopyConstants.Comma, stringCipher.Encrypt(_loanNumber), String.Empty, String.Empty);
                #endregion

                WriteLog("Loading Endpoint.", null);
                #region Server Endpoint
                EllieMae.Encompass.BusinessObjects.DataObject objEndPoint = _loanHelper.GetCustomDataObject(FgmcDocumentCopyConstants.XmlServerEndpoint, session1);
                XmlDocument xmlendpoint = new XmlDocument();
                MemoryStream msEndPoint = new MemoryStream(objEndPoint.Data);
                xmlendpoint.Load(msEndPoint);
                string serverEndPoint = GetNodeValue(xmlendpoint, FgmcDocumentCopyConstants.ServerEndPoint);


                #endregion
                WriteLog("Serializing Sending data.", null);
                GenericXmlSerializer<SearchCriteria> genericXmlSerializer = new GenericXmlSerializer<SearchCriteria>();
                string documentDetails = genericXmlSerializer.SerializeObject(searchCriteria);

                string url = serverEndPoint + FgmcDocumentCopyConstants.GetLoanDocuments + documentDetails;

                HttpCustomClient httpclient = new HttpCustomClient();
                WriteLog("Calling Server mathod for get documents.", null);
                byte[] data = httpclient.Post(url, documentDetails);

                System.Text.Encoding encoder = System.Text.Encoding.UTF8;
                string datastr = encoder.GetString(data);

                GenericXmlSerializer<List<AttachmentTemplate>> genericXmlSerializer1 = new GenericXmlSerializer<List<AttachmentTemplate>>();
                WriteLog("Deserializing response data.", null);
                _attachmentTemplate = genericXmlSerializer1.DeserializeXml(datastr);

                if (_attachmentTemplate == null)
                    return;
                if (_attachmentTemplate.Count <= 0)
                {
                    MessageBox.Show(FgmcDocumentCopyConstants.DoesNotHaveDocuments, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    return;
                }

                GetLoanDocumentAttachments(_attachmentTemplate);

                _chkCurrentVer.Enabled = true;
                _txtSearchDoc.Enabled = true;
                _grdSourceDocuments.Enabled = true;
                _grdDestDocuments.Enabled = true;
                _chkSelectAll.Enabled = true;
                _btnSearchDoc.Enabled = true;
                _btnClear.Enabled = true;
                Cursor.Current = Cursors.Default;

            }

            catch (Exception ex)
            {
                MessageBox.Show(FgmcDocumentCopyConstants.TechnicalError, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private static string GetNodeValue(XmlDocument doc, string nodeName)
        {
            string nodeValue = string.Empty;
            var selectSingleNode = doc.SelectSingleNode(nodeName);
            if (selectSingleNode != null)
                nodeValue = selectSingleNode.InnerText;
            return nodeValue;
        }

        public void GetLoanDocumentAttachments(List<AttachmentTemplate> loanTemplates) // LoanNumber or LoanIdW
        {

            _grdattachmentList.Clear();
            _chkSelectAll.Checked = false;
            _txtSearchDoc.Text = string.Empty;
            _chkCurrentVer.Checked = false;
            try
            {
                WriteLog("Renaming attachments Started.", null);

                List<DocumentFile> remainingAttachments = new List<DocumentFile>();

                foreach (AttachmentTemplate attachment in loanTemplates)
                {
                    DocumentFile docFile = new DocumentFile();

                    if (attachment == null) { continue; }

                    docFile.Select = false;

                    // 23 Dec 2015 MD : Handle safely filename read in Test and Production 

                    var matchFoundName = (from a in FgmcDocumentCopyConstants.formatsSupported
                                          where attachment.Title.ToLower().Contains(a)
                                          select a).ToList();

                    //Production Environment has Title of attachment in the Name property
                    if (matchFoundName.Count() == 0)
                    {
                        var temp = attachment.Name;
                        attachment.Name = attachment.Title;
                        attachment.Title = temp;
                    }

                    string strReceivedDate = FgmcDocumentCopyConstants.LowLine + ((DateTime)(attachment.DocumentReceviedDate)).ToString(FgmcDocumentCopyConstants.DateTimeFormate).Replace(FgmcDocumentCopyConstants.Hypen, FgmcDocumentCopyConstants.Slash);
                    if (strReceivedDate.Equals(FgmcDocumentCopyConstants.DefaultDateTime))
                    {
                        strReceivedDate = string.Empty;
                    }

                    docFile.Document = (attachment.DocumentTitle + FgmcDocumentCopyConstants.LowLine + GetFileNameWithoutExt(attachment.Title) + strReceivedDate + GetFileExtension(attachment.Title));

                    docFile.IsCurrentVersion = attachment.IsActive;

                    docFile.SourceFileName = attachment.Name;

                    docFile.FileSize = (attachment.Size / 1000);

                    if (IsSpecifiedFolder(attachment.DocumentTitle))
                    {
                        _grdattachmentList.Add(docFile);
                    }
                    else
                    {
                        remainingAttachments.Add(docFile);
                    }

                    _attachmentList.Add(attachment);
                }
                WriteLog("Binding attachment data to Grid.", null);
                _grdattachmentList.Sort((x, y) => string.Compare(x.Document, y.Document));
                remainingAttachments.Sort((x, y) => string.Compare(x.Document, y.Document));

                _grdattachmentList.AddRange(remainingAttachments);

                var bindingList = new BindingList<DocumentFile>(_grdattachmentList);
                var source = new System.Windows.Forms.BindingSource(bindingList, null);
                _grdSourceDocuments.AllowUserToAddRows = false;
                _grdSourceDocuments.DataSource = source;
                int grdwidth = _grdSourceDocuments.Width;
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnSelect].Width = Convert.ToInt32((0.10 * grdwidth));
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnCurrentVersion].Width =
                    Convert.ToInt32((0.10 * grdwidth));
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnFileSize].Width = Convert.ToInt32((0.10 * grdwidth));
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnDocument].ReadOnly = true;
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnCurrentVersion].ReadOnly = true;
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnFileSize].ReadOnly = true;
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnSourceFileName].Visible = false;
            }
            catch (Exception ex)
            {
                WriteLog("", ex);
                MessageBox.Show(FgmcDocumentCopyConstants.TechnicalError, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private bool IsSpecifiedFolder(string folder)
        {
            string _folder = folder.ToLower();
            foreach (string validFolders in FgmcDocumentCopyConstants.ValidFolders)
            {
                if (_folder.Contains(validFolders.ToLower()))
                {
                    return true;
                }
            }


            return false;
        }

        private void ChkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                foreach (System.Windows.Forms.DataGridViewRow row in _grdSourceDocuments.Rows)
                {
                    row.Cells[FgmcDocumentCopyConstants.GrdColumnSelect].Value = _chkSelectAll.Checked;
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        private void ChkCurrentVer_CheckedChanged(object sender, EventArgs e)
        {
            ApplyingFilterForSourceGrid();

        }

        private void BtnSearchDoc_Click(object sender, EventArgs e)
        {
            ApplyingFilterForSourceGrid();

            if (_grdSourceDocuments.RowCount <= 0)
            {
                MessageBox.Show(FgmcDocumentCopyConstants.SearchDocsDoesNotExist, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                return;
            }

        }

        private void _btnClear_Click(object sender, EventArgs e)
        {
            _txtSearchDoc.Text = string.Empty;
            ApplyingFilterForSourceGrid();
        }

        private void BtnMoveSourToDest_Click(object sender, EventArgs e)
        {
            try
            {
                WriteLog("Moving attachments from Source To Destination.", null);
                var selectedfiles = from System.Windows.Forms.DataGridViewRow row in _grdSourceDocuments.Rows
                                    where (Convert.ToBoolean(row.Cells[0].Value) == true)
                                    select row;
                List<DocumentFile> destDocfiles = new List<DocumentFile>();
                foreach (System.Windows.Forms.DataGridViewRow selectedfile in selectedfiles)
                {

                    DocumentFile docFile = new DocumentFile();
                    docFile.Select = false;
                    docFile.Document = selectedfile.Cells[FgmcDocumentCopyConstants.GrdColumnDocument].Value.ToString();
                    docFile.IsCurrentVersion = Convert.ToBoolean(selectedfile.Cells[FgmcDocumentCopyConstants.GrdColumnCurrentVersion].Value);
                    docFile.SourceFileName = Convert.ToString(selectedfile.Cells[FgmcDocumentCopyConstants.GrdColumnSourceFileName].Value);
                    docFile.FileSize = Convert.ToInt32(selectedfile.Cells[FgmcDocumentCopyConstants.GrdColumnFileSize].Value);
                    destDocfiles.Add(docFile);
                }

                var bindingList = new BindingList<DocumentFile>(destDocfiles);
                var source = new System.Windows.Forms.BindingSource(bindingList, null);
                _grdDestDocuments.AllowUserToAddRows = false;
                _grdDestDocuments.DataSource = source;
                int grdwidth = _grdDestDocuments.Width;
                _grdDestDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnDocument].ReadOnly = true;
                _grdDestDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnCurrentVersion].ReadOnly = true;
                _grdDestDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnSourceFileName].Visible = false;
                _grdDestDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnSelect].Width = Convert.ToInt32((0.10 * grdwidth));
                _grdDestDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnCurrentVersion].Width =
                    Convert.ToInt32((0.10 * grdwidth));
                _grdDestDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnFileSize].Width = Convert.ToInt32((0.10 * grdwidth));


                if (_grdDestDocuments.RowCount > 0)
                {
                    _btnCopyDoc.Enabled = true;
                }
                else
                {
                    _btnCopyDoc.Enabled = false;
                }


                this._btnMoveDestToSour.Enabled = false;
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
                MessageBox.Show(FgmcDocumentCopyConstants.TechnicalError, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private void BtnMoveDestToSour_Click(object sender, EventArgs e)
        {
            try
            {
                WriteLog("Moving attachments from Destination To Source.", null);
                for (int i = _grdDestDocuments.Rows.Count - 1; i >= 0; i--)
                {
                    if ((bool)_grdDestDocuments.Rows[i].Cells[0].FormattedValue)
                    {
                        _grdDestDocuments.Rows.RemoveAt(i);
                    }
                }

                this._btnMoveDestToSour.Enabled = false;

                if (_grdDestDocuments.Rows.Count <= 0)
                {
                    this._btnCopyDoc.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
                MessageBox.Show(FgmcDocumentCopyConstants.TechnicalError, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void BtnCopyDoc_Click(object sender, EventArgs e)
        {
            int _size = 1;
            string CopyDocConfirmMessage = String.Empty;
            try
            {
                WriteLog("Copy documents to destination loan process started.", null);
                foreach (System.Windows.Forms.DataGridViewRow row in _grdDestDocuments.Rows)
                {
                    _size += Convert.ToInt32(row.Cells[FgmcDocumentCopyConstants.GrdColumnFileSize].Value);

                }

                int TimeRequiedtocopy = (_size / 100);

                if (TimeRequiedtocopy >= 60)
                {
                    TimeRequiedtocopy = TimeRequiedtocopy / 60;
                    CopyDocConfirmMessage = "This may take approximately " + TimeRequiedtocopy +
                                                  " minutes and you can continue working while the documents are copied.";
                }
                else
                {
                    if (TimeRequiedtocopy == 0)
                    {
                        TimeRequiedtocopy = 1;
                    }
                    CopyDocConfirmMessage = "This may take approximately " + TimeRequiedtocopy +
                                                  " seconds and you can continue working while the documents are copied.";
                }



                DialogResult dialogResult = MessageBox.Show(CopyDocConfirmMessage, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.OK)
                {
                    EnabledControlsWhileCopy(false);
                    this._btnCopyDoc.Enabled = false;
                    this._btnCancel.Enabled = true;
                    List<AttachmentTemplate> copyAttachmentList = new List<AttachmentTemplate>();

                    copyAttachmentList.Clear();

                    foreach (System.Windows.Forms.DataGridViewRow row in _grdDestDocuments.Rows)
                    {
                        foreach (AttachmentTemplate attach in _attachmentList)
                        {
                            if (attach.Name == Convert.ToString(row.Cells[FgmcDocumentCopyConstants.GrdColumnSourceFileName].Value))
                            {
                                attach.Title = Convert.ToString(row.Cells[FgmcDocumentCopyConstants.GrdColumnDocument].Value);
                                copyAttachmentList.Add(attach);
                                break;
                            }
                        }

                    }

                    _bw = new BackgroundWorker();
                    _bw.DoWork += Bw_DoWork;
                    _bw.RunWorkerCompleted += Bw_RunWorkerCompleted;
                    _bw.ProgressChanged += Bw_ProgressChanged;
                    _bw.WorkerSupportsCancellation = true;
                    _bw.WorkerReportsProgress = true;
                    _bw.RunWorkerAsync(copyAttachmentList);
                }

            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
                MessageBox.Show(FgmcDocumentCopyConstants.TechnicalError, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void Bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _docCopyProgressBar.Value = e.ProgressPercentage;
            _lblProgressPercentage.Text = Convert.ToString(e.ProgressPercentage) + FgmcDocumentCopyConstants.Percent;
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (e.Cancelled)
                {
                    MessageBox.Show(FgmcDocumentCopyConstants.Cancelledsucessfully + _counter);

                }
                else if (e.Error != null)
                {
                    MessageBox.Show(e.Error.Message);
                }
                else
                {

                    MessageBox.Show(FgmcDocumentCopyConstants.Copiedsucessfully + _counter);
                    _grdDestDocuments.Rows.Clear();
                    _grdDestDocuments.Refresh();
                    _docCopyProgressBar.Value = 0;
                    _lblProgressPercentage.Text = string.Empty;


                }

                EnabledControlsWhileCopy(true);
                Enabling_btnMoveSourceToDest();
                this._btnMoveDestToSour.Enabled = false;
                this._btnCancel.Enabled = false;

                foreach (var fileTitle in fileTitleList)
                {
                    UpdateTrackerDetails(fileTitle);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }
        int _counter;

        List<string> fileTitleList = new List<string>();

        private void Bw_DoWork(object sender, DoWorkEventArgs e)
        {
            int progressPercentage = 0;

            _counter = 0;
            try
            {
                List<AttachmentTemplate> copyAttachmentList = (List<AttachmentTemplate>)e.Argument;

                int offset = 100 / copyAttachmentList.Count;
                fileTitleList.Clear();
                foreach (AttachmentTemplate copyAttachment in copyAttachmentList)
                {
                    if (_bw.CancellationPending)
                    {
                        e.Cancel = true;
                        _bw.ReportProgress(0);
                        return;
                    }

                    EllieMae.Encompass.BusinessObjects.DataObject objDataObject = new EllieMae.Encompass.BusinessObjects.DataObject();
                    objDataObject.Load(copyAttachment.Data);
                    Attachment att = null;
                    if (GetFileExtension(copyAttachment.Title) == FgmcDocumentCopyConstants.Filejpg || GetFileExtension(copyAttachment.Title) == FgmcDocumentCopyConstants.Filejpeg || GetFileExtension(copyAttachment.Title) == FgmcDocumentCopyConstants.Filetif || GetFileExtension(copyAttachment.Title) == FgmcDocumentCopyConstants.Filejpe)
                    {
                        att = Loan.Attachments.AddObjectImage(objDataObject, GetFileExtension(copyAttachment.Title));
                    }
                    else
                    {
                        att = Loan.Attachments.AddObject(objDataObject, GetFileExtension(copyAttachment.Title));
                    }

                    if (att != null)
                    {

                        att.Title = _loanNumber + FgmcDocumentCopyConstants.LowLine + copyAttachment.Title;
                        _counter++;
                        progressPercentage += offset;
                        _bw.ReportProgress(progressPercentage);
                        fileTitleList.Add(copyAttachment.Title);

                    }


                }

                _bw.ReportProgress(100);
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
                MessageBox.Show(FgmcDocumentCopyConstants.TechnicalError, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }

        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            try
            {
                DialogResult result = MessageBox.Show(FgmcDocumentCopyConstants.CancelMessage, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    EnabledControlsWhileCopy(true);


                    if (_bw != null)
                    {
                        if (_bw.IsBusy)
                        {
                            _bw.CancelAsync();
                        }

                        this._btnCopyDoc.Enabled = true;
                        this._btnCancel.Enabled = false;
                    }

                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        private void ApplyingFilterForSourceGrid()
        {
            WriteLog("ApplyingFilterForSourceGrid Started", null);
            _grdSourceDocuments.DataSource = null;
            IEnumerable<DocumentFile> filtedDocuments;
            try
            {
                if (!string.IsNullOrEmpty(_txtSearchDoc.Text))
                {
                    filtedDocuments = from file in _grdattachmentList
                                      where (file.Document.ToLower().Contains(_txtSearchDoc.Text.ToLower()))
                                      select file;

                }
                else
                {
                    filtedDocuments = _grdattachmentList;
                }

                if (_chkCurrentVer.Checked)
                {
                    filtedDocuments = from file in filtedDocuments
                                      where (file.IsCurrentVersion == true)
                                      select file;
                }

                var bindingList = new BindingList<DocumentFile>(filtedDocuments.ToList());
                var source = new System.Windows.Forms.BindingSource(bindingList, null);
                _grdSourceDocuments.AllowUserToAddRows = false;
                _grdSourceDocuments.DataSource = source;
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnDocument].ReadOnly = true;
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnCurrentVersion].ReadOnly = true;
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnSourceFileName].Visible = false;
                int grdwidth = _grdSourceDocuments.Width;
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnSelect].Width = Convert.ToInt32((0.10 * grdwidth));
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnCurrentVersion].Width = Convert.ToInt32((0.10 * grdwidth));
                _grdSourceDocuments.Columns[FgmcDocumentCopyConstants.GrdColumnFileSize].Width = Convert.ToInt32((0.10 * grdwidth));
                _chkSelectAll.Checked = false;

                foreach (System.Windows.Forms.DataGridViewRow row in _grdSourceDocuments.Rows)
                {
                    row.Cells[FgmcDocumentCopyConstants.GrdColumnSelect].Value = false;
                }
                this._btnMoveSourToDest.Enabled = false;
            }

            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
                MessageBox.Show(FgmcDocumentCopyConstants.TechnicalError, FgmcDocumentCopyConstants.AppName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

        }

        private void GridOldLoanDetails_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (this._gridOldLoanDetails.IsCurrentCellDirty)
            {
                _gridOldLoanDetails.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void GridOldLoanDetails_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if ((sender as DataGridView).CurrentCell is DataGridViewCheckBoxCell)
                {
                    if (Convert.ToBoolean(((sender as DataGridView).CurrentCell as DataGridViewCheckBoxCell).Value))
                    {
                        foreach (DataGridViewRow row in (sender as DataGridView).Rows)
                        {
                            if (row.Index != (sender as DataGridView).CurrentCell.RowIndex && Convert.ToBoolean(row.Cells[e.ColumnIndex].Value) == true)
                            {
                                row.Cells[e.ColumnIndex].Value = false;
                            }
                        }
                    }
                }

                Enabling_btnGetDoc();
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        private void Enabling_btnGetDoc()
        {
            var list = (from DataGridViewRow row in _gridOldLoanDetails.Rows
                        where (Convert.ToBoolean(row.Cells[FgmcDocumentCopyConstants.GrdColumnchkcol].Value) == true)
                        select row).FirstOrDefault();
            if (list != null)
            {
                this._btnGetDoc.Enabled = true;
                ResetControls();
            }
            else
            {
                this._btnGetDoc.Enabled = false;
                ResetControls();

            }

        }

        private void GrdDestDocuments_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (this._grdDestDocuments.IsCurrentCellDirty)
                {
                    _grdDestDocuments.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        private void GrdSourceDocuments_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            try
            {
                if (this._grdSourceDocuments.IsCurrentCellDirty)
                {
                    _grdSourceDocuments.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        private void GrdDestDocuments_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Enabling_btnMoveDestToSource();
        }

        private void GrdSourceDocuments_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            Enabling_btnMoveSourceToDest();
        }

        private void Enabling_btnMoveSourceToDest()
        {
            try
            {
                var list = (from DataGridViewRow row in _grdSourceDocuments.Rows
                            where (Convert.ToBoolean(row.Cells[FgmcDocumentCopyConstants.GrdColumnSelect].Value) == true)
                            select row).FirstOrDefault();
                if (list != null)
                {
                    this._btnMoveSourToDest.Enabled = true;
                }
                else
                {
                    this._btnMoveSourToDest.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        private void Enabling_btnMoveDestToSource()
        {
            try
            {
                var list = (from DataGridViewRow row in _grdDestDocuments.Rows
                            where (Convert.ToBoolean(row.Cells[FgmcDocumentCopyConstants.GrdColumnSelect].Value) == true)
                            select row).FirstOrDefault();
                if (list != null)
                {
                    this._btnMoveDestToSour.Enabled = true;
                }
                else
                {
                    this._btnMoveDestToSour.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }


        }

        public string GetFileExtension(string fileName)
        {
            string ext = string.Empty;
            int fileExtPos = fileName.LastIndexOf(FgmcDocumentCopyConstants.FullStop, StringComparison.Ordinal);
            if (fileExtPos >= 0)
                ext = fileName.Substring(fileExtPos, fileName.Length - fileExtPos);

            return ext;
        }

        public string GetFileNameWithoutExt(string fileName)
        {

            int fileExtPos = fileName.LastIndexOf(FgmcDocumentCopyConstants.FullStop);
            if (fileExtPos >= 0)
                fileName = fileName.Substring(0, fileExtPos);

            return fileName;
        }

        private void ResetControls()
        {
            _grdSourceDocuments.DataSource = null;
            _grdattachmentList.Clear();
            _chkSelectAll.Checked = false;
            _chkSelectAll.Enabled = false;
            _txtSearchDoc.Text = string.Empty;
            _txtSearchDoc.Enabled = false;
            _btnSearchDoc.Enabled = false;
            _chkCurrentVer.Checked = false;
            _chkCurrentVer.Enabled = false;
            _grdDestDocuments.DataSource = null;
            _btnCopyDoc.Enabled = false;
            _btnCancel.Enabled = false;
            _btnMoveSourToDest.Enabled = false;
            _btnMoveDestToSour.Enabled = false;
            _btnClear.Enabled = false;
        }

        #endregion

        private void BorrowerSSNTxt_FocusOut(object sender, EventArgs e)
        {
            try
            {
                string borrowerSsnValue = Convert.ToString(_borrowerSsnTxt.Text);
                if (System.Text.RegularExpressions.Regex.IsMatch(borrowerSsnValue, FgmcDocumentCopyConstants.BssnRegexForNoOnly))
                {
                    Macro.Alert(FgmcDocumentCopyConstants.InvalidBorrowerSsn);
                    borrowerSsnValue.Remove(borrowerSsnValue.Length - 1);
                    _borrowerSsnTxt.Text = String.Empty;
                    return;
                }
                if (borrowerSsnValue.Length != 9 && !String.IsNullOrWhiteSpace(_borrowerSsnTxt.Text) && _borrowerSsnTxt.Text != String.Empty)
                {
                    Macro.Alert(FgmcDocumentCopyConstants.BssnAllowedCharacters);
                    _borrowerSsnTxt.Text = String.Empty;
                    return;
                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        private void ServerLaonTxt_FocusOut(object sender, EventArgs e)
        {
            try
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(_serverLaonTxt.Text, FgmcDocumentCopyConstants.ServerLoanNoRegexAlphaNumOnly))
                {
                    Macro.Alert(FgmcDocumentCopyConstants.InValidServicerNo);
                    _serverLaonTxt.Text.Remove(_serverLaonTxt.Text.Length - 1);
                    _serverLaonTxt.Text = String.Empty;
                    return;
                }

                if (_serverLaonTxt.Text.Length > 15)
                {
                    Macro.Alert(FgmcDocumentCopyConstants.MaxCharsForLoanAndServicerNo);
                    _serverLaonTxt.Text = String.Empty;
                    return;

                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        private void OldLoanTxt_FocusOut(object sender, EventArgs e)
        {
            try
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(_oldLoanTxt.Text, FgmcDocumentCopyConstants.ServerLoanNoRegexAlphaNumOnly))
                {
                    Macro.Alert(FgmcDocumentCopyConstants.InValidLoanNo);
                    _oldLoanTxt.Text.Remove(_oldLoanTxt.Text.Length - 1);
                    _oldLoanTxt.Text = String.Empty;
                    return;
                }
                if (_oldLoanTxt.Text.Length > 15)
                {
                    Macro.Alert(FgmcDocumentCopyConstants.MaxCharsForLoanAndServicerNo);
                    _oldLoanTxt.Text = String.Empty;
                    return;

                }
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
            }

        }

        private void BtnSearch_Click(object sender, EventArgs e)
        {
            WriteLog("Seraching Loan Process Started.", null);
            StringCipher stringCipher = new StringCipher();
            if (Directory.Exists(Path.Combine(EllieMae.Encompass.Client.Session.EncompassDataDirectory, "session.txt")))
            {
                File.AppendAllText(Path.Combine(EllieMae.Encompass.Client.Session.EncompassDataDirectory, "session.txt"), Environment.NewLine + EncompassApplication.CurrentLoan.LoanNumber.ToString());
            }

            this._btnSearch.Enabled = false;
            EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value = "false";

            Cursor.Current = Cursors.WaitCursor;
            _resultantLoans.Clear();
            string oldLoanNumber = Convert.ToString(_oldLoanTxt.Text);
            string borrowerSsn = Convert.ToString(_borrowerSsnTxt.Text);
            string oldServicerLoanNumber = Convert.ToString(_serverLaonTxt.Text);

            try
            {
                if (!string.IsNullOrEmpty(oldLoanNumber) && Loan.LoanNumber == oldLoanNumber)
                {
                    Macro.Alert(FgmcDocumentCopyConstants.SameOldLoanNo);
                    this._btnSearch.Enabled = true;
                    EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value = "true";
                    return;
                }

                if (!string.IsNullOrEmpty(borrowerSsn))   // Newly changed
                {
                    if (Convert.ToString(Loan.Fields[FgmcDocumentCopyConstants.BorrowerSsnFieldId].Value) != borrowerSsn)
                    {
                        if (Convert.ToString(Loan.Fields[FgmcDocumentCopyConstants.CoborrowerSsnFieldId].Value) != borrowerSsn)
                        {
                            Macro.Alert(FgmcDocumentCopyConstants.SameOldBorrowerSsn);
                            this._btnSearch.Enabled = true;
                            EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value = "true";
                            return;
                        }
                    }

                }

                if (!string.IsNullOrEmpty(oldServicerLoanNumber) && Convert.ToString(Loan.Fields[FgmcDocumentCopyConstants.OldServicerLoanNoFieldId].Value) == oldServicerLoanNumber)
                {
                    Macro.Alert(FgmcDocumentCopyConstants.OldServicerLoanNo);
                    this._btnSearch.Enabled = true;
                    EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value = "true";
                    return;
                }


                // Using Loan Number
                if (string.IsNullOrEmpty(oldLoanNumber) &&
                    string.IsNullOrEmpty(borrowerSsn) &&
                    string.IsNullOrEmpty(oldServicerLoanNumber))
                {
                    Macro.Alert(FgmcDocumentCopyConstants.EmptyMessage);
                    this._btnSearch.Enabled = true;
                    EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value = "true";
                    return;
                }

                EllieMae.Encompass.Client.Session session = EncompassApplication.Session;


                #region SearchCriteria
                WriteLog("searchCriteria object building.", null);
                SearchCriteria searchCriteria = new SearchCriteria();

                searchCriteria.LoanCriteria = string.Join(FgmcDocumentCopyConstants.Comma, stringCipher.Encrypt(oldLoanNumber), stringCipher.Encrypt(oldServicerLoanNumber), stringCipher.Encrypt(borrowerSsn));

                CurrentLoanInfo loanInfo = new CurrentLoanInfo();
                loanInfo.BorrowerSsn = Convert.ToString(Loan.Fields[FgmcDocumentCopyConstants.BorrowerSsnFieldId].Value);
                loanInfo.OldLoanGuid = Convert.ToString(Loan.Guid);
                searchCriteria.CurrentLoanInfo = loanInfo;

                #endregion

                #region Server Endpoint
                WriteLog("Loading Endpoint.", null);
                EllieMae.Encompass.BusinessObjects.DataObject objEndPoint = _loanHelper.GetCustomDataObject(FgmcDocumentCopyConstants.XmlServerEndpoint, session);
                XmlDocument xmlendpoint = new XmlDocument();
                MemoryStream msEndPoint = new MemoryStream(objEndPoint.Data);
                xmlendpoint.Load(msEndPoint);
                string serverEndPoint = GetNodeValue(xmlendpoint, FgmcDocumentCopyConstants.ServerEndPoint);


                #endregion

                GenericXmlSerializer<SearchCriteria> genericXmlSerializer = new GenericXmlSerializer<SearchCriteria>();
                string searchParameter = genericXmlSerializer.SerializeObject(searchCriteria);

                string url = serverEndPoint + FgmcDocumentCopyConstants.GetLoan + searchParameter;

                HttpCustomClient httpclient = new HttpCustomClient();
                WriteLog("Calling Server mathod to get loans.", null);
                byte[] data = httpclient.Post(url, searchParameter);

                System.Text.Encoding encoder = System.Text.Encoding.UTF8;
                string datastr = encoder.GetString(data);

                GenericXmlSerializer<List<LoanTemplate>> genericXmlSerializer1 = new GenericXmlSerializer<List<LoanTemplate>>();
                _loanTemplate = genericXmlSerializer1.DeserializeXml(datastr);
                if (_loanTemplate.Count <= 0)
                {
                    Macro.Alert(FgmcDocumentCopyConstants.GenericMessage);
                    this._btnSearch.Enabled = true;
                    EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value = "true";
                    return;
                }
                if (!CheckErrorMessage(_loanTemplate))
                {
                    this._btnSearch.Enabled = true;
                    EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value = "true";
                    return;

                }
                BindLoanDataToGrid(_gridOldLoanDetails, _loanTemplate);
            }
            catch (Exception ex)
            {
                WriteLog(string.Empty, ex);
                Macro.Alert(FgmcDocumentCopyConstants.TechnicalError);
                this._btnSearch.Enabled = true;
                EncompassApplication.CurrentLoan.Fields["CX.DOCCOPYSEARCHVISIBILITY"].Value = "true";
            }

        }

        private bool CheckErrorMessage(List<LoanTemplate> loanTemplates)
        {
            var loans = from loan in loanTemplates
                        where (loan != null && loan.LoanError != null && !string.IsNullOrEmpty(loan.LoanError.Message))
                        select loan;

            if (loans != null && loans.Count() > 0)
            {
                Macro.Alert(loans.ToList()[0].LoanError.Message);
                return false;
            }
            return true;

        }

        private void BindLoanDataToGrid(DataGridView gridOldLoanDetails, List<LoanTemplate> loanDetails)
        {
            StringCipher stringCipher = new StringCipher();
            System.Windows.Forms.DataGridViewCheckBoxColumn newColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            newColumn.Name = FgmcDocumentCopyConstants.GrdColumnchkcol;
            newColumn.HeaderText = FgmcDocumentCopyConstants.GrdColumnSelect;
            newColumn.ReadOnly = false;
            if (!gridOldLoanDetails.Columns.Contains(FgmcDocumentCopyConstants.GrdColumnchkcol))
                gridOldLoanDetails.Columns.Add(newColumn);

            foreach (var item in loanDetails)
            {
                item.LoanId = stringCipher.Decrypt(item.LoanId);
                item.Ssn = stringCipher.Decrypt(item.Ssn);
                item.ServicerLoan = stringCipher.Decrypt(item.ServicerLoan);
            }
            gridOldLoanDetails.DataSource = loanDetails;

            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.GrdColumnloanError].Visible = false;   // hide last column
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.LoanId].ReadOnly = true;
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.Name].ReadOnly = true;
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.Address].ReadOnly = true;
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.Ssn].ReadOnly = true;
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.ServicerLoan].ReadOnly = true;
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.CoBorrowerName].ReadOnly = true;
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.CoBorrowerSsn].ReadOnly = true;
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.CurrentStatus].ReadOnly = true;
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.StatusDate].ReadOnly = true;
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.MileStoneStatus].ReadOnly = true;
            gridOldLoanDetails.Columns[FgmcDocumentCopyConstants.GrdColumnchkcol].Width = 50;


            if (!_gridFrm.IsDisposed)
            {
                _gridFrm.TopMost = true;
                _gridFrm.Show();
                _gridFrm.TopMost = false;

            }

            else
            {
                Macro.Alert(FgmcDocumentCopyConstants.FormRefresh);
            }
        }

        private void UpdateTrackerDetails(string attachmentTitle)
        {
            try
            {
                TrackerHeader trackerHeader = new TrackerHeader();
                List<TrackerHeader> trackerHeaderList = new List<TrackerHeader>();
                string loanNum = _loanNumber;
                string searchedBy = EncompassApplication.CurrentUser.FullName;
                TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById(FgmcDocumentCopyConstants.EasternStandardTime);
                DateTime strTodaysdate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);
                string searchDate = strTodaysdate.ToShortDateString();
                DateTime easternNow = System.TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById(FgmcDocumentCopyConstants.EasternStandardTime));
                string strTime = String.Format(easternNow.ToString(), FgmcDocumentCopyConstants.TimePartFormat);
                DateTime timePart = DateTime.Parse(strTime);
                string srchdTime = timePart.ToString(FgmcDocumentCopyConstants.TimePartFormat);
                trackerHeader.LoanNumber = loanNum;
                trackerHeader.SearchedBy = searchedBy;
                trackerHeader.SearchedDate = searchDate;
                trackerHeader.SearchedTime = srchdTime;
                trackerHeader.DocsCopyStatus = attachmentTitle + "_Copied.";


                string result = trackerHeader.LoanNumber + FgmcDocumentCopyConstants.PipeCharacter +
                                trackerHeader.SearchedBy + FgmcDocumentCopyConstants.PipeCharacter +
                                trackerHeader.SearchedDate + FgmcDocumentCopyConstants.PipeCharacter +
                                trackerHeader.SearchedTime
                                + FgmcDocumentCopyConstants.PipeCharacter + trackerHeader.DocsCopyStatus;


                string oldTrackerData = Convert.ToString(EncompassApplication.CurrentLoan.Fields[FgmcDocumentCopyConstants.TrackerHistoryFieldId].Value);

                if (oldTrackerData.Contains(FgmcDocumentCopyConstants.TrackerHeader))
                {

                    EncompassApplication.CurrentLoan.Fields["CX.COPYDOC.TRACKER"].Value += Environment.NewLine + string.Join(Environment.NewLine, result);
                }
                else
                {

                    EncompassApplication.CurrentLoan.Fields["CX.COPYDOC.TRACKER"].Value = FgmcDocumentCopyConstants.TrackerHeader + Environment.NewLine + FgmcDocumentCopyConstants.NewLine + string.Join(Environment.NewLine, result);
                }
            }
            catch (Exception ex)
            {

                WriteLog(string.Empty, ex);
                Macro.Alert(FgmcDocumentCopyConstants.TechnicalError);
                return;
            }



        }

        private void EnabledControlsWhileCopy(bool isDissable)
        {
            this._gridOldLoanDetails.Enabled = isDissable;
            this._btnGetDoc.Enabled = isDissable;
            this._grdSourceDocuments.Enabled = isDissable;
            this._grdDestDocuments.Enabled = isDissable;
            this._btnMoveSourToDest.Enabled = isDissable;
            this._btnMoveDestToSour.Enabled = isDissable;
            this._chkSelectAll.Enabled = isDissable;
            this._chkCurrentVer.Enabled = isDissable;
            this._txtSearchDoc.Enabled = isDissable;
            this._btnSearchDoc.Enabled = isDissable;
            this._btnClear.Enabled = isDissable;
        }
        /// <summary>
        /// logging program exicution info
        /// </summary>
        /// <param name="info">To give information about exicuting part.</param>
        /// <param name="exception">To log exception deatils</param>
        private void WriteLog(string info, Exception exception)
        {

            StreamWriter sw = null;
            string directorypath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FGMCCustomLogs");
            System.IO.Directory.CreateDirectory(directorypath);
            string filepath = Path.Combine(directorypath, "DocCopy.log");
            try
            {
                sw = File.AppendText(filepath);
                if (!string.IsNullOrEmpty(info))
                {
                    sw.WriteLine(DateTime.Now + " : " + info);
                }
                if (exception != null)
                {
                    sw.WriteLine(exception.Message);
                    if (exception.InnerException != null)
                    {
                        sw.WriteLine(DateTime.Now + " : " + exception.InnerException.Message);
                    }
                    if (!string.IsNullOrEmpty(exception.StackTrace))
                    {
                        sw.WriteLine(DateTime.Now + " : " + exception.StackTrace);
                    }
                }
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {

            }
        }
    }
}
