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
        /// Show/hide the EnvelopeID in the document sent through DocuSign (default = true).
        /// *** Requires a corresponding setting in DocuSign admin to be set; ignored otherwise. ***
        /// </summary>
        public bool DSStampEnvelopeId { get; set; }

        /// <summary>
        /// Allow/block recipients from reassigning envelopes sent to them (default = true).
        /// </summary>
        public bool DSAllowReassign { get; set; }

        /// <summary>
        /// Allow/block recipients from printing and signing (wet sign) envelopes sent to them (default = true).
        /// </summary>
        public bool DSAllowPrintAndSign { get; set; }
        
        /// <summary>
        /// Default constructor. 
        /// </summary>
        public DocumentPacketModel()
        {
            DSStampEnvelopeId = true;
            DSAllowReassign = true;
            DSAllowPrintAndSign = true;
        }

    }

}
