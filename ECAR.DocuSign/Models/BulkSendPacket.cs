using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign.Models
{
    /// <summary>
    /// Data model for sending multiple documents sent for signature to many recipients in a batch via ECAR.DocuSign.    
    /// </summary>
    public class BulkSendPacketList           // ===> BulkSendingList (parent object)
    {
        /// <summary>
        /// List of documents to send in a batch.
        /// </summary>
        public List<BulkSendPacketRecipientModel> BulkPacketRecipientList { get; set; }

        /// <summary>
        /// Friendly name/ID for the list/batch.
        /// </summary>
        public string BulkBatchName { get; set; }

        /// <summary>
        /// Subject text for the bulk batch emails
        /// </summary>
        public string BulkEmailSubject { get; set; }

        /// <summary>
        /// Body text for the bulk batch emails
        /// </summary>
        public string BulkEmailBody { get; set; }

        /// <summary>
        /// DocuSign template to use for the batch.
        /// </summary>
        public List<string> DSBatchPacketTemplates { get; set; }

        /// <summary>
        /// The ID of the list included in this batch returned by DocuSign.
        /// </summary>
        public string DSListId { get; internal set; }

        /// <summary>
        /// The ID of this batch returned by DocuSign.
        /// </summary>
        public string DSBatchId { get; internal set; }

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
        public BulkSendPacketList()
        {
            DSStampEnvelopeId = true;
            DSAllowReassign = true;
            DSAllowPrintAndSign = true;
        }
    }

    /// <summary>
    /// Construct for batch/bulk sending doument packets for DocuSign signature.
    /// </summary>
    public class BulkSendPacketRecipientModel       // ===> BulkSendingCopy object
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
        /// List of preset fields for this document.
        /// </summary>
        public List<DocPreset> Presets { get; set; }

        /// <summary>
        /// Show/hide the EnvelopeID in the document sent through DocuSign (default = true).
        /// *** Requires a corresponding setting in DocuSign admin to be set; ignored otherwise. ***
        /// </summary>
        public bool DSStampEnvelopeID { get; set; }

        /// <summary>
        /// Allow/block recipients from reassigning envelopes sent to them (default = true).
        /// </summary>
        public bool DSAllowReassign { get; set; }

        /// <summary>
        /// Custom email subject (optional) for the batch. 
        /// *Will be overridden if DSEmailSubject is set in the DocumentModel object.*
        /// </summary>
        public string CustomEmailSubject { get; set; }

        /// <summary>
        /// Custom email body (optional) for the batch. 
        /// </summary>
        public string CustomEmailBody { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BulkSendPacketRecipientModel()
        {
            DSStampEnvelopeID = true;
            DSAllowReassign = true;
        }
    }
}
