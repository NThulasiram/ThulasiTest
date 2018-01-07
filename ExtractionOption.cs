using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EncompassLibrary.LoanDataExtractor
{
    /// <summary>
    /// provides filter options for attachment extraction
    /// </summary>
    public class ExtractionOption
    {
        List<string> _placeholders = new List<string>();
        List<string> _skipedPlaceholders = new List<string>();
        List<string> _placeholdersStartWith = new List<string>();
        List<string> _attachmentNames = new List<string>();
        List<string> _skipedAttachmentNames = new List<string>();
        List<string> _attachmentNamesStartWith = new List<string>();
        List<string> _allowedExtensions = new List<string>();
        bool _isCurrentVersion = false;
        public ExtractionOption()
        {

        }
        public ExtractionOption(List<string> placeholders, List<string> skipedPlaceholders, List<string> placeholdersStartWith, List<string> attachmentNames,
            List<string> skipedAttachmentNames, List<string> attachmentNamesStartWith, List<string> allowedExtensions, bool isCurrentVersion)
        {
            _placeholders = placeholders;
            _skipedPlaceholders = skipedPlaceholders;
            _placeholdersStartWith = placeholdersStartWith;
            _attachmentNames = attachmentNames;
            _skipedAttachmentNames = skipedAttachmentNames;
            _attachmentNamesStartWith = attachmentNamesStartWith;
            _allowedExtensions = allowedExtensions;
            _isCurrentVersion = isCurrentVersion;
        }
        /// <summary>
        /// Download attachments from these placeholders only
        /// </summary>
        public List<string> Placeholders { get; set; }

        /// <summary>
        /// Skip any attachment download from these placeholders
        /// </summary>
        public List<string> SkipedPlaceholders { get; set; }

        /// <summary>
        /// List of Placeholders starts with. e.g. all placeholders starting from Assets and Statement 
        /// </summary>
        public List<string> PlaceholdersStartWith { get; set; }


        //Attachment related properties
        public List<string> AttachmentNames { get; set; }
        public List<string> SkipedAttachmentNames { get; set; }
        public List<string> AttachmentNamesStartWith { get; set; }

        /// <summary>
        /// Only download files of these extensions
        /// </summary>
        public List<string> AllowedExtensions { get; set; }

        /// <summary>
        /// When set to True, it downloads only Current Version Files
        /// </summary>
        public bool IsCurrentVersion { get; set; }

    }
}
