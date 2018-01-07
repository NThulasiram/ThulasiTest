using FGMC.SecurityLibrary;
using Log4NetLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Configuration;
using System.Net.Mail;
using System.Net.Mime;
using System.Reflection;
using System.Text;

namespace FGMC.SMTPLibrary
{
    public class EmailManager : IEmailManager
    {
        private readonly ILoggingService _logService;
        private LogModel _logModel;
        private StringCipher _stringCipher;
        const string APPKEY_IS_EMAIL_PASSWORD_ENCRYPTED = "IsEmailPasswordEncrypted";

        public EmailManager()
        {
            _logService = new FileLoggingService(typeof(EmailManager));
            _logModel = new LogModel();
            _stringCipher = new StringCipher();
        }
        public void SendEmailForDocExraction(string senderEmail, string mailTo, string userId, string userFullName, string batchNo, string batchDate, string savedLocation)
        {
            try
            {

                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(senderEmail);
                    message.To.Add(mailTo);
                    var exeDirectoryPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
                    exeDirectoryPath = exeDirectoryPath.Replace("bin\\Debug", "");
                    var templateName = "EmailTemplate.htm";
                    var emailHtmlFile = string.Format("EmailTemplates\\{0}", templateName);
                    var filePath = Path.Combine(exeDirectoryPath, emailHtmlFile).Replace("file:\\", string.Empty);
                    var fileContent = System.IO.File.ReadAllText(filePath);
                    SetMessageBody(userId, userFullName, batchNo, batchDate, savedLocation, message, fileContent);
                    message.Subject = ConfigurationManager.AppSettings["mailSubject"] + batchNo + " - Success";
                    message.BodyEncoding = Encoding.UTF8;
                    message.IsBodyHtml = true;
                    using (var smtpClient = new SmtpClient())
                    {
                        bool isEmailPasswordEncrypted = Convert.ToBoolean(ConfigurationManager.AppSettings[APPKEY_IS_EMAIL_PASSWORD_ENCRYPTED]);
                        if (isEmailPasswordEncrypted)
                        {
                            smtpClient.Credentials = GetNetworkCredential();
                        }
                        smtpClient.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        private void SetMessageBody(string userId, string userFullName, string batchNo, string batchDate, string savedLocation, MailMessage message, string fileContent)
        {
            message.Body = fileContent.Replace("{{UserName}}", userFullName)
                .Replace("{{BatchNumber}}", batchNo)
                .Replace("{{BatchDate}}", batchDate)
                .Replace("{{FilkeSavedIn}}", savedLocation)
                .Replace("{{UserId}}", userId);
        }


        private NetworkCredential GetNetworkCredential()
        {
            var smtpSection = (SmtpSection)ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            string username = smtpSection.Network.UserName;
            string password = smtpSection.Network.Password;
            string domain = smtpSection.Network.Host;
            bool isEmailPasswordEncrypted = Convert.ToBoolean(ConfigurationManager.AppSettings[APPKEY_IS_EMAIL_PASSWORD_ENCRYPTED]);
            if(isEmailPasswordEncrypted)
            {
                password = _stringCipher.Decrypt(password);
            }
            NetworkCredential networkCredential = new NetworkCredential(username, password, domain);
            return networkCredential;
        }
        public bool SendEmail(string from, string mailTo, string subject, string body,int applicationId, List<string> attachmentFilePaths = null, NotificationType? notificationType = null)
        {
            bool mailSuccess = false;
            try
            {
                using (var message = new MailMessage())
                {
                    message.From = new MailAddress(from);
                    if (mailTo.Contains(",") || mailTo.Contains(";"))
                    {
                        var toEmailIds = mailTo.Split(new char[] { ',', ';' });
                        foreach (var to in toEmailIds)
                        {
                            message.To.Add(to.Trim());
                        }
                    }
                    else
                    {
                        message.To.Add(mailTo);
                    }
                    message.Body = body;
                    message.Subject = subject;
                    message.BodyEncoding = Encoding.UTF8;
                    message.IsBodyHtml = true;
                    if (attachmentFilePaths != null)
                    {
                        foreach (var filePath in attachmentFilePaths)
                        {
                            Attachment data = new Attachment(filePath, MediaTypeNames.Application.Octet);
                            // Add time stamp information for the file.
                            ContentDisposition disposition = data.ContentDisposition;
                            disposition.CreationDate = System.IO.File.GetCreationTime(filePath);
                            disposition.ModificationDate = System.IO.File.GetLastWriteTime(filePath);
                            disposition.ReadDate = System.IO.File.GetLastAccessTime(filePath);
                            // Add the file attachment to this e-mail message.
                            message.Attachments.Add(data);
                        }
                    }
                    using (var smtpClient = new SmtpClient())
                    {
                        bool isEmailPasswordEncrypted = Convert.ToBoolean(ConfigurationManager.AppSettings[APPKEY_IS_EMAIL_PASSWORD_ENCRYPTED]);
                        if (isEmailPasswordEncrypted)
                        {
                            smtpClient.Credentials = GetNetworkCredential();
                        }
                        smtpClient.Send(message);
                        mailSuccess = true;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(_logService, _logModel, applicationId, ex.Message, string.Empty, ex);
                mailSuccess = false;
            }

            return mailSuccess;
        }
    }
}
