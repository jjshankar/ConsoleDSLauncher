using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign.Models
{
    /// <summary>
    /// Data model for setting up DocuSign notification to a callback/webhook method.
    /// </summary>
    public class NotificationCallBackModel
    {
        /// <summary>
        /// List of envelope event codes to get notifications for.  
        /// Valid values are: "Draft", "Sent", "Completed", "Declined", "Delivered", "Voided".
        /// </summary>
        public List<string> EnvelopeEvents;

        /// <summary>
        /// SSL URL of the public endpoint to which notifications are sent as JSON.
        /// </summary>
        public string WebHookUrl;
    }
}
