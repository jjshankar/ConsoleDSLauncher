using System;
using System.Collections.Generic;
using System.Text;

using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using ECAR.DocuSign.Models;
using ECAR.DocuSign.Properties;
using ECAR.DocuSign.Security;

namespace ECAR.DocuSign.Common
{
    internal static class Utils
    {
        #region PRIVATE_MEMBERS

        // Private cache of DocuSign template IDs to reduce API calls
        private static readonly Dictionary<string, string> __TemplateIDCache = new Dictionary<string, string>();

        #endregion

        #region INTERNAL_PUBLIC_METHODS
        /// <summary>
        /// Get Template ID from local cache or DocuSign server for use in DocuSign calls.
        /// </summary>
        /// <param name="accountId">DocuSign account ID</param>
        /// <param name="templateName">Template name to search</param>
        /// <returns>DocuSign Template ID</returns>
        public static string GetTemplateId(
            string accountId,
            string templateName)
        {
            string templateId = "";

            // Check the local cache 
            if (__TemplateIDCache.Count > 0)
                __TemplateIDCache.TryGetValue(templateName, out templateId);

            // If not found, go to DocuSign
            if (string.IsNullOrEmpty(templateId))
            {
                // Get the template ID by calling the Templates API
                TemplatesApi templatesApi = Authenticate.CreateTemplateApiClient();
                TemplatesApi.ListTemplatesOptions options = new TemplatesApi.ListTemplatesOptions { searchText = templateName };
                EnvelopeTemplateResults searchResults = templatesApi.ListTemplates(accountId, options);

                // Process results
                if (int.Parse(searchResults.ResultSetSize) > 0)
                {
                    // Found the template! Record its id
                    templateId = searchResults.EnvelopeTemplates[0].TemplateId;
                    __TemplateIDCache.Add(templateName, templateId);
                }
                else
                    throw new Exception(String.Format(Resources.TEMPLATE_NOT_FOUND_x, templateName));
            }

            // Return found ID
            return templateId;
        }

        
        /// <summary>
        /// Create the Tabs object from DocPresets for use in DocuSign calls.
        /// </summary>
        /// <param name="Presets">List of DocPreset objects</param>
        /// <returns>DocuSign Tabs object</returns>
        public static Tabs ParsePresets(
            List<DocPreset> Presets)
        {
            List<Checkbox> checkBoxTabs = null;
            List<Date> dateTabs = null;
            List<Ssn> ssnTabs = null;
            List<Text> textTabs = null;

            foreach (DocPreset t in Presets)
            {
                switch (t.Type)
                {
                    case Models.Presets.Checkbox:
                        {
                            Checkbox tab = new Checkbox { 
                                TabLabel = t.Label, 
                                Selected = t.Value, 
                                Locked = (t.Locked ? "true" : "false") 
                            };
                            if (checkBoxTabs != null)
                                checkBoxTabs.Add(tab);
                            else
                                checkBoxTabs = new List<Checkbox> { tab };
                            break;
                        }
                    case Models.Presets.Date:
                        {
                            Date tab = new Date { 
                                TabLabel = t.Label, 
                                Value = t.Value, 
                                Locked = (t.Locked ? "true" : "false") 
                            };
                            if (dateTabs != null) 
                                dateTabs.Add(tab); 
                            else
                                dateTabs = new List<Date> { tab };
                            break;
                        }
                    case Models.Presets.Ssn:
                        {
                            Ssn tab = new Ssn { 
                                TabLabel = t.Label, 
                                Value = t.Value, 
                                Locked = (t.Locked ? "true" : "false") 
                            };
                            if (ssnTabs != null)
                                ssnTabs.Add(tab);
                            else
                                ssnTabs = new List<Ssn> { tab };
                            break;
                        }
                    case Models.Presets.Text:
                        {
                            Text tab = new Text { 
                                TabLabel = t.Label, 
                                Value = t.Value, 
                                Locked = (t.Locked ? "true" : "false") 
                            };
                            if (textTabs != null)
                                textTabs.Add(tab);
                            else
                                textTabs = new List<Text> { tab };
                            break;
                        }
                }
            }

            // return processed Tabs
            return new Tabs
            {
                CheckboxTabs = checkBoxTabs,
                DateTabs = dateTabs,
                SsnTabs = ssnTabs,
                TextTabs = textTabs,
            };
        }

