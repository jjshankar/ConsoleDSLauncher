using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign.Models
{
    /// <summary>
    /// Data model for a single document sent for signature to a single recipient via ECAR.DocuSign.
    /// </summary>
    public class DocumentModel
    {
        /// <summary>
        /// Email address for the recipient.
        /// </summary>
        public string SignerEmail { get; set; }

        /// <summary>
        /// Display name for the recipient.
        /// </summary>
        public string SignerName { get; set; }

        /// <summary>
        /// The recipient's identifier in the calling application (e.g. tracking number).
        /// </summary>
        public string SignerId { get; set; }

        /// <summary>
        /// Name of the DocuSign recipient role.
        /// </summary>
        public string DSRoleName { get; set; }

        /// <summary>
        /// Name of the DocuSign template to use.
        /// </summary>
        public string DSTemplateName { get; set; }

        /// <summary>
        /// Subject line for the email sent from DocuSign.
        /// </summary>
        public string DSEmailSubject { get; set; }

        /// <summary>
        /// Text body for the email sent from DocuSign.
        /// </summary>
        public string DSEmailBody { get; set; }

        /// <summary>
        /// Envelope ID returned from the DocuSign ceremony.
        /// </summary>
        public string DSEnvelopeId { get; internal set; }

        /// <summary>
        /// Document ID returned from the DocuSign ceremony.
        /// </summary>
        public string DSDocumentId { get; internal set; }

        /// <summary>
        /// Envelope Preview URL returned from the DocuSign.
        /// </summary>
        public string DSPreviewUrl { get; internal set; }

        /// <summary>
        /// Show/hide the EnvelopeID in the document sent through DocuSign (default = true).
        /// </summary>
        public bool DSStampEnvelopeID { get; set; }

        /// <summary>
        /// Default constructor. 
        /// </summary>
        public DocumentModel()
        {
            DSStampEnvelopeID = true;
        }

        /// <summary>
        /// Special object constructor that accepts a DocuSign EnvelopeId.
        /// </summary>
        /// <param name="envelopeId">DocuSign Envelope ID</param>
        public DocumentModel(string envelopeId)
        {
            DSEnvelopeId = envelopeId;
            DSStampEnvelopeID = true;
        }
    }

}
