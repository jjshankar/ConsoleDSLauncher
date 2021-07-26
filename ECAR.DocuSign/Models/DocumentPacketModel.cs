using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign.Models
{
    /// <summary>
    /// Data model for multiple documents sent as a packet to a single recipient for signature via ECAR.DocuSign.
    /// </summary>
    public class DocumentPacketModel
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
        /// Names of the DocuSign templates to use in this document packet.
        /// </summary>
        public List<string> DSTemplateList { get; set; }

        /// <summary>
        /// Subject line for the email sent from DocuSign.
        /// </summary>
        public string DSEmailSubject { get; set; }

        /// <summary>
        /// Body text for the email sent from DocuSign.
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
        /// Default constructor. 
        /// </summary>
        public DocumentPacketModel()
        {            
        }

    }

}
