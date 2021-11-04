using System;
using System.Collections.Generic;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using ECAR.DocuSign.Models;
using ECAR.DocuSign.Properties;
using ECAR.DocuSign.Security;
using ECAR.DocuSign.Common;

namespace ECAR.DocuSign
{
    /// <summary>
    /// ECAR.DocuSign embedded signing ceremony with a predefined document template.
    /// </summary>
    public static class TemplateSign
    {
        #region PRIVATE_METHODS

        /// <summary>
        /// Internal private method to call DocuSign to create (not send) an envelope for preview 
        /// </summary>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Rem">Reminder data</param>
        /// <param name="Exp">Expiration Data</param>
        /// <param name="Hook">Notification callback webhook Data</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>EnvelopeSummary object with result from DocuSign call</returns>
        private static EnvelopeSummary CreateDocuSignPreview(
            ref DocumentModel Doc,
            ReminderModel Rem = null,
            ExpirationModel Exp = null,
            NotificationCallBackModel Hook = null,
            List<DocPreset> Presets = null)
        {
            if (!DocuSignConfig.Ready)
                throw new Exception(Resources.DSCONFIG_NOT_SET);

            // Signing Ceremony with a template
            // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
            // 2. Use the SDK to create and send the envelope with a Template document in DocuSign

            // Read config values
            string accountId = DocuSignConfig.AccountID;

            // 1. Create envelope request object as "created" for preview
            EnvelopeDefinition envelopeDefinition = Utils.CreateEnvelopeDefinition(accountId, EnvelopeStatus.STATUS_CREATED, Doc, Rem, Exp, Hook, Presets);

            // 2. Use the SDK to create the envelope
            EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
            EnvelopeSummary results = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

            // Set envelopeId in Doc object before returning
            Doc.DSEnvelopeId = results.EnvelopeId;

            // return EnvelopeSummary back to caller
            return results;
        }


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
            List<DocPreset> Presets = null)
        {
            if (!DocuSignConfig.Ready)
                throw new Exception(Resources.DSCONFIG_NOT_SET);

            // Signing Ceremony with a template
            // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
            // 2. Use the SDK to create and send the envelope with a Template document in DocuSign

            // Read config values
            string accountId = DocuSignConfig.AccountID;

            // 1. Create envelope request object for sending
            EnvelopeDefinition envelopeDefinition = Utils.CreateEnvelopeDefinition(accountId, EnvelopeStatus.STATUS_SENT, Doc, Rem, Exp, Hook, Presets);

            // 2. Use the SDK to create and send the envelope
            EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
            EnvelopeSummary results = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

            // Set envelopeId in Doc object before returning
            Doc.DSEnvelopeId = results.EnvelopeId;

            // return EnvelopeSummary back to caller
            return results;
        }

        /// <summary>
        /// Internal private method to call DocuSign for both embedded and asynchronous (email) signing
        /// </summary>
        /// <param name="DocPacket">Document Packet data (in: document list &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Rem">Reminder data</param>
        /// <param name="Exp">Expiration Data</param>
        /// <param name="Hook">Notification callback webhook Data</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>EnvelopeSummary object with result from DocuSign call</returns>
        private static EnvelopeSummary ExecuteDocuSignMultiple(
            ref DocumentPacketModel DocPacket,
            ReminderModel Rem = null,
            ExpirationModel Exp = null,
            NotificationCallBackModel Hook = null,
            List<DocPreset> Presets = null)
        {
            if (!DocuSignConfig.Ready)
                throw new Exception(Resources.DSCONFIG_NOT_SET);

            // Signing Ceremony with a template
            // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
            // 2. Use the SDK to create and send the envelope with a Template document in DocuSign

            // Read config values
            string accountId = DocuSignConfig.AccountID;

            // 1. Create envelope request object for sending
            EnvelopeDefinition envelopeDefinition = Utils.CreateEnvelopePacketDefinition(accountId, EnvelopeStatus.STATUS_SENT, DocPacket, Rem, Exp, Hook, Presets);

            // 2. Use the SDK to create and send the envelope
            EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
            EnvelopeSummary results = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

            // Set envelopeId in Doc object before returning
            DocPacket.DSEnvelopeId = results.EnvelopeId;

            // return EnvelopeSummary back to caller
            return results;
        }
        #endregion PRIVATE_METHODS