        /// <summary>
        /// Create the CompositeTemplate objects for use in DocuSign calls.
        /// </summary>
        /// <param name="TemplateId">DocuSign ID of the template to use</param>
        /// <param name="SeqNum">Sequence number for this template</param>
        /// <param name="Doc">DocumentModel object with recipient data (do not use for bulk send)</param>
        /// <param name="Presets">Presets for the current recipient (do not use for bulk send)</param>
        /// <returns>DocuSign CompositeTemplate object</returns>
        public static CompositeTemplate CreateCompositeTemplate(
            string TemplateId,
            string SeqNum,
            DocumentModel Doc = null,
            List<DocPreset> Presets = null)
        {
            // Create a composite template object 
            CompositeTemplate compTemplate = new CompositeTemplate
            {
                CompositeTemplateId = SeqNum
            };

            // Start with the different components of the request
            // Add the server Template
            compTemplate.ServerTemplates = new List<ServerTemplate> {
                new ServerTemplate {
                    Sequence = "1",
                    TemplateId = TemplateId    // TemplateId for the doc in DocuSign
                }
            };

            // Set up prefilled fields (if passed in) and add tabs to signer object
            if (Doc != null && Presets != null && Presets.Count > 0)
            {
                // Create the signer recipient object 
                Signer signer = new Signer
                {
                    Email = Doc.SignerEmail,
                    Name = Doc.SignerName,
                    ClientUserId = Doc.SignerId ?? null,    // For DocuSign via email, the ClientUserId must be null
                    RecipientId = "1",
                    RoleName = Doc.DSRoleName
                };

                signer.Tabs = ParsePresets(Presets);

                // Create recipients object
                Recipients recipients = new Recipients { Signers = new List<Signer> { signer } };


                // Add the roles via an inlineTemplate
                compTemplate.InlineTemplates = new List<InlineTemplate> {
                    new InlineTemplate {
                        Sequence = "2",
                        Recipients = recipients
                    }
                };
            }

            return compTemplate;
        }

        /// <summary>
        /// Create the CompositeTemplate objects for use with multi-document packets.
        /// </summary>
        /// <param name="TemplateId">DocuSign ID of the template to use</param>
        /// <param name="SeqNum">Sequence number for this template</param>
        /// <param name="Doc">DocumentPacketModel object with recipient data (do not use for bulk send)</param>
        /// <param name="Presets">Presets for the current recipient (do not use for bulk send)</param>
        /// <returns>DocuSign CompositeTemplate object</returns>
        public static CompositeTemplate CreateCompositeTemplatePacket(
            string TemplateId,
            int SeqNum,
            DocumentPacketModel Doc = null,
            List<DocPreset> Presets = null)
        {
            // Create a composite template object 
            CompositeTemplate compTemplate = new CompositeTemplate
            {
                CompositeTemplateId = SeqNum.ToString()
            };

            // Start with the different components of the request
            // Add the server Template
            compTemplate.ServerTemplates = new List<ServerTemplate> {
                new ServerTemplate {
                    Sequence = "1",
                    TemplateId = TemplateId    // TemplateId for the doc in DocuSign
                }
            };

            // Set up prefilled fields (if passed in) and add tabs to signer object
            if (Doc != null && Presets != null && Presets.Count > 0)
            {
                // Create the signer recipient object 
                Signer signer = new Signer
                {
                    Email = Doc.SignerEmail,
                    Name = Doc.SignerName,
                    ClientUserId = Doc.SignerId ?? null,    // For DocuSign via email, the ClientUserId must be null
                    RecipientId = "1",
                    RoleName = Doc.DSRoleName
                };

                signer.Tabs = ParsePresets(Presets);

                // Create recipients object
                Recipients recipients = new Recipients { Signers = new List<Signer> { signer } };

                // Add the roles via an inlineTemplate
                compTemplate.InlineTemplates = new List<InlineTemplate> {
                    new InlineTemplate {
                        Sequence = "2",
                        Recipients = recipients
                    }
                };
            }

            return compTemplate;
        }

        /// <summary>
        /// Create the Notification object for use in DocuSign calls.
        /// </summary>
        /// <param name="Rem">Reminder details</param>
        /// <param name="Exp">Expiration details</param>
        /// <returns>DocuSign Notification object</returns>
        public static Notification ParseNotifications(ReminderModel Rem, ExpirationModel Exp)
        {
            Notification notification = new Notification
            {
                UseAccountDefaults = "false"
            };

            if (Rem != null)
            {
                notification.Reminders = new Reminders
                {
                    ReminderEnabled = Rem.ReminderEnabled.ToString(),
                    ReminderDelay = Rem.ReminderDelayDays.ToString(),
                    ReminderFrequency = Rem.ReminderFrequencyDays.ToString()
                };
            }

            if (Exp != null)
            {
                notification.Expirations = new Expirations
                {
                    ExpireEnabled = Exp.ExpirationEnabled.ToString(),
                    ExpireAfter = Exp.ExpireAfterDays.ToString(),
                    ExpireWarn = Exp.ExpireWarnDays.ToString()
                };
            }

            return notification;
        }

