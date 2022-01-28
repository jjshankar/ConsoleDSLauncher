using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign.Models
{

    /// <summary>
    /// Envelope Status retrieved from DocuSign.
    /// </summary>
    public class EnvelopeStatus
    {
        /// <summary>
        /// EnvelopeStatus = "any"
        /// </summary>
        public static string STATUS_ANY { get { return "any"; } }

        /// <summary>
        /// EnvelopeStatus = "created"
        /// </summary>
        public static string STATUS_CREATED { get { return "created"; } }

        /// <summary>
        /// EnvelopeStatus = "completed"
        /// </summary>
        public static string STATUS_COMPLETED { get { return "completed"; } }

        /// <summary>
        /// EnvelopeStatus = "declined"
        /// </summary>
        public static string STATUS_DECLINED { get { return "declined"; } }

        /// <summary>
        /// EnvelopeStatus = "deleted"
        /// </summary>
        public static string STATUS_DELETED { get { return "deleted"; } }

        /// <summary>
        /// EnvelopeStatus = "delivered"
        /// </summary>
        public static string STATUS_DELIVERED { get { return "delivered"; } }

        /// <summary>
        /// EnvelopeStatus = "draft"
        /// </summary>
        public static string STATUS_DRAFT { get { return "draft"; } }

        /// <summary>
        /// EnvelopeStatus = "sent"
        /// </summary>
        public static string STATUS_SENT { get { return "sent"; } }

        /// <summary>
        /// EnvelopeStatus = "signed"
        /// </summary>
        public static string STATUS_SIGNED { get { return "signed"; } }

        /// <summary>
        /// EnvelopeStatus = "timedout"
        /// </summary>
        public static string STATUS_TIMEDOUT { get { return "timedout"; } }

        /// <summary>
        /// EnvelopeStatus = "voided"
        /// </summary>
        public static string STATUS_VOIDED { get { return "voided"; } }
    }

    /// <summary>
    /// Data model for envelopes retrieved from DocuSign.
    /// </summary>
    public class EnvelopeModel
    {
        /// <summary>
        /// The date and time the item was last modified (UTC/ISO).
        /// </summary>
        public string LastModifiedDateTime { get; internal set; }

        /// <summary>
        /// If the envelope is set to expire
        /// </summary>
        public bool ExpireEnabled { get; internal set; }

        /// <summary>
        /// The expiration DateTime, if exists (UTC/ISO).
        /// </summary>
        public string ExpireDateTime { get; internal set; }

        /// <summary>
        /// Expiration ticks in minutes.
        /// </summary>
        public int ExpireAfter { get; internal set; }

        /// <summary>
        /// The date and time the envelope or template was voided, if exists (UTC/ISO).
        /// </summary>
        public string VoidedDateTime { get; internal set; }

        /// <summary>
        /// The date and time the status changed (UTC/ISO).
        /// </summary>
        public string StatusChangedDateTime { get; internal set; }

        /// <summary>
        /// Indicates the envelope's current status. 
        ///</summary>
        public string Status { get; internal set; }

        /// <summary>
        /// The date and time the envelope was sent (UTC/ISO).
        /// </summary>
        public string SentDateTime { get; internal set; }

        /// <summary>
        /// The GUID identifier for the envelope.
        /// </summary>
        public string EnvelopeId { get; internal set; }

        /// <summary>
        /// The reason the envelope or template was voided, if exists.
        /// </summary>
        public string VoidedReason { get; internal set; }

        /// <summary>
        /// Specifies the date and time this item was completed, if exists (UTC/ISO).
        /// </summary>
        public string CompletedDateTime { get; internal set; }

        /// <summary>
        /// Indicates the date and time the item was created (UTC/ISO).
        /// </summary>
        public string CreatedDateTime { get; internal set; }

        /// <summary>
        /// Specifies the subject of the email that is sent to all recipients, if exists. 
        /// </summary>
        public string EmailSubject { get; internal set; }

        /// <summary>
        /// Text that is included in email body for all envelope recipients, if exists.
        /// </summary>
        public string EmailBlurb { get; internal set; }

        /// <summary>
        /// The date and time the recipient declined the document, if exists (UTC/ISO).
        /// </summary>
        public string DeclinedDateTime { get; internal set; }

        /// <summary>
        /// The date and time the envelope was delivered (UTC/ISO).
        /// </summary>
        public string DeliveredDateTime { get; internal set; }

        /// <summary>
        /// Key Value pair of custom fields contained in this envelope.
        /// </summary>
        public Dictionary<string, string> EnvelopeCustomFields { get; internal set; }

        /// <summary>
        /// List of recipients this envelope was sent to with status and details for each.
        /// </summary>
        public List<EnvelopeRecipientModel> EnvelopeRecipients { get; internal set; }
    }
}
