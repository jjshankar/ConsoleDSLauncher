using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Text;
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
        /// <param name="DSStartDate">The date to start querying from</param>
        /// <returns></returns>
        public static List<string> DSGetAllEnvelopes(DateTime DSStartDate)
        {
            try
            {
                // Validate input
                if (DSStartDate == null)
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
                    fromDate = DSStartDate.ToString("MM/dd/yyyy")
                };

                EnvelopesInformation envInfo = envelopesApi.ListStatusChanges(accountId, options);
                List<string> allEnvelopes = new List<string>();
                foreach (Envelope env in envInfo.Envelopes)
                {
                    allEnvelopes.Add(env.EnvelopeId);
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
        /// Check the status of the DocuSign signature ceremony.
        /// </summary>
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <returns>Status of the document (any, created, deleted, sent, delivered, signed, completed, declined, timedout, voided)</returns>
        public static string DSCheckStatus(string DSEnvelopeID)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(DSEnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                Envelope results = envelopesApi.GetEnvelope(accountId, DSEnvelopeID);
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
        /// Retrieve info about the document contained in a DocuSign envelope (only supports envelopes with single document).
        /// </summary>
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <returns></returns>
        public static DocumentModel DSGetDocInfo(string DSEnvelopeID)
        {
            try
            {
                // Validate input
                if (string.IsNullOrEmpty(DSEnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                DocumentModel retDoc = null;

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                TemplateInformation templates = envelopesApi.ListTemplates(accountId, DSEnvelopeID);
                Envelope results = envelopesApi.GetEnvelope(accountId, DSEnvelopeID);
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, DSEnvelopeID);
                Recipients signer = envelopesApi.ListRecipients(accountId, DSEnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    retDoc = new DocumentModel
                    {
                        SignerEmail = signer.Signers[0].Email,
                        SignerName = signer.Signers[0].Name,
                        SignerId = signer.Signers[0].ClientUserId,
                        DSDocumentId = docList.EnvelopeDocuments[0].DocumentId,
                        DSEmailSubject = results.EmailSubject,
                        DSEnvelopeId = docList.EnvelopeId,
                        DSRoleName = signer.Signers[0].RoleName,
                        DSTemplateName = templates.Templates[0].Name
                    };
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
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DSDocumentID">ID of the document within the DocuSign envelope</param>
        /// <returns>File stream of the document retrieved from DocuSign</returns>
        public static Stream DSGetDocument(string DSEnvelopeID, string DSDocumentID)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(DSEnvelopeID))
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
                Stream results = envelopesApi.GetDocument(accountId, DSEnvelopeID, DSDocumentID);

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
        /// <param name="DSTabLabel">Tab label of the check box in the DocuSign template</param>
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DSDocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>Returns the check box's selected value as a string ("true" or "false")</returns>
        public static string DSGetDocumentCheckBoxField(string DSTabLabel, string DSEnvelopeID, string DSDocumentID = "0")
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(DSTabLabel))
                    throw new Exception(Resources.EMPTY_TAB_LABEL);

                if (string.IsNullOrEmpty(DSEnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                if (string.IsNullOrEmpty(DSDocumentID))
                    throw new Exception(Resources.EMPTY_DOCUMENT_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                string retVal = "";

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, DSEnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, DSEnvelopeID,
                        (DSDocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DSDocumentID);
                    if (tabs.CheckboxTabs.Count > 0)
                    {
                        Checkbox consent = tabs.CheckboxTabs.Find(x => x.TabLabel == DSTabLabel);
                        if (consent == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, DSTabLabel));

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
        /// <param name="DSTabLabel">Tab label of the text field in the DocuSign template</param>
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DSDocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>User entered text in the field</returns>
        public static string DSGetDocumentTextField(string DSTabLabel, string DSEnvelopeID, string DSDocumentID = "0")
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(DSTabLabel))
                    throw new Exception(Resources.EMPTY_TAB_LABEL);

                if (string.IsNullOrEmpty(DSEnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                if (string.IsNullOrEmpty(DSDocumentID))
                    throw new Exception(Resources.EMPTY_DOCUMENT_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                string retVal = "";

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, DSEnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, DSEnvelopeID,
                        (DSDocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DSDocumentID);
                    if (tabs.TextTabs.Count > 0)
                    {
                        Text textTab = tabs.TextTabs.Find(x => x.TabLabel == DSTabLabel);
                        if (textTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, DSTabLabel));

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
        /// <param name="DSTabLabel">Tab label of the SSN field in the DocuSign template</param>
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DSDocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>User entered SSN text in the field</returns>
        public static string DSGetDocumentSsnField(string DSTabLabel, string DSEnvelopeID, string DSDocumentID = "0")
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(DSTabLabel))
                    throw new Exception(Resources.EMPTY_TAB_LABEL);

                if (string.IsNullOrEmpty(DSEnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                if (string.IsNullOrEmpty(DSDocumentID))
                    throw new Exception(Resources.EMPTY_DOCUMENT_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                string retVal = "";

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, DSEnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, DSEnvelopeID,
                        (DSDocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DSDocumentID);
                    if (tabs.SsnTabs.Count > 0)
                    {
                        Ssn ssnTab = tabs.SsnTabs.Find(x => x.TabLabel == DSTabLabel);
                        if (ssnTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, DSTabLabel));

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
        /// <param name="DSTabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DSDocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>Date signed as set in the named field</returns>
        public static string DSGetDocumentDateSignedField(string DSTabLabel, string DSEnvelopeID, string DSDocumentID = "0")
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(DSTabLabel))
                    throw new Exception(Resources.EMPTY_TAB_LABEL);

                if (string.IsNullOrEmpty(DSEnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                if (string.IsNullOrEmpty(DSDocumentID))
                    throw new Exception(Resources.EMPTY_DOCUMENT_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                string retVal = "";

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, DSEnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, DSEnvelopeID,
                        (DSDocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DSDocumentID);
                    if (tabs.DateSignedTabs.Count > 0)
                    {
                        DateSigned dtSignedTab = tabs.DateSignedTabs.Find(x => x.TabLabel == DSTabLabel);
                        if (dtSignedTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, DSTabLabel));

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
        /// <param name="DSTabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DSDocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>User provided date value in the named field</returns>
        public static string DSGetDocumentDateField(string DSTabLabel, string DSEnvelopeID, string DSDocumentID = "0")
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(DSTabLabel))
                    throw new Exception(Resources.EMPTY_TAB_LABEL);

                if (string.IsNullOrEmpty(DSEnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                if (string.IsNullOrEmpty(DSDocumentID))
                    throw new Exception(Resources.EMPTY_DOCUMENT_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                string retVal = "";

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, DSEnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, DSEnvelopeID,
                        (DSDocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DSDocumentID);
                    if (tabs.DateTabs.Count > 0)
                    {
                        Date dtTab = tabs.DateTabs.Find(x => x.TabLabel == DSTabLabel);
                        if (dtTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, DSTabLabel));

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
        /// <param name="DSTabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DSDocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>User provided first name in the named field</returns>
        public static string DSGetDocumentFirstNameField(string DSTabLabel, string DSEnvelopeID, string DSDocumentID = "0")
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(DSTabLabel))
                    throw new Exception(Resources.EMPTY_TAB_LABEL);

                if (string.IsNullOrEmpty(DSEnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                if (string.IsNullOrEmpty(DSDocumentID))
                    throw new Exception(Resources.EMPTY_DOCUMENT_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                string retVal = "";

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, DSEnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, DSEnvelopeID,
                        (DSDocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DSDocumentID);
                    if (tabs.FirstNameTabs.Count > 0)
                    {
                        FirstName fnTab = tabs.FirstNameTabs.Find(x => x.TabLabel == DSTabLabel);
                        if (fnTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, DSTabLabel));

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
        /// <param name="DSTabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DSDocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>User provided last name value in the named field</returns>
        public static string DSGetDocumentLastNameField(string DSTabLabel, string DSEnvelopeID, string DSDocumentID = "0")
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(DSTabLabel))
                    throw new Exception(Resources.EMPTY_TAB_LABEL);

                if (string.IsNullOrEmpty(DSEnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                if (string.IsNullOrEmpty(DSDocumentID))
                    throw new Exception(Resources.EMPTY_DOCUMENT_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                string retVal = "";

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, DSEnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, DSEnvelopeID,
                        (DSDocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DSDocumentID);
                    if (tabs.LastNameTabs.Count > 0)
                    {
                        LastName lnTab = tabs.LastNameTabs.Find(x => x.TabLabel == DSTabLabel);
                        if (lnTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, DSTabLabel));

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
        /// <param name="DSTabLabel">Tab label of the date field in the DocuSign template</param>
        /// <param name="DSEnvelopeID">GUID of the DocuSign envelope sent to the signer</param>
        /// <param name="DSDocumentID">Document ID (optional; if not passed, defaults to first document in the envelope)</param>
        /// <returns>Returns one of: "active", "signed", "declined" or "na" (e.g. for optional signature fields)</returns>
        public static string DSGetDocumentSignHereField(string DSTabLabel, string DSEnvelopeID, string DSDocumentID = "0")
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrEmpty(DSTabLabel))
                    throw new Exception(Resources.EMPTY_TAB_LABEL);

                if (string.IsNullOrEmpty(DSEnvelopeID))
                    throw new Exception(Resources.EMPTY_ENVELOPE_ID);

                if (string.IsNullOrEmpty(DSDocumentID))
                    throw new Exception(Resources.EMPTY_DOCUMENT_ID);

                // Check config
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                string retVal = "";

                // Read account ID from config
                string accountId = DocuSignConfig.AccountID;

                // Create API Client and call it
                EnvelopesApi envelopesApi = Authenticate.CreateEnvelopesApiClient();
                EnvelopeDocumentsResult docList = envelopesApi.ListDocuments(accountId, DSEnvelopeID);

                if (docList.EnvelopeDocuments.Count > 0)
                {
                    // Get checkbox selection
                    Tabs tabs = envelopesApi.GetDocumentTabs(accountId, DSEnvelopeID,
                        (DSDocumentID == "0") ? docList.EnvelopeDocuments[0].DocumentId : DSDocumentID);
                    if (tabs.SignHereTabs.Count > 0)
                    {
                        SignHere signTab = tabs.SignHereTabs.Find(x => x.TabLabel == DSTabLabel);
                        if (signTab == null)
                            throw new Exception(string.Format(Resources.FIELD_x_NOT_FOUND, DSTabLabel));

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
