using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using DocuSign.eSign.Model;
using ECAR.DocuSign.Models;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;

namespace ConsoleLauncher
{
    public class DocuSignTester
    {
        private static Dictionary<string, string> Templates = new Dictionary<string, string>();

        private static string GetTemplateId(DocuSignClient apiClient, string accountId, string templateName)
        {
            string templateId = "";

            if (Templates.Count > 0 && Templates.ContainsKey(templateName))
            {
                Templates.TryGetValue(templateName, out templateId);
            }
            else
            {
                // Get the template ID by calling the Templates API
                TemplatesApi templatesApi = new TemplatesApi(apiClient);
                TemplatesApi.ListTemplatesOptions options = new TemplatesApi.ListTemplatesOptions { searchText = templateName };
                EnvelopeTemplateResults searchResults = templatesApi.ListTemplates(accountId, options);


                // Process results
                if (int.Parse(searchResults.ResultSetSize) > 0)
                {
                    // Found the template! Record its id
                    templateId = searchResults.EnvelopeTemplates[0].TemplateId;
                    Templates.Add(templateName, templateId);
                }
                else
                    throw new ApiException(404, "Template not found: " + templateName);
            }
            return templateId;
        }

        private static CompositeTemplate CreateCompositeTemplate(DocumentModel doc, string templateId, string seqNum)
        {
            // Create the signer recipient object 
            Signer signer = new Signer
            {
                Email = doc.SignerEmail,
                Name = doc.SignerName,
                // ClientUserId = doc.SignerId,     // Including a client user ID automatically specifies the signer as embedded
                RecipientId = "1",
                RoleName = doc.DSRoleName,
            };

            // Create recipients object
            Recipients recipients = new Recipients { Signers = new List<Signer> { signer } };

            // Create a composite template for the Server Template
            CompositeTemplate compTemplate = new CompositeTemplate
            {
                CompositeTemplateId = seqNum
            };
            ServerTemplate serverTemplates = new ServerTemplate
            {
                Sequence = seqNum,
                TemplateId = templateId    // TemplateId for the doc in DocuSign
            };

            compTemplate.ServerTemplates = new List<ServerTemplate> { serverTemplates };

            // Add the roles via an inlineTemplate
            InlineTemplate inlineTemplate = new InlineTemplate
            {
                Sequence = seqNum,
                Recipients = recipients
            };

            compTemplate.InlineTemplates = new List<InlineTemplate> { inlineTemplate };

            return compTemplate;
        }

