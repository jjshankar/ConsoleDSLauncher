using System;
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
                    allEnvelopes.Add(new EnvelopeModel { 
                        EnvelopeId = env.EnvelopeId,
                        Status = env.Status,
                        CompletedDateTime = env.CompletedDateTime,
                        CreatedDateTime = env.CreatedDateTime,
                        DeclinedDateTime = env.DeclinedDateTime,    
                        DeliveredDateTime = env.DeliveredDateTime,
                        EmailBlurb = env.EmailBlurb,
                        EmailSubject = env.EmailSubject,
                        ExpireAfter = int.Parse(env.ExpireAfter),
                        ExpireDateTime = env.ExpireDateTime,
                        ExpireEnabled = bool.Parse(env.ExpireEnabled),
                        LastModifiedDateTime = env.LastModifiedDateTime,
                        SentDateTime = env.SentDateTime,
                        StatusChangedDateTime = env.StatusChangedDateTime,
                        VoidedDateTime = env.VoidedDateTime,
                        VoidedReason = env.VoidedReason                        
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
                List<EnvelopeRecipientModel> recipents = new List<EnvelopeRecipientModel>();
                foreach(Signer s in results.Signers)
                {
                    recipents.Add(new EnvelopeRecipientModel {
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

                return recipents;
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
        /// Retrieve info about the documents contained in a DocuSign envelope.
        /// If envelope was signed, the signer's DocuSign certificate is appended to this list.
        /// </summary>
        /// <param name="EnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <returns>List of all documents in the requested envelope as EnvelopeDocumentModel objects.</returns>
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
        public static string DSGetDocumentCheckBoxField(string TabLabel, string EnvelopeID, string DocumentID = "0")
        {
            try
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
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, EnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, EnvelopeID,
                        (DocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DocumentID);
                    if (tabs.CheckboxTabs.Count > 0)
                    {
                        Checkbox consent = tabs.CheckboxTabs.Find(x => x.TabLabel == TabLabel);
                        if (consent == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                        retVal = string.IsNullOrEmpty(consent.Selected) ? "false" : consent.Selected;
                    }
                }
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
        public static string DSGetDocumentTextField(string TabLabel, string EnvelopeID, string DocumentID = "0")
        {
            try
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
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, EnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, EnvelopeID,
                        (DocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DocumentID);
                    if (tabs.TextTabs.Count > 0)
                    {
                        Text textTab = tabs.TextTabs.Find(x => x.TabLabel == TabLabel);
                        if (textTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                        retVal = textTab.Value;
                    }
                }
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
        public static string DSGetDocumentSsnField(string TabLabel, string EnvelopeID, string DocumentID = "0")
        {
            try
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
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, EnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, EnvelopeID,
                        (DocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DocumentID);
                    if (tabs.SsnTabs.Count > 0)
                    {
                        Ssn ssnTab = tabs.SsnTabs.Find(x => x.TabLabel == TabLabel);
                        if (ssnTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                        retVal = ssnTab.Value;
                    }
                }
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
        public static string DSGetDocumentDateSignedField(string TabLabel, string EnvelopeID, string DocumentID = "0")
        {
            try
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
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, EnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, EnvelopeID,
                        (DocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DocumentID);
                    if (tabs.DateSignedTabs.Count > 0)
                    {
                        DateSigned dtSignedTab = tabs.DateSignedTabs.Find(x => x.TabLabel == TabLabel);
                        if (dtSignedTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                        retVal = dtSignedTab.Value;
                    }
                }
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
        public static string DSGetDocumentDateField(string TabLabel, string EnvelopeID, string DocumentID = "0")
        {
            try
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
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, EnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, EnvelopeID,
                        (DocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DocumentID);
                    if (tabs.DateTabs.Count > 0)
                    {
                        Date dtTab = tabs.DateTabs.Find(x => x.TabLabel == TabLabel);
                        if (dtTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                        retVal = dtTab.Value;
                    }
                }
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
        public static string DSGetDocumentFirstNameField(string TabLabel, string EnvelopeID, string DocumentID = "0")
        {
            try
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
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, EnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, EnvelopeID,
                        (DocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DocumentID);
                    if (tabs.FirstNameTabs.Count > 0)
                    {
                        FirstName fnTab = tabs.FirstNameTabs.Find(x => x.TabLabel == TabLabel);
                        if (fnTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                        retVal = fnTab.Value;
                    }
                }
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
        public static string DSGetDocumentLastNameField(string TabLabel, string EnvelopeID, string DocumentID = "0")
        {
            try
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
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, EnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, EnvelopeID,
                        (DocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DocumentID);
                    if (tabs.LastNameTabs.Count > 0)
                    {
                        LastName lnTab = tabs.LastNameTabs.Find(x => x.TabLabel == TabLabel);
                        if (lnTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                        retVal = lnTab.Value;
                    }
                }
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
        public static string DSGetDocumentSignHereField(string TabLabel, string EnvelopeID, string DocumentID = "0")
        {
            try
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
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, EnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, EnvelopeID,
                        (DocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DocumentID);
                    if (tabs.SignHereTabs.Count > 0)
                    {
                        SignHere signTab = tabs.SignHereTabs.Find(x => x.TabLabel == TabLabel);
                        if (signTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, TabLabel));

                        retVal = signTab.Status;
                    }
                }
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
    }
}
