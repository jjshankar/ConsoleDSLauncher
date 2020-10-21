using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign.Models
{
    /// <summary>
    /// Data model for documents retrieved from DocuSign.
    /// </summary>
    public class EnvelopeDocumentModel
    {
        /// <summary>
        /// Envelope ID returned from the DocuSign ceremony.
        /// </summary>
        public string EnvelopeId { get; internal set; }

        /// <summary>
        /// Document ID from DocuSign (integer ID for actual documents, 'certificate' for certificates).
        /// </summary>
        public string DocumentId { get; internal set; }

        /// <summary>
        /// Document GUID used by DocuSign.
        /// </summary>
        public string DSDocumentGUID { get; internal set; }

        /// <summary>
        /// Signature sequence order of the document (999 for certificates).
        /// </summary>
        public string Order { get; internal set; }

        /// <summary>
        /// Name of the document.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Type of the document ('content' for documents, 'summary' for certificates).
        /// </summary>
        public string Type { get; internal set; }

    }
}
