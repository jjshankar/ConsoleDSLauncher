﻿using System;
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
                if (!DocuSignConfig.Ready)
                    throw new Exception(Resources.DSCONFIG_NOT_SET);

                // Embedded Signing Ceremony with a template
                // 1. Create envelope request obj with template ID (get ID from DocuSign based on template name)
                // 2. Use the SDK to create and send the envelope with a Template document in DocuSign
                // 3. Create Envelope Recipient View request obj
                // 4. Use the SDK to obtain a Recipient View URL
                // 5. Return the Recipient View URL to the caller (takes user to DocuSign)
                //    - After signing, DocuSign will redirect to the URL specified in the AppReturnUrl argument

                // 1. Create envelope request object
                // Get access token
                string accessToken = Authenticate.GetToken();

                // Read config values
                string basePath = DocuSignConfig.BasePath;
                string apiSuffix = DocuSignConfig.APISuffix;
                string accountId = DocuSignConfig.AccountID;

                ApiClient apiClient = new ApiClient(basePath + apiSuffix);
                apiClient.Configuration.AddDefaultHeader("Authorization", "Bearer " + accessToken);

                // Get the template ID by calling the Templates API
                TemplatesApi templatesApi = new TemplatesApi(apiClient.Configuration);
                TemplatesApi.ListTemplatesOptions options = new TemplatesApi.ListTemplatesOptions { searchText = Doc.DSTemplateName };
                EnvelopeTemplateResults searchResults = templatesApi.ListTemplates(accountId, options);

                string templateId;
                string resultsTemplateName;

                // Process results
                if (int.Parse(searchResults.ResultSetSize) > 0)
                {
                    // Found the template! Record its id
                    templateId = searchResults.EnvelopeTemplates[0].TemplateId;
                    resultsTemplateName = searchResults.EnvelopeTemplates[0].Name;
                }
                else
                    throw new ApiException(404, "Template not found: " + Doc.DSTemplateName);

                // Start with the different components of the request
                // Create the signer recipient object 
                Signer signer = new Signer
                {
                    Email = Doc.SignerEmail,
                    Name = Doc.SignerName,
                    ClientUserId = Doc.SignerId,
                    RecipientId = "1",
                    RoleName = Doc.DSRoleName
                };

                // Set up prefilled fields (if passed in)
                if(Presets != null && Presets.Count > 0)
                {
                    List<Checkbox> checkBoxTabs = null;
                    List<Date> dateTabs = null;
                    List<Ssn> ssnTabs = null;
                    List<Text> textTabs = null;

                    foreach (DocPreset t in Presets)
                    {
                        switch(t.Type)
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

                // Bring the objects together in the EnvelopeDefinition
                EnvelopeDefinition envelopeDefinition = new EnvelopeDefinition
                {
                    EmailSubject = Doc.DSEmailSubject,
                    Status = "sent",
                    CompositeTemplates = new List<CompositeTemplate> { compTemplate }
                };

                // 2. Use the SDK to create and send the envelope
                EnvelopesApi envelopesApi = new EnvelopesApi(apiClient.Configuration);
                EnvelopeSummary results = envelopesApi.CreateEnvelope(accountId, envelopeDefinition);

                // 3. Create Envelope Recipient View request obj
                Doc.DSEnvelopeId = results.EnvelopeId;

                RecipientViewRequest viewRequest = new RecipientViewRequest
                {
                    ReturnUrl = AppReturnUrl + String.Format("?envelopeId={0}", results.EnvelopeId),
                    ClientUserId = Doc.SignerId,
                    AuthenticationMethod = "none",
                    UserName = Doc.SignerName,
                    Email = Doc.SignerEmail
                };

                // 4. Use the SDK to obtain a Recipient View URL
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
    }
}