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
                EmailBlurb = (string)(root.SelectToken("emailBlurb") ?? ""),
                EnvelopeCustomFields = null
            };

            // Read customFields from the payload
            JToken customFields = root["customFields"];
            if (customFields != null)
            {
                Dictionary<string, string> dictCustomFields = new Dictionary<string, string>();

                JEnumerable<JToken> listCustomFields = customFields["listCustomFields"].Children();
                JEnumerable<JToken> textCustomFields = customFields["textCustomFields"].Children();

                foreach (JToken titem in textCustomFields)
                    dictCustomFields.Add((string)titem["name"], (string)titem["value"]);

                foreach (JToken litem in listCustomFields)
                    dictCustomFields.Add((string)litem["name"], (string)litem["value"]);

                // Return the dictionary
                envelopeModel.EnvelopeCustomFields = dictCustomFields;
            }

            // Read recipient data from the payload
            JToken recipients = root["recipients"];

            if (recipients != null)
            {
                List<EnvelopeRecipientModel> recipientList = new List<EnvelopeRecipientModel>();
                JEnumerable<JToken> signers = recipients["signers"].Children();

                foreach (JToken s in signers)
                {
                    recipientList.Add(new EnvelopeRecipientModel
                    {
                        RecipientId = (string)s["recipientId"],
                        ClientUserId = "",                                      // this will be empty for emailed documents
                        DeclinedDateTime = (string)s["declinedDateTime"],
                        DeclinedReason = (string)s["declinedReason"],
                        DeliveredDateTime = (string)s["deliveredDateTime"],
                        Email = (string)s["email"],
                        Name = (string)s["name"],
                        RecipientType = (string)s["recipientType"],
                        RoleName = (string)s["roleName"],
                        //SignatureName = (string)s.SignatureInfo?.SignatureName,
                        SignedDateTime = (string)s["signedDateTime"],
                        Status = (string)s["status"],
                        DSUserGUID = (string)s["userId"],
                    });
                }

                // Add the list to output
                envelopeModel.EnvelopeRecipients = recipientList;
            }

            return envelopeModel;

        }
    }
}