        #region PUBLIC_METHODS
        /// <summary>
        /// Initiate embedded signing ceremony with DocuSign.
        /// </summary>
        /// <param name="AppReturnUrl">Return URL that DocuSign should transfer control to after the embedded signing cermony is complete ('?envelopeId=[id]' will be appended)</param>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>DocuSign URL for embedded signing</returns>
        /// <exception cref="System.Exception"></exception>
        public static string EmbeddedTemplateSign(
            string AppReturnUrl,
            ref DocumentModel Doc,
            List<DocPreset> Presets = null)
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

                // 5. Return the preview URL
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
        /// <exception cref="System.Exception"></exception>
        public static string EmbeddedTemplateSign(
            string AppReturnUrl,
            ref DocumentModel Doc,
            ReminderModel Rem,
            ExpirationModel Exp,
            List<DocPreset> Presets = null)
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

                // 5. Return the preview URL
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
        /// <exception cref="System.Exception"></exception>
        public static string EmailedTemplateSign(
            ref DocumentModel Doc,
            List<DocPreset> Presets = null)
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
        /// <exception cref="System.Exception"></exception>
        public static string EmailedTemplateSign(
            ref DocumentModel Doc,
            ReminderModel Rem,
            ExpirationModel Exp,
            List<DocPreset> Presets = null)
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
        /// Send a DocuSign for asynchronous signing via email with callback option.
        /// </summary>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Hook">Notification callback webhook details</param>
        /// <param name="Rem">Reminder details (optional) for DocuSign envelope</param>
        /// <param name="Exp">Expiration details (optional) for the DocuSign envelope</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>DocuSign envelope status</returns>
        /// <exception cref="System.Exception"></exception>
        public static string EmailedTemplateSignWithCallBack(
            ref DocumentModel Doc,
            NotificationCallBackModel Hook,
            ReminderModel Rem = null,
            ExpirationModel Exp = null,
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

        /// <summary>
        /// Create a preview URL for the DocuSign document to review that the recipients will see.
        /// </summary>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="AppReturnUrl">Return URL that DocuSign should transfer control when the preview is closed</param>
        /// <param name="SendByMail">Indicates whether the DocuSign envelope will be sent by mail or in an embedded ceremony</param>
        /// <param name="Hook">Notification callback webhook details</param>
        /// <param name="Rem">Reminder details (optional) for DocuSign envelope</param>
        /// <param name="Exp">Expiration details (optional) for the DocuSign envelope</param>
        /// <param name="Presets">Fields to prefill (optional) in the DocuSign document</param>
        /// <returns>DocuSign URL for recipient preview</returns>
        /// <exception cref="System.Exception"></exception>
        public static string CreatePreviewURL(
            ref DocumentModel Doc,
            string AppReturnUrl,
            bool SendByMail,
            NotificationCallBackModel Hook,
            ReminderModel Rem = null,
            ExpirationModel Exp = null,
            List<DocPreset> Presets = null)
        {
            try
            {
                // Create a preview URL
                // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
                // 2. Use the SDK to create and send the envelope with a Template document in DocuSign
                // 3. Create Envelope Recipient View request obj
                // 4. Use the SDK to obtain a Recipient Preview URL
                // 5. Return the Recipient Preview URL to the caller (takes user to DocuSign)

                // Steps 1 & 2 are performed in the private method

                // Create preview envelope (dont use clientUserId for emails)
                string origSignerId = Doc.SignerId;
                Doc.SignerId = (SendByMail) ? null : origSignerId;
                EnvelopeSummary results = CreateDocuSignPreview(ref Doc, Rem, Exp, Hook, Presets);

                // Reset signer ID, populate EnvelopeId and DocumentId for return
                //  DocumentId is always 1 for template sign
                Doc.SignerId = origSignerId;
                Doc.DSEnvelopeId = results.EnvelopeId;
                Doc.DSDocumentId = "1";

                // 4. Use the SDK to obtain a Recipient View URL
                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Set up RecipientPreviewRequest object 
                RecipientPreviewRequest previewRequest = new RecipientPreviewRequest
                {
                    ReturnUrl = AppReturnUrl,
                    RecipientId = "1"
                };

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                ViewUrl viewUrl = envelopesApi.CreateEnvelopeRecipientPreview(accountId, results.EnvelopeId, previewRequest);

                // 5. Set up the preview URL in the Doc object and return it
                Doc.DSPreviewUrl = viewUrl.Url;
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
        /// Send a previously created DocuSign envelope for signature.
        /// </summary>
        /// <param name="Doc">Document data populated with the DocuSign envelope ID of the preview</param>
        /// <returns>Boolean result from DocuSign call</returns>
        /// <exception cref="System.Exception"></exception>
        public static bool SendPreviewedEnvelope(
            DocumentModel Doc)
        {
            try
            { 
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Return error if EnvelopeId is empty
                if (string.IsNullOrEmpty(Doc.DSEnvelopeId))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                // Read config values
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();

                // 1. Create envelope request object
                Envelope envelope = new Envelope
                {
                    Status = EnvelopeStatus.STATUS_SENT
                };

                // 2. Use the SDK to 'Send' the envelope by calling the update method
                EnvelopeUpdateSummary updateSummary = envelopesApi.Update(accountId, Doc.DSEnvelopeId, envelope);

                // If error details is not null, then error occurred  
                if (updateSummary.ErrorDetails != null)
                {
                    throw new Exception(updateSummary.ErrorDetails.Message);
                }

                // return EnvelopeSummary back to caller
                return true;
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
        /// Send a single document (template) to multiple users using DocuSign's Bulk Send option.
        /// </summary>
        /// <param name="BulkSendData">Bulk send information (recipients, template, email message etc.) in a BulkSendDocumentList object; the List ID is returned in the object</param>
        /// <param name="Hook">Callback notification webhook details</param>
        /// <returns>The Batch ID of the current batch (as a string) for status queries</returns>
        /// <exception cref="System.Exception"></exception>
        public static string BulkSendTemplate(ref BulkSendDocumentList BulkSendData, NotificationCallBackModel Hook)
        {
            if (!DocuSignConfig.Ready)
                throw new Exception(Resources.DSCONFIG_NOT_SET);

            // Return error if Template name is empty
            if (string.IsNullOrEmpty(BulkSendData.DSBatchTemplateName))
                throw new Exception(Resources.EMPTY_TEMPLATE_NAME);

            // Create bulk sending list
            BulkSendingList bulkSendingList = Utils.CreateTemplateBulkSendingList(BulkSendData);

            // Read config values
            string accountId = DocuSignConfig.AccountID;

            // Create API Client and call it
            BulkEnvelopesApi bulkEnvelopesApi = Authenticate.CreateBulkEnvelopesApiClient();
            BulkSendingList createBulkListResult = bulkEnvelopesApi.CreateBulkSendList(accountId, bulkSendingList);

            // Create the envelope with the template 
            string templateId = Utils.GetTemplateId(accountId, BulkSendData.DSBatchTemplateName);

            // Create the CompositeTemplate object
            //  Sequence number will always be "1" for a single template
            CompositeTemplate compTemplate = Utils.CreateCompositeTemplate(templateId, "1");

            // Bring the objects together in the EnvelopeDefinition
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
            {
                EmailSubject = BulkSendData.BulkEmailSubject,
                EmailBlurb = BulkSendData.BulkEmailBody,
                Status = "created",
                EventNotification = (Hook != null) ? Utils.ParseEventNotifications(Hook) : null,
                CompositeTemplates = new List<CompositeTemplate> { compTemplate },
                EnvelopeIdStamping = BulkSendData.DSStampEnvelopeId ? "true" : "false",
                AllowReassign = BulkSendData.DSAllowReassign ? "true" : "false",
                EnableWetSign = BulkSendData.DSAllowPrintAndSign ? "true" : "false",
            };

            EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
            var envelopeResults = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

            // Attach the list ID to the envelope
            // We will add envelope custom fields set to the value of the listId (EnvelopeCustomFields::create)
            // and a placeholder field for SignerId.  These Custom fields may be used for tracking the
            // envelope ID from Bulk Send back to the batch and signer via the DSGetEnvelopeCustomFields method
            Dictionary<string, string> fieldValuePairs = new Dictionary<string, string>();
            fieldValuePairs.Add(Constants.CUSTOM_FIELD_BULK_MAILING_LIST_ID, createBulkListResult.ListId);
            fieldValuePairs.Add(Constants.CUSTOM_FIELD_BULK_MAILING_SIGNER_ID, "_placeholder_");

            CustomFields fields = Utils.CreateCustomTextFields(fieldValuePairs);
            envelopesApi.CreateCustomFields(accountId, envelopeResults.EnvelopeId, fields);

            // Add a placeholder Recipients object. 
            // These will be replaced by the details provided in the Bulk List uploaded 
            Recipients recipients = Utils.CreatePlaceholderRecipients();
            envelopesApi.CreateRecipient(accountId, envelopeResults.EnvelopeId, recipients);

            // Initiate bulk send
            BulkSendResponse bulkRequestResult = bulkEnvelopesApi.CreateBulkSendRequest(accountId, createBulkListResult.ListId, new BulkSendRequest { EnvelopeOrTemplateId = envelopeResults.EnvelopeId });

            // Set List ID and Batch ID in the ref object
            BulkSendData.DSListId = createBulkListResult.ListId;
            BulkSendData.DSBatchId = bulkRequestResult.BatchId;

            // Return the Batch ID of this batch 
            return bulkRequestResult.BatchId;
        }

        /// <summary>
        /// Send a document packet containing multiple documents (templates) to multiple users using DocuSign's Bulk Send option.
        /// </summary>
        /// <param name="BulkSendData">Bulk send information (recipients, templates, email message etc.) in a BulkSendPacketList object; The List ID and Batch ID are returned in the object</param>
        /// <param name="Hook">Callback notification webhook details</param>
        /// <returns>The Batch ID of the current batch (as a string) for status queries</returns>
        /// <exception cref="System.Exception"></exception>
        public static string BulkSendPacket(ref BulkSendPacketList BulkSendData, NotificationCallBackModel Hook)
        {
            if (!DocuSignConfig.Ready)
                throw new Exception(Resources.DSCONFIG_NOT_SET);

            // Return error if Template name is empty
            if (BulkSendData.DSBatchPacketTemplates == null || BulkSendData.DSBatchPacketTemplates.Count <= 0 )
                throw new Exception(Resources.EMPTY_TEMPLATE_NAME);

            // Create bulk sending list
            BulkSendingList bulkSendingList = Utils.CreatePacketBulkSendingList(BulkSendData);

            // Read config values
            string accountId = DocuSignConfig.AccountID;

            // Create API Client and call it
            BulkEnvelopesApi bulkEnvelopesApi = Authenticate.CreateBulkEnvelopesApiClient();
            BulkSendingList createBulkListResult = bulkEnvelopesApi.CreateBulkSendList(accountId, bulkSendingList);

            // Create the envelope with all the templates in the packet
            List<CompositeTemplate> packetTemplates = new List<CompositeTemplate>();
            foreach (string template in BulkSendData.DSBatchPacketTemplates)
            {
                // Get Template Id
                string templateId = Utils.GetTemplateId(accountId, template);

                // Create the CompositeTemplate object and add to the list
                //  Sequence number will be set according to the order in the list
                packetTemplates.Add(Utils.CreateCompositeTemplate(templateId, 
                    (1+BulkSendData.DSBatchPacketTemplates.IndexOf(template)).ToString()));
            }
            // Bring the objects together in the EnvelopeDefinition
            EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
            {
                EmailSubject = BulkSendData.BulkEmailSubject,
                EmailBlurb = BulkSendData.BulkEmailBody,
                Status = "created",
                EventNotification = (Hook != null) ? Utils.ParseEventNotifications(Hook) : null,
                CompositeTemplates = packetTemplates,
                EnvelopeIdStamping = BulkSendData.DSStampEnvelopeId ? "true" : "false",
                AllowReassign = BulkSendData.DSAllowReassign ? "true" : "false",
                EnableWetSign = BulkSendData.DSAllowPrintAndSign ? "true" : "false",
            };

            EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
            var envelopeResults = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

            // Attach the list ID to the envelope
            // We will add envelope custom fields set to the value of the listId (EnvelopeCustomFields::create)
            // and a placeholder field for SignerId.  These Custom fields may be used for tracking the
            // envelope ID from Bulk Send back to the batch and signer via the DSGetEnvelopeCustomFields method
            Dictionary<string, string> fieldValuePairs = new Dictionary<string, string>();
            fieldValuePairs.Add(Constants.CUSTOM_FIELD_BULK_MAILING_LIST_ID, createBulkListResult.ListId);
            fieldValuePairs.Add(Constants.CUSTOM_FIELD_BULK_MAILING_SIGNER_ID, "_placeholder_");

            CustomFields fields = Utils.CreateCustomTextFields(fieldValuePairs);
            envelopesApi.CreateCustomFields(accountId, envelopeResults.EnvelopeId, fields);

            // Add a placeholder Recipients object. 
            // These will be replaced by the details provided in the Bulk List uploaded 
            Recipients recipients = Utils.CreatePlaceholderRecipients();
            envelopesApi.CreateRecipient(accountId, envelopeResults.EnvelopeId, recipients);

            // Initiate bulk send
            BulkSendResponse bulkRequestResult = bulkEnvelopesApi.CreateBulkSendRequest(accountId, createBulkListResult.ListId, new BulkSendRequest { EnvelopeOrTemplateId = envelopeResults.EnvelopeId });

            // Set List ID and Batch ID in the ref object
            BulkSendData.DSListId = createBulkListResult.ListId;
            BulkSendData.DSBatchId = bulkRequestResult.BatchId;

            // Return the batch ID as a string
            return bulkRequestResult.BatchId;
        }

        /// <summary>
        /// Send a multi-document packet via DocuSign for email signing with callback option.
        /// </summary>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Hook">Notification callback webhook details</param>
        /// <param name="Rem">Reminder details (optional) for DocuSign envelope</param>
        /// <param name="Exp">Expiration details (optional) for the DocuSign envelope</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>DocuSign envelope status</returns>
        /// <exception cref="System.Exception"></exception>
        public static string EmailedPacketSign(
            ref DocumentPacketModel Doc,
            NotificationCallBackModel Hook,
            ReminderModel Rem = null,
            ExpirationModel Exp = null,
            List<DocPreset> Presets = null)
        {
            try
            {
                // REMOVING SignerId value from the DocumentModel - indicates signature via email
                string origSignerId = Doc.SignerId;
                Doc.SignerId = null;

                // Email Signing Ceremony with a template
                EnvelopeSummary results = ExecuteDocuSignMultiple(ref Doc, Rem, Exp, Hook, Presets);

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
        /// Initiate embedded signing ceremony for a multi-document packet with DocuSign (with reminder and expiration settings).
        /// </summary>
        /// <param name="AppReturnUrl">Return URL that DocuSign should transfer control to after the embedded signing cermony is complete ('?envelopeId=[id]' will be appended)</param>
        /// <param name="Doc">Document data (in: document &amp; signer info; out: DocuSign envelope &amp; document ID)</param>
        /// <param name="Rem">Reminder details if required for DocuSign envelope</param>
        /// <param name="Exp">Expiration details if required for the DocuSign envelope</param>
        /// <param name="Presets">(Optional) Fields to prefill in the DocuSign document</param>
        /// <returns>DocuSign URL for embedded signing</returns>
        /// <exception cref="System.Exception"></exception>
        public static string EmbeddedPacketSign(
            string AppReturnUrl,
            ref DocumentPacketModel Doc,
            ReminderModel Rem,
            ExpirationModel Exp,
            List<DocPreset> Presets = null)
        {
            try
            {
                // Embedded Signing Ceremony with a template
                EnvelopeSummary results = ExecuteDocuSignMultiple(ref Doc, Rem, Exp, null, Presets);

                // Populate EnvelopeId and DocumentId for return
                //  DocumentId is always 1 for template sign
                Doc.DSEnvelopeId = results.EnvelopeId;
                Doc.DSDocumentId = "1";

                // Create Envelope Recipient View request obj
                RecipientViewRequest viewRequest = new RecipientViewRequest
                {
                    ReturnUrl = AppReturnUrl + String.Format("?envelopeId={0}", results.EnvelopeId),
                    ClientUserId = Doc.SignerId,
                    AuthenticationMethod = "none",
                    UserName = Doc.SignerName,
                    Email = Doc.SignerEmail
                };

                // Use the SDK to obtain a Recipient View URL
                //  Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                ViewUrl viewUrl = envelopesApi.CreateRecipientView(accountId, results.EnvelopeId, viewRequest);

                // Return the preview URL
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

        #endregion
    }
}