        /// <summary>
        /// Create the EventNotification object for callback from DocuSign calls.
        /// </summary>
        /// <param name="Hook">Notification callback details</param>
        /// <returns>DocuSign EventNotification object</returns>
        public static EventNotification ParseEventNotifications(NotificationCallBackModel Hook)
        {
            // Get Envelope event codes
            List<EnvelopeEvent> envelopeEvents = new List<EnvelopeEvent>();
            foreach (string envEvent in Hook.EnvelopeEvents)
            {
                if(envEvent == EnvelopeStatus.STATUS_DRAFT|| envEvent == EnvelopeStatus.STATUS_SENT ||
                    envEvent == EnvelopeStatus.STATUS_DELIVERED || envEvent == EnvelopeStatus.STATUS_COMPLETED || 
                    envEvent == EnvelopeStatus.STATUS_DECLINED || envEvent == EnvelopeStatus.STATUS_VOIDED)
                    envelopeEvents.Add(new EnvelopeEvent { EnvelopeEventStatusCode = envEvent });
                else
                    throw new Exception(String.Format(Resources.INVALID_STATUS_FOR_EVENTNOTIFICATION_x, envEvent));
            }

            // Set up EventNotification object
            return new EventNotification
            {
                EnvelopeEvents = envelopeEvents,
                EventData = new ConnectEventData { Format = "json", Version = "restv2.1" },
                IncludeCertificateOfCompletion = "false",
                IncludeCertificateWithSoap = "false",
                IncludeDocumentFields = "false",
                IncludeDocuments = "false",
                IncludeEnvelopeVoidReason = "true",
                IncludeHMAC = "true",
                IncludeSenderAccountAsCustomField = "true",
                IncludeTimeZone = "true",
                LoggingEnabled = "true",
                RecipientEvents = null,
                RequireAcknowledgment = "true",
                SignMessageWithX509Cert = "true",
                SoapNameSpace = null,
                Url = Hook.WebHookUrl,
                UseSoapInterface = "false"
            };
        }

        public static EnvelopeDefinition CreateEnvelopeDefinition(
            string accountId,
            string status,
            DocumentModel Doc,
            ReminderModel Rem = null,
            ExpirationModel Exp = null,
            NotificationCallBackModel Hook = null,
            List<DocPreset> Presets = null)
        {
            // Get template ID
            string templateId = Utils.GetTemplateId(accountId, Doc.DSTemplateName);

            // Create composite template
            CompositeTemplate compTemplate = Utils.CreateCompositeTemplate(templateId, "1", Doc, Presets);

            // Set up reminders and expirations if available
            Notification notification = null;
            if (Rem != null || Exp != null)
            {
                notification = Utils.ParseNotifications(Rem, Exp);
            }

            // Custom webhook notification if available
            EventNotification eventNotification = null;
            if (Hook != null)
            {
                eventNotification = Utils.ParseEventNotifications(Hook);
            }

            // Bring the objects together in the EnvelopeDefinition
            //  Set the envelopeId if available and status ('sent' or 'created')
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
            {
                EnvelopeId = Doc.DSEnvelopeId ?? null,
                EmailSubject = Doc.DSEmailSubject,
                EmailBlurb = Doc.DSEmailBody ?? null,
                Status = status,
                CompositeTemplates = new List<CompositeTemplate> { compTemplate },
                Notification = notification ?? null,
                EventNotification = eventNotification ?? null,
                EnvelopeIdStamping = Doc.DSStampEnvelopeId ? "true" : "false",
                AllowReassign = Doc.DSAllowReassign ? "true" : "false",
            };

            return envelopeDefinition;
        }

        public static EnvelopeDefinition CreateEnvelopePacketDefinition(
            string accountId,
            string status,
            DocumentPacketModel DocPacket,
            ReminderModel Rem = null,
            ExpirationModel Exp = null,
            NotificationCallBackModel Hook = null,
            List<DocPreset> Presets = null)
        {
            // Create server template collection
            List<CompositeTemplate> compTemplates = new List<CompositeTemplate>();
            int seqNum = 1;
            foreach (string templateName in DocPacket.DSTemplateList)
            {
                // Get template ID
                string templateId = Utils.GetTemplateId(accountId, templateName);

                // Create composite template
                compTemplates.Add(Utils.CreateCompositeTemplatePacket(templateId, 
                    seqNum,
                    DocPacket, Presets));
            }

            // Set up reminders and expirations if available
            Notification notification = null;
            if (Rem != null || Exp != null)
            {
                notification = Utils.ParseNotifications(Rem, Exp);
            }

            // Custom webhook notification if available
            EventNotification eventNotification = null;
            if (Hook != null)
            {
                eventNotification = Utils.ParseEventNotifications(Hook);
            }

            // Bring the objects together in the EnvelopeDefinition
            //  Set the envelopeId if available and status ('sent' or 'created')
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
            {
                EmailSubject = DocPacket.DSEmailSubject,
                EmailBlurb = DocPacket.DSEmailBody,
                Status = status,
                CompositeTemplates = compTemplates,
                Notification = notification ?? null,
                EventNotification = eventNotification ?? null,
                EnvelopeIdStamping = DocPacket.DSStampEnvelopeId ? "true" : "false",
                AllowReassign = DocPacket.DSAllowReassign ? "true" : "false",
            };

            return envelopeDefinition;
        }