        public static void SendBulkTemplate(List<DocumentModel> docList, string templateName)
        {
            try
            {
                List<BulkSendingCopy> bulkCopies = new List<BulkSendingCopy>();

                foreach (DocumentModel doc in docList)
                {
                    BulkSendingCopy copy = new BulkSendingCopy
                    {
                        Recipients = new List<BulkSendingCopyRecipient>(),
                        EmailSubject = "Batch Send",
                        EmailBlurb = "This document was sent as a batch",
                    };

                    BulkSendingCopyRecipient recipient = new BulkSendingCopyRecipient
                    {
                        Name = doc.SignerName,
                        //ClientUserId = doc.SignerId,
                        Email = doc.SignerEmail,
                        //RecipientId = (1 + docList.IndexOf(doc)).ToString(),  // don't use RecipientId for bulk send
                        RoleName = doc.DSRoleName,
                        Tabs = new List<BulkSendingCopyTab> { 
                            new BulkSendingCopyTab
                            {
                                InitialValue = doc.SignerName,
                                TabLabel = "MEMBER_FN"
                            },
                            new BulkSendingCopyTab
                            {
                                InitialValue = "Shankar",
                                TabLabel = "MEMBER_LN"
                            },
                            new BulkSendingCopyTab
                            {
                                InitialValue = "1/1/1991",
                                TabLabel = "MEMBER_DOB"
                            },
                            new BulkSendingCopyTab
                            {
                                InitialValue = "111-11-1111",
                                TabLabel = "MEMBER_SSN"
                            },
                        }

                    };
                    copy.Recipients.Add(recipient);
                    bulkCopies.Add(copy);
                }

                /*******
                 * 
                    bulk send

                    add multiple document templates to envelope 

                    ==============================

                    7/16
                    need to rethink how to add tabs to multiple docs per recipient in bulk send
                    Test here first (create second doc with tabs, send values in, see if they set)

                    =================
                 * *************/


                BulkSendingList bulkSendingList = new BulkSendingList
                {
                    Name = "EMT_Bulk_Send_List_1",
                    BulkCopies = bulkCopies
                };

                // Get config
                string accountId = ConfigurationManager.AppSettings["DS_AccountId"];
                string apiSuffix = ConfigurationManager.AppSettings["DS_APISuffix"];
                string basePath = ConfigurationManager.AppSettings["DS_BasePath"];

                // Get access token
                string accessToken = GetToken();

                // Step 1. Construct your API headers
                DocuSignClient apiClient = new DocuSignClient(basePath + apiSuffix);
                apiClient.Configuration.AddDefaultHeader("Authorization", "Bearer " + accessToken);

                // Step 2. Submit a bulk list
                var bulkEnvelopesApi = new BulkEnvelopesApi(apiClient);
                var createBulkListResult = bulkEnvelopesApi.CreateBulkSendList(accountId, bulkSendingList);

                // Step 3. Create the envelope with the template 
                string templateId = GetTemplateId(apiClient, accountId, templateName);
                string templateId2 = GetTemplateId(apiClient, accountId, "Supporting Document");

                // Create a composite template for the Server Template
                CompositeTemplate compTemplate = new CompositeTemplate
                {
                    CompositeTemplateId = "1",
                    ServerTemplates = new List<ServerTemplate> { 
                        new ServerTemplate { 
                            Sequence = "1",
                            TemplateId = templateId    // TemplateId for the doc in DocuSign
                        }
                    }
                };

                CompositeTemplate compTemplate2 = new CompositeTemplate
                {
                    CompositeTemplateId = "2",
                    ServerTemplates = new List<ServerTemplate> {
                        new ServerTemplate {
                            Sequence = "1",
                            TemplateId = templateId2    // TemplateId for the doc in DocuSign
                        }
                    }
                };

                // Bring the objects together in the EnvelopeDefinition
                EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
                {
                    EmailSubject = "This document was sent as a batch",
                    Status = "created",
                    CompositeTemplates = new List<CompositeTemplate> { compTemplate, compTemplate2 },
                };

                EnvelopesApi envelopesApi = new EnvelopesApi(apiClient);
                var envelopeResults = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

                // Step 4. Attach your bulk list ID to the envelope
                // Add an envelope custom field set to the value of your listId (EnvelopeCustomFields::create)
                // This Custom Field is used for tracking your Bulk Send via the Envelopes::Get method

                var fields = new CustomFields
                {
                    ListCustomFields = new List<ListCustomField> { },

                    TextCustomFields = new List<TextCustomField>
                    {
                        new TextCustomField
                        {
                            Name = "mailingListId",
                            Required = "false",
                            Show = "false",
                            Value = createBulkListResult.ListId //Adding the BULK_LIST_ID as an Envelope Custom Field
                        }
                    }
                };
                envelopesApi.CreateCustomFields(accountId, envelopeResults.EnvelopeId, fields);

                // Step 5. Add placeholder recipients. 
                // These will be replaced by the details provided in the Bulk List uploaded during Step 2
                // Note: The name / email format used is:
                // Name: Multi Bulk Recipients::{rolename}
                // Email: MultiBulkRecipients-{rolename}@docusign.com

                var recipients = new Recipients
                {
                    Signers = new List<Signer>
                    {
                        new Signer
                        {
                            Name = "Multi Bulk Recipient::signer",
                            Email = "multiBulkRecipients-signer@docusign.com",
                            RoleName = "signer",
                            RoutingOrder = "1",
                            Status = "sent",
                            DeliveryMethod = "Email",
                            RecipientId = "1",
                            RecipientType = "signer"
                        }
                    }
                };
                envelopesApi.CreateRecipient(accountId, envelopeResults.EnvelopeId, recipients);

                // Step 6. Initiate bulk send
                BulkSendResponse bulkRequestResult = bulkEnvelopesApi.CreateBulkSendRequest(accountId, createBulkListResult.ListId, new BulkSendRequest { EnvelopeOrTemplateId = envelopeResults.EnvelopeId });

                // TODO: instead of waiting 5 seconds, consider using the Asynchrnous method

                Console.WriteLine("Waiting for batch \'{0}\' ({1}) to finish...", bulkRequestResult.BatchName, bulkRequestResult.BatchId);
                System.Threading.Thread.Sleep(5000);

                // Step 7. Confirm successful batch send 
                Console.WriteLine("Sent = {0}", bulkEnvelopesApi.GetBulkSendBatchStatus(accountId, bulkRequestResult.BatchId).Sent);
            }
            catch (ApiException ex)
            {
                Console.WriteLine("SendBulkTemplate Exception occurred: {0} - {1}", ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendBulkTemplate Exception occurred: {0} - {1}", ex.HResult, ex.Message);
            }

            return;
        }



        public static string SendTemplate(DocumentModel doc)
        {
            try
            {
                // Embedded Signing Ceremony
                // 1. Create envelope request obj
                // 2. Use the SDK to create and send the envelope
                // 3. Create Envelope Recipient View request obj
                // 4. Use the SDK to obtain a Recipient View URL
                // 5. Redirect the user's browser to the URL
                // 6. After signing, DocuSign will redirect to our URL specified in the returnUrl argument

                // 1. Create envelope request object
                // Get access token
                string accessToken = GetToken();

                // Read config values
                string basePath = ConfigurationManager.AppSettings["DS_BasePath"];
                string apiSuffix = ConfigurationManager.AppSettings["DS_APISuffix"];
                string accountId = ConfigurationManager.AppSettings["DS_AccountId"];

                DocuSignClient apiClient = new DocuSignClient(basePath + apiSuffix);
                apiClient.Configuration.AddDefaultHeader("Authorization", "Bearer " + accessToken);

                string templateId = GetTemplateId(apiClient, accountId, doc.DSTemplateName);
                CompositeTemplate compTemplate = CreateCompositeTemplate(doc, templateId, "1");

                templateId = GetTemplateId(apiClient, accountId, "Supporting Document");
                CompositeTemplate compTemplate2 = CreateCompositeTemplate(doc, templateId, "2");

                // Custom webhook notification
                EventNotification eventNotification = new EventNotification
                {
                    EnvelopeEvents = new List<EnvelopeEvent> {
                        new EnvelopeEvent { EnvelopeEventStatusCode = "Completed" },
                        new EnvelopeEvent { EnvelopeEventStatusCode = "Declined" },
                        new EnvelopeEvent { EnvelopeEventStatusCode = "Delivered" },
                        new EnvelopeEvent { EnvelopeEventStatusCode = "Voided" }
                    },
                    EventData = new ConnectEventData { Format = "json", Version = "restv2.1" },
                    IncludeCertificateOfCompletion = "false",
                    IncludeCertificateWithSoap = "false",
                    IncludeDocumentFields = "true",
                    IncludeDocuments = "true",
                    IncludeEnvelopeVoidReason = "true",
                    IncludeHMAC = "true",
                    IncludeSenderAccountAsCustomField = "true",
                    IncludeTimeZone = "true",
                    LoggingEnabled = "true",
                    //RecipientEvents = new List<RecipientEvent> {
                    //    new RecipientEvent { RecipientEventStatusCode = "Completed" },
                    //    new RecipientEvent { RecipientEventStatusCode = "Declined" },
                    //    new RecipientEvent { RecipientEventStatusCode = "Delivered" },
                    //    new RecipientEvent { RecipientEventStatusCode = "AuthenticationFailed" },
                    //    new RecipientEvent { RecipientEventStatusCode = "AutoResponded" }
                    //},
                    RequireAcknowledgment = "true",
                    SignMessageWithX509Cert = "true",
                    SoapNameSpace = null,
                    Url = "https://webhook.site/1334f51b-fc69-4ffc-81da-8f2a6eab16d2",
                    UseSoapInterface = "false"
                };

                // Bring the objects together in the EnvelopeDefinition
                EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
                {
                    EmailSubject = "Please sign the document",
                    EmailBlurb = "This is a multi document envelope sent from DocuSign",
                    Status = "Sent",
                    CompositeTemplates = new List<CompositeTemplate> { compTemplate, compTemplate2 },
                    EventNotification = eventNotification,
                };

                // 2. Use the SDK to create and send the envelope
                EnvelopesApi envelopesApi = new EnvelopesApi(apiClient);
                EnvelopeSummary results = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

                // 3. Redirect to status
                return results.Status;
            }
            catch (ApiException ex)
            {
                Console.WriteLine("SendTemplate Exception occurred: {0} - {1}", ex.ErrorCode, ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("SendTemplate Exception occurred: {0} - {1}", ex.HResult, ex.Message);
            }
            return "failed";
        }

        internal static string GetToken()
        {
            // Read config values from file
            string accessToken = ConfigurationManager.AppSettings["AccessToken"];
            string clientId = ConfigurationManager.AppSettings["DS_ClientID"];
            string userGuid = ConfigurationManager.AppSettings["DS_UserGUID"];
            string authServer = ConfigurationManager.AppSettings["DS_AuthServer"];
            string rsaKeyFile = ConfigurationManager.AppSettings["DS_RSAKeyFile"];

            DateTime expiry;
            DateTime.TryParse(ConfigurationManager.AppSettings["AccessTokenExp"], out expiry);

            if (string.IsNullOrEmpty(accessToken) || expiry == null || expiry < DateTime.Now.AddSeconds(5))
            {
                DocuSignClient apiClient = new DocuSignClient();

                // Get new token
                OAuth.OAuthToken authToken = apiClient.RequestJWTUserToken(clientId,
                                userGuid,
                                authServer,
                                Encoding.UTF8.GetBytes(System.IO.File.ReadAllText(Path.Combine(
                                    System.Reflection.Assembly.GetExecutingAssembly().Location, @"..\..\..\Resources", rsaKeyFile))),
                                1);

                accessToken = authToken.access_token;

                // Validate
                apiClient.SetOAuthBasePath(ConfigurationManager.AppSettings["DS_AuthServer"]);
                OAuth.UserInfo userInfo = apiClient.GetUserInfo(authToken.access_token);
                Account acct = null;

                var accounts = userInfo.Accounts;
                acct = accounts.FirstOrDefault(a => a.IsDefault == "true");

                // Write JWT token, server path & expiry back to config 
                ConfigurationManager.AppSettings["AccessToken"] = accessToken;
                ConfigurationManager.AppSettings["DS_BasePath"] = acct.BaseUri;

                DateTime dt = DateTime.Now.AddSeconds(authToken.expires_in.Value);
                ConfigurationManager.AppSettings["AccessTokenExp"] = dt.ToLongDateString() + " " + dt.ToLongTimeString();
            }

            return accessToken;
        }
    }
}
