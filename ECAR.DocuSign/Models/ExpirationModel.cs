using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign.Models
{
    /// <summary>
    /// Data model for expiration settings for documents sent via ECAR.DocuSign.
    /// </summary>
    public class ExpirationModel
    {
        /// <summary>
        /// Indicates whether expiration is enabled.
        /// </summary>
        public bool ExpirationEnabled { get; set; }

        /// <summary>
        /// Envelope will expire after this many days.
        /// </summary>
        public int ExpireAfterDays { get; set; }

        /// <summary>
        /// Expiration warning will be sent this many days before.
        /// </summary>
        public int ExpireWarnDays { get; set; }
    }
}
