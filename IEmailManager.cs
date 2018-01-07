using System.Collections.Generic;

namespace FGMC.SMTPLibrary
{
	public interface IEmailManager
	{
		bool SendEmail(string from, string mailTo, string subject, string body,int applicationId, List<string> attachmentFilePaths = null, NotificationType? notificationType = null);
	}


	public enum NotificationType
	{
		NoFilesCopied=0,
		NoSftpLocation=1,
		PartiallyCopied =2,
		Default=3
	}
}
