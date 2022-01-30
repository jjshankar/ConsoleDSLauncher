﻿using System;
using System.Collections.Generic;
using System.IO;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using ECAR.DocuSign.Models;
using ECAR.DocuSign.Properties;
using ECAR.DocuSign.Security;

namespace ECAR.DocuSign
{
    /// <summary>
    /// Access documents signed using ECAR.DocuSign.
    /// </summary>
    public static class Status
    {

    #region PUBLIC_METHODS

    #if DEBUG
            /// <summary>
            ///  DEBUG ONLY
            /// </summary>
            /// <param name="sec"></param>
            public static void AdjustExpiration(int sec)
            {
                DocuSignConfig.AccessTokenExpiration = DateTime.Now.AddSeconds(sec);
            }
    #endif

        /// <summary>
        /// Get a list of all DocuSign envelope IDs starting from a given date.
        /// </summary>
        /// <param name="StartDate">The date to start querying from</param>
        /// <returns>List of all envelopes from the given date as EnvelopeModel objects</returns>
        /// <exception cref="System.Exception"></exception>
        public static List<EnvelopeModel> DSGetAllEnvelopes(DateTime StartDate)
        {
            try
            {
                // Validate input
                if (StartDate == null)
                    throw new Exception(Resources.EMPTY_STARTDATE_VALUE);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                EnvelopesApi.ListStatusChangesOptions options = new EnvelopesApi.ListStatusChangesOptions
                {
                    fromDate = StartDate.ToString("MM/dd/yyyy")
                };

                EnvelopesInformation envInfo = envelopesApi.ListStatusChanges(accountId, options);
                List<EnvelopeModel> allEnvelopes = new List<EnvelopeModel>();
                foreach (Envelope env in envInfo.Envelopes)
                {
                    // Get envelope custom fields
                    Dictionary<string, string> customFields = null;
                    if (env.CustomFields != null)
                    {
                        customFields = new Dictionary<string, string>();

                        foreach (ListCustomField list in env.CustomFields.ListCustomFields)
                            customFields.Add(list.Name, list.Value);

                        foreach (TextCustomField text in env.CustomFields.TextCustomFields)
                            customFields.Add(text.Name, text.Value);
                    }

                    // Get recipents data
                    List<EnvelopeRecipientModel> recipients = null;
                    if (env.Recipients != null && env.Recipients.Signers != null)
                    {
                        recipients = new List<EnvelopeRecipientModel>();

                        foreach (Signer s in env.Recipients.Signers)
                        {
                            recipients.Add(new EnvelopeRecipientModel
                            {
                                RecipientId = s.RecipientId,
                                ClientUserId = s.ClientUserId,
                                DeclinedDateTime = s.DeclinedDateTime,
                                DeclinedReason = s.DeclinedReason,
                                DeliveredDateTime = s.DeliveredDateTime,
                                Email = s.Email,
                                Name = s.Name,
                                RecipientType = s.RecipientType,
                                RoleName = s.RoleName,
                                SignatureName = s.SignatureInfo?.SignatureName,
                                SignedDateTime = s.SignedDateTime,
                                Status = s.Status,
                                DSUserGUID = s.UserId
                            });
                        }
                    }

                    allEnvelopes.Add(new EnvelopeModel { 
                        EnvelopeId = env.EnvelopeId,
                        Status = env.Status,
                        CompletedDateTime = env.CompletedDateTime,
                        CreatedDateTime = env.CreatedDateTime,
                        DeclinedDateTime = env.DeclinedDateTime,    
                        DeliveredDateTime = env.DeliveredDateTime,
                        EmailBlurb = env.EmailBlurb,
                        EmailSubject = env.EmailSubject,
                        ExpireAfter = int.Parse(env.ExpireAfter ?? "0"),
                        ExpireDateTime = env.ExpireDateTime,
                        ExpireEnabled = bool.Parse(env.ExpireEnabled ?? "false"),
                        LastModifiedDateTime = env.LastModifiedDateTime,
                        SentDateTime = env.SentDateTime,
                        StatusChangedDateTime = env.StatusChangedDateTime,
                        VoidedDateTime = env.VoidedDateTime,
                        VoidedReason = env.VoidedReason,
                        EnvelopeCustomFields = customFields,
                        EnvelopeRecipients = recipients
                    });
                }

                return allEnvelopes;
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
        /// Get the envelope object for a given enveloe ID.
        /// </summary>
        /// <param name="EnvelopeID">The envelope ID to retrieve</param>
        /// <returns>An EnvelopeModel object with the details of the speicified envelope</returns>
        /// <exception cref="System.Exception"></exception>
        public static EnvelopeModel DSGetEnvelope(string EnvelopeID)
        {
            try
            {
                // Validate input
                if (EnvelopeID == null)
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Return value
                EnvelopeModel retEnvelope = null;
                EnvelopesApi.GetEnvelopeOptions queryOptions = new EnvelopesApi.GetEnvelopeOptions
                {
                    include = "recipients, custom_fields"
                };

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                Envelope env = envelopesApi.GetEnvelope(accountId, EnvelopeID, queryOptions);

                if (env != null)                
                {
                    // Get envelope custom fields
                    Dictionary<string, string> customFields = null;
                    CustomFields envCustomFields = env.CustomFields;
                    if (envCustomFields != null)
                    {
                        customFields = new Dictionary<string, string>();

                        foreach (ListCustomField list in envCustomFields.ListCustomFields)
                            customFields.Add(list.Name, list.Value);

                        foreach (TextCustomField text in envCustomFields.TextCustomFields)
                            customFields.Add(text.Name, text.Value);
                    }

                    // Get recipents data
                    List<EnvelopeRecipientModel> recipients = null;
                    Recipients envRecipients = env.Recipients;
                    if (envRecipients != null && envRecipients.Signers != null)
                    {
                        recipients = new List<EnvelopeRecipientModel>();

                        foreach (Signer s in envRecipients.Signers)
                        {
                            recipients.Add(new EnvelopeRecipientModel
                            {
                                RecipientId = s.RecipientId,
                                ClientUserId = s.ClientUserId,
                                DeclinedDateTime = s.DeclinedDateTime,
                                DeclinedReason = s.DeclinedReason,
                                DeliveredDateTime = s.DeliveredDateTime,
                                Email = s.Email,
                                Name = s.Name,
                                RecipientType = s.RecipientType,
                                RoleName = s.RoleName,
                                SignatureName = s.SignatureInfo?.SignatureName,
                                SignedDateTime = s.SignedDateTime,
                                Status = s.Status,
                                DSUserGUID = s.UserId
                            });
                        }
                    }

                    retEnvelope = new EnvelopeModel
                    {
                        EnvelopeId = env.EnvelopeId,
                        Status = env.Status,
                        CompletedDateTime = env.CompletedDateTime,
                        CreatedDateTime = env.CreatedDateTime,
                        DeclinedDateTime = env.DeclinedDateTime,
                        DeliveredDateTime = env.DeliveredDateTime,
                        EmailBlurb = env.EmailBlurb,
                        EmailSubject = env.EmailSubject,
                        ExpireAfter = int.Parse(env.ExpireAfter ?? "0"),
                        ExpireDateTime = env.ExpireDateTime,
                        ExpireEnabled = bool.Parse(env.ExpireEnabled ?? "false"),
                        LastModifiedDateTime = env.LastModifiedDateTime,
                        SentDateTime = env.SentDateTime,
                        StatusChangedDateTime = env.StatusChangedDateTime,
                        VoidedDateTime = env.VoidedDateTime,
                        VoidedReason = env.VoidedReason,
                        EnvelopeCustomFields = customFields,
                        EnvelopeRecipients = recipients
                    };
                }

                return retEnvelope;
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
        /// Get a list of all DocuSign envelope recipients for a given envelope ID.
        /// </summary>
        /// <param name="EnvelopeID">The envelope ID to query</param>
        /// <returns>List of all recipients for the envelope as a list of RecipientModel objects</returns>
        /// <exception cref="System.Exception"></exception>
        public static List<EnvelopeRecipientModel> DSGetAllRecipients(string EnvelopeID)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(EnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                Recipients results = envelopesApi.ListRecipients(accountId, EnvelopeID);

                // Return value
                List<EnvelopeRecipientModel> recipients = new List<EnvelopeRecipientModel>();
                foreach(Signer s in results.Signers)
                {
                    recipients.Add(new EnvelopeRecipientModel {
                        RecipientId = s.RecipientId,
                        ClientUserId = s.ClientUserId,
                        DeclinedDateTime =  s.DeclinedDateTime,
                        DeclinedReason =  s.DeclinedReason,
                        DeliveredDateTime =  s.DeliveredDateTime,
                        Email = s.Email,
                        Name = s.Name,
                        RecipientType = s.RecipientType,
                        RoleName = s.RoleName,
                        SignatureName = s.SignatureInfo?.SignatureName,
                        SignedDateTime = s.SignedDateTime,
                        Status = s.Status,
                        DSUserGUID = s.UserId
                    });
                }

                return recipients;
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
        /// Check the status of the DocuSign signature ceremony.
        /// </summary>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <returns>Status of the document (any, created, deleted, sent, delivered, signed, completed, declined, timedout, voided)</returns>
        /// <exception cref="System.Exception"></exception>
        public static string DSCheckStatus(string EnvelopeID)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(EnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                Envelope results = envelopesApi.GetEnvelope(accountId, EnvelopeID);
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
        /// Void, or cancel, an envelope that was sent to a recipient. *** THIS ACTION IS IRREVERSIBLE! ***
        /// Note: You cannot void draft or completed envelopes.
        /// </summary>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="VoidedReason">Reason for voiding the envelope</param>
        /// <returns>boolean</returns>
        /// <exception cref="System.Exception"></exception>
        public static bool DSVoidEnvelope(string EnvelopeID, string VoidedReason)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(EnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                if (string.IsNullOrEmpty(VoidedReason))
                    throw new Exception(Resources.EMPTY_VOIDED_REASON);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                Envelope envelope = new Envelope
                {
                    Status = "voided",
                    VoidedReason = VoidedReason,
                };

                envelopesApi.Update(accountId, EnvelopeID, envelope);
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
        /// Resend a previously sent DocuSign envelope (in 'created', 'sent' or 'delivered' states).
        /// </summary>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <returns>Boolean result from DocuSign call</returns>
        /// <exception cref="System.Exception"></exception>
        public static bool DSResendEnvelope(string EnvelopeID)
        {
            // Return error if EnvelopeId is empty
            if (string.IsNullOrEmpty(EnvelopeID))
                throw new Exception(Resources.EMPTY_ENVELOPE_ID);

            // Check config
            if (!DocuSignConfig.Ready)
                throw new Exception(Resources.DSCONFIG_NOT_SET);

            // 1. Create envelope request object
            // Read config values
            string accountId = DocuSignConfig.AccountID;

            Envelope envelope = new Envelope
            {
                Status = EnvelopeStatus.STATUS_SENT
            };

            EnvelopesApi.UpdateOptions updateOptions = new EnvelopesApi.UpdateOptions
            {
                resendEnvelope = "true"
            };

            // 2. Use the SDK to 'resend' the envelope by calling the update method
            // Create API Client and call it
            EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
            EnvelopeUpdateSummary updateSummary = envelopesApi.Update(accountId, EnvelopeID, envelope, updateOptions);

            // If error details is not null, then error occurred  
            if (updateSummary.ErrorDetails != null)
            {
                throw new Exception(updateSummary.ErrorDetails.Message);
            }

            // return EnvelopeSummary back to caller
            return true;
        }

        /// <summary>
        /// Retrieve info about the documents contained in a DocuSign envelope. If envelope was signed, the signer's DocuSign certificate is appended to this list.
        /// </summary>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <returns>List of all documents in the requested envelope as EnvelopeDocumentModel objects</returns>
        /// <exception cref="System.Exception"></exception>
        public static List<EnvelopeDocumentModel> DSGetAllDocuments(string EnvelopeID)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(EnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, EnvelopeID);

                List<EnvelopeDocumentModel> retDoc = new List<EnvelopeDocumentModel>();
                foreach (EnvelopeDocument doc in docList.EnvelopeDocuments)
                {
                    retDoc.Add(new EnvelopeDocumentModel
                    {
                        DocumentId = doc.DocumentId,
                        EnvelopeId = docList.EnvelopeId,
                        Order = doc.Order,
                        Name = doc.Name,
                        Type = doc.Type,
                        DSDocumentGUID = doc.DocumentIdGuid
                    });
                }

                return retDoc;
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
        /// Retrieve a document from DocuSign.
        /// </summary>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DSDocumentID">ID of the document within the DocuSign envelope</param>
        /// <returns>File stream of the document retrieved from DocuSign</returns>
        /// <exception cref="System.Exception"></exception>
        public static Stream DSGetDocument(string EnvelopeID, string DSDocumentID)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(EnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                if (string.IsNullOrEmpty(DSDocumentID))
                    throw new Exception(Resources.EMPTY_DOCUMENT_ID);

                // Check config                
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                Stream results = envelopesApi.GetDocument(accountId, EnvelopeID, DSDocumentID);

                // Return for download
                return results;
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
        /// Retrieve the user selected value of a named check-box within a DocuSign document.
        /// </summary>
        /// <param name="TabLabel">Tab label of the check box in the DocuSign template</param>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>Returns the check box's selected value as a string ("true" or "false")</returns>
        /// <exception cref="System.Exception"></exception>
        public static string DSGetDocumentCheckBoxField(string TabLabel, string EnvelopeID, string DocumentID = "1")
        {
            try
            {
                string retVal = GetDocumentTabValue(TabLabel, EnvelopeID, DocumentID, TabTypes.Checkbox);
                return retVal;
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
        /// Retrieve the user provided value of a named text field within a DocuSign document.
        /// </summary>
        /// <param name="TabLabel">Tab label of the text field in the DocuSign template</param>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>User entered text in the field</returns>
        /// <exception cref="System.Exception"></exception>
        public static string DSGetDocumentTextField(string TabLabel, string EnvelopeID, string DocumentID = "1")
        {
            try
            {
                string retVal = GetDocumentTabValue(TabLabel, EnvelopeID, DocumentID, TabTypes.Text);
                return retVal;
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
        /// Retrieve the user provided value for a named SSN field within a DocuSign document.
        /// </summary>
        /// <param name="TabLabel">Tab label of the SSN field in the DocuSign template</param>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>User entered SSN text in the field</returns>
        /// <exception cref="System.Exception"></exception>
        public static string DSGetDocumentSsnField(string TabLabel, string EnvelopeID, string DocumentID = "1")
        {
            try
            {
                string retVal = GetDocumentTabValue(TabLabel, EnvelopeID, DocumentID, TabTypes.Ssn);
                return retVal;
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
        /// Retrieve the value for a named date of signature field within a DocuSign document.
        /// </summary>
        /// <param name="TabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>Date signed as set in the named field</returns>
        /// <exception cref="System.Exception"></exception>
        public static string DSGetDocumentDateSignedField(string TabLabel, string EnvelopeID, string DocumentID = "1")
        {
            try
            {
                string retVal = GetDocumentTabValue(TabLabel, EnvelopeID, DocumentID, TabTypes.DateSigned);
                return retVal;
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
        /// Retrieve the user provided value for a named date field within a DocuSign document.
        /// </summary>
        /// <param name="TabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>User provided date value in the named field</returns>
        /// <exception cref="System.Exception"></exception>
        public static string DSGetDocumentDateField(string TabLabel, string EnvelopeID, string DocumentID = "1")
        {
            try
            {
                string retVal = GetDocumentTabValue(TabLabel, EnvelopeID, DocumentID, TabTypes.Date);
                return retVal;
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
        /// Retrieve the user provided value for a named First Name field within a DocuSign document.
        /// </summary>
        /// <param name="TabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>User provided first name in the named field</returns>
        /// <exception cref="System.Exception"></exception>
        public static string DSGetDocumentFirstNameField(string TabLabel, string EnvelopeID, string DocumentID = "1")
        {
            try
            {
                string retVal = GetDocumentTabValue(TabLabel, EnvelopeID, DocumentID, TabTypes.FirstName);
                return retVal;
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
        /// Retrieve the user provided value for a named Last Name field within a DocuSign document.
        /// </summary>
        /// <param name="TabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>User provided last name value in the named field</returns>
        /// <exception cref="System.Exception"></exception>
        public static string DSGetDocumentLastNameField(string TabLabel, string EnvelopeID, string DocumentID = "1")
        {
            try
            {
                string retVal = GetDocumentTabValue(TabLabel, EnvelopeID, DocumentID, TabTypes.LastName);
                return retVal;
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
        /// Retrieve the user signature status for a named sign-here field within a DocuSign document.
        /// </summary>
        /// <param name="TabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>Returns one of: "active", "signed", "declined" or "na" (e.g. for optional signature fields)</returns>
        /// <exception cref="System.Exception"></exception>
        public static string DSGetDocumentSignHereField(string TabLabel, string EnvelopeID, string DocumentID = "1")
        {
            try
            {
                string retVal = GetDocumentTabValue(TabLabel, EnvelopeID, DocumentID, TabTypes.SignHere);
                return retVal;
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
        /// Retrieve the user selected value within a drop-down field within a DocuSign document.
        /// </summary>
        /// <param name="TabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>Returns one of: "active", "signed", "declined" or "na" (e.g. for optional signature fields)</returns>
        /// <exception cref="System.Exception"></exception>
        public static string DSGetDocumentListField(string TabLabel, string EnvelopeID, string DocumentID = "1")
        {
            try
            {
                string retVal = GetDocumentTabValue(TabLabel, EnvelopeID, DocumentID, TabTypes.List);
                return retVal;
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
        /// Get a list of all DocuSign envelope IDs included in a given batch.
        /// </summary>
        /// <param name="BulkSendBatchID">The batch ID to get the envelopes for</param>
        /// <returns>List of all envelopes from the requested batch as EnvelopeModel objects</returns>
        /// <exception cref="System.Exception"></exception>
        public static List<EnvelopeModel> DSGetBulkBatchEnvelopes(string BulkSendBatchID)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(BulkSendBatchID))
                    throw new Exception(Resources.EMPTY_BATCHID_VALUE);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Set up options to return recipient info
                BulkEnvelopesApi.GetBulkSendBatchEnvelopesOptions queryOptions = new BulkEnvelopesApi.GetBulkSendBatchEnvelopesOptions
                {
                    include = "recipients"
                };

                // Create BulkEnvelopes API Client and call it
                BulkEnvelopesApi bulkEnvelopesApi = Authenticate.CreateBulkEnvelopesApiClient();
                EnvelopesInformation envInfo = bulkEnvelopesApi.GetBulkSendBatchEnvelopes(accountId, BulkSendBatchID, queryOptions);

                // No envelopes available to process (batch may be in process; it is too early to check)
                if (envInfo.Envelopes == null)
                    return null;

                List<EnvelopeModel> allEnvelopes = new List<EnvelopeModel>();
                foreach (Envelope env in envInfo.Envelopes)
                {
                    Dictionary<string, string> customFields = null;
                    if (env.CustomFields != null)
                    {
                        customFields = new Dictionary<string, string>();
                        foreach (ListCustomField list in env.CustomFields.ListCustomFields)
                            customFields.Add(list.Name, list.Value);

                        foreach (TextCustomField text in env.CustomFields.TextCustomFields)
                            customFields.Add(text.Name, text.Value);
                    }

                    // Get recipents data
                    List<EnvelopeRecipientModel> recipients = null;
                    if (env.Recipients != null && env.Recipients.Signers != null)
                    {
                        recipients = new List<EnvelopeRecipientModel>();

                        foreach (Signer s in env.Recipients.Signers)
                        {
                            recipients.Add(new EnvelopeRecipientModel
                            {
                                RecipientId = s.RecipientId,
                                ClientUserId = s.ClientUserId,
                                DeclinedDateTime = s.DeclinedDateTime,
                                DeclinedReason = s.DeclinedReason,
                                DeliveredDateTime = s.DeliveredDateTime,
                                Email = s.Email,
                                Name = s.Name,
                                RecipientType = s.RecipientType,
                                RoleName = s.RoleName,
                                SignatureName = s.SignatureInfo?.SignatureName,
                                SignedDateTime = s.SignedDateTime,
                                Status = s.Status,
                                DSUserGUID = s.UserId
                            });
                        }
                    }

                    allEnvelopes.Add(new EnvelopeModel
                    {
                        EnvelopeId = env.EnvelopeId,
                        Status = env.Status,
                        CompletedDateTime = env.CompletedDateTime,
                        CreatedDateTime = env.CreatedDateTime,
                        DeclinedDateTime = env.DeclinedDateTime,
                        DeliveredDateTime = env.DeliveredDateTime,
                        EmailBlurb = env.EmailBlurb,
                        EmailSubject = env.EmailSubject,
                        ExpireAfter = int.Parse(env.ExpireAfter ?? "0"),
                        ExpireDateTime = env.ExpireDateTime,
                        ExpireEnabled = bool.Parse(env.ExpireEnabled ?? "false"),
                        LastModifiedDateTime = env.LastModifiedDateTime,
                        SentDateTime = env.SentDateTime,
                        StatusChangedDateTime = env.StatusChangedDateTime,
                        VoidedDateTime = env.VoidedDateTime,
                        VoidedReason = env.VoidedReason,
                        EnvelopeCustomFields = customFields,
                        EnvelopeRecipients = recipients
                    });
                }

                return allEnvelopes;
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
        /// Retrieve the custom fields from a DocuSign envelope
        /// </summary>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <returns>Key pair dictionary of custom field IDs and values</returns>
        /// <exception cref="System.Exception"></exception>
        public static Dictionary<string, string> DSGetEnvelopeCustomFields(string EnvelopeID)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(EnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                Dictionary<string, string> retDict = new Dictionary<string, string>();

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                CustomFieldsEnvelope customFields = envelopesApi.ListCustomFields(accountId, EnvelopeID);

                foreach(ListCustomField list in customFields.ListCustomFields)
                    retDict.Add(list.Name , list.Value);

                foreach(TextCustomField text in customFields.TextCustomFields)
                    retDict.Add(text.Name, text.Value);

                return retDict;
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

        #endregion PUBLIC_METHODS


    #region PRIVATES

        private enum TabTypes
        {
            Checkbox,
            Text,
            Ssn,
            Date,
            DateSigned,
            FirstName,
            LastName,
            List,
            SignHere
        } 

        private static string GetDocumentTabValue(string TabLabel, string EnvelopeID, string DocumentID, TabTypes tabType)
        {
            // Validate inputs
            if (string.IsNullOrEmpty(TabLabel))
                throw new Exception(Resources.EMPTY_TAB_LABEL);

            if (string.IsNullOrEmpty(EnvelopeID))
                throw new Exception(Resources.EMPTY_ENVELOPE_ID);

            if (string.IsNullOrEmpty(DocumentID))
                throw new Exception(Resources.EMPTY_DOCUMENT_ID);

            // Check config
            if (!DocuSignConfig.Ready)
                throw new Exception(Resources.DSCONFIG_NOT_SET);

            string retVal = "";

            // Read account ID from config
            string accountId = DocuSignConfig.AccountID;

            // Create API Client and call it
            EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
            Tabs tabs = envelopesApi.GetDocumentTabs(accountId, EnvelopeID, DocumentID);

            if (tabs != null)
            {
                switch (tabType)
                {
                    case TabTypes.Checkbox:
                        // Get checkbox selection
                        if (tabs.CheckboxTabs.Count > 0)
                        {
                            Checkbox consent = tabs.CheckboxTabs.Find(x => x.TabLabel == TabLabel);
                            if (consent == null)
                                throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                            retVal = string.IsNullOrEmpty(consent.Selected) ? "false" : consent.Selected;
                        }
                        break;

                    case TabTypes.Text:
                        // Get text field
                        if (tabs.TextTabs.Count > 0)
                        {
                            Text textTab = tabs.TextTabs.Find(x => x.TabLabel == TabLabel);
                            if (textTab == null)
                                throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                            retVal = textTab.Value;
                        }
                        break;

                    case TabTypes.Ssn:
                        // Get SSN field
                        if (tabs.SsnTabs.Count > 0)
                        {
                            Ssn ssnTab = tabs.SsnTabs.Find(x => x.TabLabel == TabLabel);
                            if (ssnTab == null)
                                throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                            retVal = ssnTab.Value;
                        }
                        break;

                    case TabTypes.DateSigned:
                        // Get DateSigned field
                        if (tabs.DateSignedTabs.Count > 0)
                        {
                            DateSigned dtSignedTab = tabs.DateSignedTabs.Find(x => x.TabLabel == TabLabel);
                            if (dtSignedTab == null)
                                throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                            retVal = dtSignedTab.Value;
                        }
                        break;

                    case TabTypes.Date:
                        // Get Date field
                        if (tabs.DateTabs.Count > 0)
                        {
                            Date dtTab = tabs.DateTabs.Find(x => x.TabLabel == TabLabel);
                            if (dtTab == null)
                                throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                            retVal = dtTab.Value;
                        }
                        break;

                    case TabTypes.FirstName:
                        // Get first name field
                        if (tabs.FirstNameTabs.Count > 0)
                        {
                            FirstName fnTab = tabs.FirstNameTabs.Find(x => x.TabLabel == TabLabel);
                            if (fnTab == null)
                                throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                            retVal = fnTab.Value;
                        }
                        break;

                    case TabTypes.LastName:
                        // Get last name field
                        if (tabs.LastNameTabs.Count > 0)
                        {
                            LastName lnTab = tabs.LastNameTabs.Find(x => x.TabLabel == TabLabel);
                            if (lnTab == null)
                                throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                            retVal = lnTab.Value;
                        }
                        break;

                    case TabTypes.List:
                        // Get drop down field 
                        if (tabs.ListTabs.Count > 0)
                        {
                            List listTab = tabs.ListTabs.Find(x => x.TabLabel == TabLabel);
                            if (listTab == null)
                                throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                            retVal = listTab.ListSelectedValue;
                        }
                        break;

                    case TabTypes.SignHere:
                        // Get SignHere field
                        if (tabs.SignHereTabs.Count > 0)
                        {
                            SignHere signTab = tabs.SignHereTabs.Find(x => x.TabLabel == TabLabel);
                            if (signTab == null)
                                throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                            retVal = signTab.Status;
                        }
                        break;
                }
            }
            return retVal;
        }

        #endregion PRIVATES
    }
}
