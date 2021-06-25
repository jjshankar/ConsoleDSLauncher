using ECAR.DocuSign.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace ECAR.DocuSign
{
    /// <summary>
    /// Process event notifications received by the webhook method
    /// </summary>
    public static class CallBack
    {
        /// <summary>
        /// Process event notifications received from DocuSign
        /// </summary>
        /// <param name="jsonPayload">JSON event payload received from DocuSign</param>
        /// <returns>EnvelopeModel object populated with data from DocuSign</returns>
        public static EnvelopeModel Process(string jsonPayload)
        {
            JToken root = JObject.Parse(jsonPayload);
            EnvelopeModel envelopeModel = new EnvelopeModel
            {
                Status = (string)(root.SelectToken("status") ?? ""),
                CompletedDateTime = (string)(root.SelectToken("completedDateTime") ?? ""),
                LastModifiedDateTime = (string)(root.SelectToken("lastModifiedDateTime") ?? ""),
                EmailSubject = (string)(root.SelectToken("emailSubject") ?? ""),
                ExpireEnabled = bool.Parse((string)(root.SelectToken("expireEnabled") ?? "false")),
                ExpireDateTime = (string)(root.SelectToken("expireDateTime") ?? ""),
                ExpireAfter = (int)(root.SelectToken("expireAfter") ?? 0),
                VoidedDateTime = (string)(root.SelectToken("voidedDateTime") ?? ""),
                StatusChangedDateTime = (string)(root.SelectToken("statusChangedDateTime") ?? ""),
                SentDateTime = (string)(root.SelectToken("sentDateTime") ?? ""),
                EnvelopeId = (string)(root.SelectToken("envelopeId") ?? ""),
                VoidedReason = (string)(root.SelectToken("voidedReason") ?? ""),
                CreatedDateTime = (string)(root.SelectToken("createdDateTime") ?? ""),
                DeclinedDateTime = (string)(root.SelectToken("declinedDateTime") ?? ""),
                DeliveredDateTime = (string)(root.SelectToken("deliveredDateTime") ?? ""),
                EmailBlurb = ""
            };
            
            return envelopeModel;

        }
    }
}