        /// <summary>
        /// Helper method to create a bulk send copy object (for DRY principle)
        /// </summary>
        /// <param name="doc">Dynamic: must be a BulkSendRecipientModel or BulkSendPacketRecipientModel object</param>
        /// <returns></returns>
        private static BulkSendingCopy CreateSingleBulkSendingCopy(dynamic doc)
        {
            if (string.IsNullOrEmpty(doc.SignerId))
                throw new Exception(Resources.EMPTY_SIGNER_ID_FOR_BATCH);

            List<BulkSendingCopyCustomField> customfields = new List<BulkSendingCopyCustomField>();
            customfields.Add(new BulkSendingCopyCustomField
            {
                Name = Constants.CUSTOM_FIELD_BULK_MAILING_SIGNER_ID,
                Value = doc.SignerId
            });

            BulkSendingCopy copy = new BulkSendingCopy
            {
                Recipients = new List<BulkSendingCopyRecipient>(),
                EmailSubject = doc.CustomEmailSubject,
                EmailBlurb = doc.CustomEmailBody,
                CustomFields = customfields
            };

            // Set up presets for this recipient
            List<BulkSendingCopyTab> recipientTabs = null;
            if (doc.Presets != null)
            {
                recipientTabs = new List<BulkSendingCopyTab>();
                foreach (DocPreset tab in doc.Presets)
                {
                    recipientTabs.Add(new BulkSendingCopyTab
                    {
                        InitialValue = tab.Value,
                        TabLabel = tab.Label
                    });
                };
            }

            // Create the bulk recipient object 
            BulkSendingCopyRecipient recipient = new BulkSendingCopyRecipient
            {
                Name = doc.SignerName,
                Email = doc.SignerEmail,
                RoleName = doc.DSRoleName,
                Tabs = recipientTabs
            };

            // Add recipient to copy object and add that to bulk copies
            copy.Recipients.Add(recipient);
            return copy;
        }

        public static BulkSendingList CreateTemplateBulkSendingList(BulkSendDocumentList BulkRecipients)
        {
            List<BulkSendingCopy> bulkCopies = new List<BulkSendingCopy>();

            foreach (BulkSendRecipientModel doc in BulkRecipients.BulkRecipientList)
            {
                bulkCopies.Add(CreateSingleBulkSendingCopy(doc));
            }

            // Return the bulk send list
            return new BulkSendingList
            {
                Name = BulkRecipients.BulkBatchName,
                BulkCopies = bulkCopies
            };
        }

        public static BulkSendingList CreatePacketBulkSendingList(BulkSendPacketList BulkPacketRecipients)
        {
            List<BulkSendingCopy> bulkCopies = new List<BulkSendingCopy>();

            foreach (BulkSendPacketRecipientModel doc in BulkPacketRecipients.BulkPacketRecipientList)
            {
                bulkCopies.Add(CreateSingleBulkSendingCopy(doc));
            }

            return new BulkSendingList
            {
                Name = BulkPacketRecipients.BulkBatchName,
                BulkCopies = bulkCopies
            };
        }

        public static CustomFields CreateCustomTextFields(Dictionary<string, string> fieldValuePairs)
        {
            List<TextCustomField> customFields = new List<TextCustomField>();

            foreach (KeyValuePair<string, string> item in fieldValuePairs)
            {
                customFields.Add(new TextCustomField {
                    Name = item.Key,
                    Required = "false",
                    Show = "false",
                    Value = item.Value
                });
            }

            return new CustomFields
            {
                TextCustomFields = customFields
            };
        }

        public static Recipients CreatePlaceholderRecipients()
        {
            // Placeholder recipients. 
            // These values will be replaced by the details provided in the Bulk List uploaded 
            //  Note: The name / email format used is:
            //  Name: Multi Bulk Recipients::{rolename}
            //  Email: MultiBulkRecipients-{rolename}@epiqglobal.com
            return new Recipients
            {
                Signers = new List<Signer>
                    {
                        new Signer
                        {
                            Name = "Multi Bulk Recipient::signer",
                            Email = "multiBulkRecipients-signer@epiqglobal.com",
                            RoleName = "_placeholder_",
                            RoutingOrder = "1",
                            Status = "sent",
                            DeliveryMethod = "Email",
                            RecipientId = "1",
                            RecipientType = "signer"
                        }
                    }
            };
        }

        #endregion INTERNAL_PUBLIC_METHODS
    }
}
