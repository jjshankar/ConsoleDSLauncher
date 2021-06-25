using System;
using System.Collections.Generic;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using ECAR.DocuSign.Models;
using ECAR.DocuSign.Properties;
using ECAR.DocuSign.Security;

namespace ECAR.DocuSign
{
    /// <summary>
    /// ECAR.DocuSign embedded signing ceremony with a predefined document template.
    /// </summary>
    public static class TemplateSign
    {
        /// <summary>
        /// Internal private method to call DocuSign for both embedded and asynchronous (email) signing
        /// </summary>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Rem">Reminder data</param>
        /// <param name="Exp">Expiration Data</param>
        /// <param name="Hook">Notification callback webhook Data</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>EnvelopeSummary object with result from DocuSign call</returns>
        private static EnvelopeSummary ExecuteDocuSign(
            ref DocumentModel Doc, 
            ReminderModel Rem = null, 
            ExpirationModel Exp = null, 
            NotificationCallBackModel Hook = null,
            List < DocPreset> Presets = null)
        {
            if (!DocuSignConfig.Ready)
                throw new Exception(Resources.DSCONFIG_NOT_SET);

            // Signing Ceremony with a template
            // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
            // 2. Use the SDK to create and send the envelope with a Template document in DocuSign

            // 1. Create envelope request object
            // Read config values
            string accountId = DocuSignConfig.AccountID;

            // Get the template ID by calling the Templates API
            TemplatesApi templatesApi = Authenticate.CreateTemplateApiClient();
            TemplatesApi.ListTemplatesOptions options = new TemplatesApi.ListTemplatesOptions { searchText = Doc.DSTemplateName };
            EnvelopeTemplateResults searchResults = templatesApi.ListTemplates(accountId, options);

            string templateId;

            // Process results
            if (int.Parse(searchResults.ResultSetSize) > 0)
            {
                // Found the template! Record its id
                templateId = searchResults.EnvelopeTemplates[0].TemplateId;
            }
            else
                throw new ApiException(404, String.Format(Resources.TEMPLATE_NOT_FOUND_x, Doc.DSTemplateName));

            // Start with the different components of the request
            // Create the signer recipient object 
            Signer signer = new Signer
            {
                Email = Doc.SignerEmail,
                Name = Doc.SignerName,
                ClientUserId = Doc.SignerId ?? null,    // For DocuSign via email, the ClientUserId must be null
                RecipientId = "1",
                RoleName = Doc.DSRoleName
            };

            // Set up prefilled fields (if passed in)
            if (Presets != null && Presets.Count > 0)
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
                                Checkbox tab = new Checkbox { TabLabel = t.Label, Selected = t.Value, Locked = (t.Locked ? "true" : "false") };
                                checkBoxTabs = new List<Checkbox>();
                                checkBoxTabs.Add(tab);
                                break;
                            }
                        case Models.Presets.Date:
                            {
                                Date tab = new Date { TabLabel = t.Label, Value = t.Value, Locked = (t.Locked ? "true" : "false") };
                                dateTabs = new List<Date>();
                                dateTabs.Add(tab);
                                break;
                            }
                        case Models.Presets.Ssn:
                            {
                                Ssn tab = new Ssn { TabLabel = t.Label, Value = t.Value, Locked = (t.Locked ? "true" : "false") };
                                ssnTabs = new List<Ssn>();
                                ssnTabs.Add(tab);
                                break;
                            }
                        case Models.Presets.Text:
                            {
                                Text tab = new Text { TabLabel = t.Label, Value = t.Value, Locked = (t.Locked ? "true" : "false") };
                                textTabs = new List<Text>();
                                textTabs.Add(tab);
                                break;
                            }
                    }
                }

                // Add tabs to signer object
                signer.Tabs = new Tabs
                {
                    CheckboxTabs = checkBoxTabs,
                    DateTabs = dateTabs,
                    SsnTabs = ssnTabs,
                    TextTabs = textTabs
                };
            }

            // Create recipients object
            Recipients recipients = new Recipients { Signers = new List<Signer> { signer } };

            // Create a composite template for the Server Template
            CompositeTemplate compTemplate = new CompositeTemplate
            {
                CompositeTemplateId = "1"
            };
            ServerTemplate serverTemplates = new ServerTemplate
            {
                Sequence = "1",
                TemplateId = templateId    // TemplateId for the doc in DocuSign
            };

            compTemplate.ServerTemplates = new List<ServerTemplate> { serverTemplates };

            // Add the roles via an inlineTemplate
            InlineTemplate inlineTemplate = new InlineTemplate
            {
                Sequence = "1",
                Recipients = recipients
            };

            compTemplate.InlineTemplates = new List<InlineTemplate> { inlineTemplate };

            // Set up reminders and expirations if available
            Notification notification = null;
            if (Rem != null || Exp != null)
            {
                notification = new Notification
                {
                    UseAccountDefaults = "false"
                };

                if(Rem != null)
                {
                    notification.Reminders = new Reminders
                    {
                        ReminderEnabled = Rem.ReminderEnabled.ToString(),
                        ReminderDelay = Rem.ReminderDelayDays.ToString(),
                        ReminderFrequency = Rem.ReminderFrequencyDays.ToString()
                    };
                }

                if(Exp != null)
                {
                    notification.Expirations = new Expirations
                    {
                        ExpireEnabled = Exp.ExpirationEnabled.ToString(),
                        ExpireAfter = Exp.ExpireAfterDays.ToString(),
                        ExpireWarn = Exp.ExpireWarnDays.ToString()
                    };
                }
            }

            // Custom webhook notification
            EventNotification eventNotification = null;

            if (Hook != null)
            {
                // Get Envelope event codes
                List<EnvelopeEvent> envelopeEvents = new List<EnvelopeEvent>();
                foreach (string envEvent in Hook.EnvelopeEvents)
                    envelopeEvents.Add(new EnvelopeEvent { EnvelopeEventStatusCode = envEvent });

                // Set up EventNotification object
                eventNotification = new EventNotification
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

            // Bring the objects together in the EnvelopeDefinition
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
            {
                EmailSubject = Doc.DSEmailSubject,
                Status = "sent",
                CompositeTemplates = new List<CompositeTemplate> { compTemplate },
                Notification = notification ?? null,
                EventNotification = eventNotification ?? null
            };

            // 2. Use the SDK to create and send the envelope
            EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
            EnvelopeSummary results = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

            // return EnvelopeSummary back to caller
            return results;
        }

        #region PUBLIC_METHODS
        /// <summary>
        /// Initiate embedded signing ceremony with DocuSign.
        /// </summary>
        /// <param name="AppReturnUrl">Return URL that DocuSign should transfer control to after the embedded signing cermony is complete ('?envelopeId=[id]' will be appended)</param>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>DocuSign URL for embedded signing</returns>
        public static string EmbeddedTemplateSign(string AppReturnUrl, ref DocumentModel Doc, List<DocPreset> Presets = null)
        {
            try
            {
                // Embedded Signing Ceremony with a template
                // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
                // 2. Use the SDK to create and send the envelope with a Template document in DocuSign
                // 3. Create Envelope Recipient View request obj
                // 4. Use the SDK to obtain a Recipient View URL
                // 5. Return the Recipient View URL to the caller (takes user to DocuSign)
                //    - After signing, DocuSign will redirect to the URL specified in the AppReturnUrl argument

                // Steps 1 & 2 are performed in the private method
                EnvelopeSummary results = ExecuteDocuSign(ref Doc, null, null, null, Presets);

                // Populate EnvelopeId and DocumentId for return
                //  DocumentId is always 1 for template sign
                Doc.DSEnvelopeId = results.EnvelopeId;
                Doc.DSDocumentId = "1";     

                // 3. Create Envelope Recipient View request obj
                RecipientViewRequest viewRequest = new RecipientViewRequest
                {
                    ReturnUrl = AppReturnUrl + String.Format("?envelopeId={0}", results.EnvelopeId),
                    ClientUserId = Doc.SignerId,
                    AuthenticationMethod = "none",
                    UserName = Doc.SignerName,
                    Email = Doc.SignerEmail
                };

                // 4. Use the SDK to obtain a Recipient View URL
                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                ViewUrl viewUrl = envelopesApi.CreateRecipientView(accountId, results.EnvelopeId, viewRequest);

                // 5. Redirect the user's browser to the URL
                return viewUrl.Url;
            }
            catch (ApiException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Initiate embedded signing ceremony with DocuSign (with reminder and expiration settings).
        /// </summary>
        /// <param name="AppReturnUrl">Return URL that DocuSign should transfer control to after the embedded signing cermony is complete ('?envelopeId=[id]' will be appended)</param>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Rem">Reminder details if required for DocuSign envelope</param>
        /// <param name="Exp">Expiration details if required for the DocuSign envelope</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>DocuSign URL for embedded signing</returns>
        public static string EmbeddedTemplateSign(string AppReturnUrl, ref DocumentModel Doc, ReminderModel Rem, ExpirationModel Exp, List<DocPreset> Presets = null)
        {
            try
            {
                // Embedded Signing Ceremony with a template
                // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
                // 2. Use the SDK to create and send the envelope with a Template document in DocuSign
                // 3. Create Envelope Recipient View request obj
                // 4. Use the SDK to obtain a Recipient View URL
                // 5. Return the Recipient View URL to the caller (takes user to DocuSign)
                //    - After signing, DocuSign will redirect to the URL specified in the AppReturnUrl argument

                // Steps 1 & 2 are performed in the private method
                EnvelopeSummary results = ExecuteDocuSign(ref Doc, Rem, Exp, null, Presets);

                // Populate EnvelopeId and DocumentId for return
                //  DocumentId is always 1 for template sign
                Doc.DSEnvelopeId = results.EnvelopeId;
                Doc.DSDocumentId = "1";

                // 3. Create Envelope Recipient View request obj
                RecipientViewRequest viewRequest = new RecipientViewRequest
                {
                    ReturnUrl = AppReturnUrl + String.Format("?envelopeId={0}", results.EnvelopeId),
                    ClientUserId = Doc.SignerId,
                    AuthenticationMethod = "none",
                    UserName = Doc.SignerName,
                    Email = Doc.SignerEmail
                };

                // 4. Use the SDK to obtain a Recipient View URL
                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                ViewUrl viewUrl = envelopesApi.CreateRecipientView(accountId, results.EnvelopeId, viewRequest);

                // 5. Redirect the user's browser to the URL
                return viewUrl.Url;
            }
            catch (ApiException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Send a DocuSign for asynchronous signing via email.
        /// </summary>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>DocuSign envelope status</returns>
        public static string EmailedTemplateSign(ref DocumentModel Doc, List<DocPreset> Presets = null)
        {
            try
            {
                // Email Signing Ceremony with a template
                // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
                // 2. Use the SDK to create and send the envelope with a Template document in DocuSign

                // REMOVING SignerId value from the DocumentModel indicates signature via email
                string origSignerId = Doc.SignerId;
                Doc.SignerId = null;

                // Steps 1 & 2 are performed in the private method
                EnvelopeSummary results = ExecuteDocuSign(ref Doc, null, null, null, Presets);

                // RESTORE outgoing Doc object with the original signerId sent
                Doc.SignerId = origSignerId;

                // Populate EnvelopeId and DocumentId for return
                //  DocumentId is always 1 for template sign
                Doc.DSEnvelopeId = results.EnvelopeId;
                Doc.DSDocumentId = "1";

                // Return the envelope result
                return results.Status;
            }
            catch (ApiException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Send a DocuSign for asynchronous signing via email (with reminder and expiration settings).
        /// </summary>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Rem">Reminder details if required for DocuSign envelope</param>
        /// <param name="Exp">Expiration details if required for the DocuSign envelope</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>DocuSign envelope status</returns>
        public static string EmailedTemplateSign(ref DocumentModel Doc, ReminderModel Rem, ExpirationModel Exp, List<DocPreset> Presets = null)
        {
            try
            {
                // Email Signing Ceremony with a template
                // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
                // 2. Use the SDK to create and send the envelope with a Template document in DocuSign

                // REMOVING SignerId value from the DocumentModel indicates signature via email
                string origSignerId = Doc.SignerId;
                Doc.SignerId = null;

                // Steps 1 & 2 are performed in the private method
                EnvelopeSummary results = ExecuteDocuSign(ref Doc, Rem, Exp, null, Presets);

                // RESTORE outgoing Doc object with the original signerId sent
                Doc.SignerId = origSignerId;

                // Populate EnvelopeId and DocumentId for return
                //  DocumentId is always 1 for template sign
                Doc.DSEnvelopeId = results.EnvelopeId;
                Doc.DSDocumentId = "1";

                // Return the envelope result
                return results.Status;
            }
            catch (ApiException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Send a DocuSign for asynchronous signing via email (with reminder and expiration settings).
        /// </summary>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Rem">Reminder details if required for DocuSign envelope</param>
        /// <param name="Exp">Expiration details if required for the DocuSign envelope</param>
        /// <param name="Hook">Notification callback webhook details</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>DocuSign envelope status</returns>
        public static string EmailedTemplateSignWithCallBack(
            ref DocumentModel Doc, 
            ReminderModel Rem, 
            ExpirationModel Exp, 
            NotificationCallBackModel Hook,
            List<DocPreset> Presets = null)
        {
            try
            {
                // Email Signing Ceremony with a template
                // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
                // 2. Use the SDK to create and send the envelope with a Template document in DocuSign

                // REMOVING SignerId value from the DocumentModel - indicates signature via email
                string origSignerId = Doc.SignerId;
                Doc.SignerId = null;

                // Steps 1 & 2 are performed in the private method
                EnvelopeSummary results = ExecuteDocuSign(ref Doc, Rem, Exp, Hook, Presets);

                // RESTORE outgoing Doc object with the original signerId sent
                Doc.SignerId = origSignerId;

                // Populate EnvelopeId and DocumentId for return
                //  DocumentId is always 1 for template sign
                Doc.DSEnvelopeId = results.EnvelopeId;
                Doc.DSDocumentId = "1";

                // Return the envelope result
                return results.Status;
            }
            catch (ApiException ex)
            {
                throw new Exception(ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion
    }
}
