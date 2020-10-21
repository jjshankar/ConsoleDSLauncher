using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign.Models
{
    /// <summary>
    /// Data model for document recipients retrieved from DocuSign.
    /// </summary>
    public class EnvelopeRecipientModel
    {
        /// <summary>
        /// Recipient ID (friendly) returned from DocuSign.
        /// </summary>
        public string RecipientId { get; set; }
        
        /// <summary>
        /// Client specific Id for the recipient (empty for emailed DocuSign documents).
        /// </summary>
        public string ClientUserId { get; set; }
        
        /// <summary>
        /// Date the recipient declined to sign (if available).
        /// </summary>
        public string DeclinedDateTime  { get; set; }

        /// <summary>
        /// Reason the recipient declined to sign (if available).
        /// </summary>
        public string DeclinedReason { get; set; }
        
        /// <summary>
        /// Date the envelope was delivered to this recipient.
        /// </summary>
        public string DeliveredDateTime { get; set; }

        /// <summary>
        /// Email address for the recipient.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Name of the recipient.
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Status of the DocuSign envelope for this recipient.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// The recipient type for this recipient (signer/witness etc.).
        /// </summary>
        public string RecipientType { get; set; }

        /// <summary>
        /// The role name for this recipient.
        /// </summary>
        public string RoleName { get; set; }
        
        /// <summary>
        /// Name provided by the recipient to DocuSign at signature ceremony.
        /// </summary>
        public string SignatureName { get; set; }
        
        /// <summary>
        /// Date and time the recipient completed the DocuSign ceremony (if available).
        /// </summary>
        public string SignedDateTime { get; set; }

        /// <summary>
        /// Recipient GUID used by DocuSign.
        /// </summary>
        public string DSUserGUID { get; set; }
    }
}
