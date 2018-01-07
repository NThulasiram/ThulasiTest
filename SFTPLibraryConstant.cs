using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FGMC.SFTPLibrary
{
    public  class SFTPLibraryConstant
    {
        public const string NetworkIssueMsg = "Copying files to remote FTP failed due to network issue.";
        public const string AceessDeniedMsg = "SFTP Authentication failed due to wrong userid/password.";
        public const string InvalidHostMsg = "Host 'sftp.loanlogics.com' does not exist or the site may be down temporarily.";
    }
}
