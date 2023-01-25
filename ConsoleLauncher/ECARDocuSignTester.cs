using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Diagnostics;

using ECAR.DocuSign;
using ECAR.DocuSign.Models;


namespace ConsoleLauncher
{
    class ECARDocuSignTester
    {
        const string BULKDOCNAME = "NCAA HIPAA Consent Form";
        const string DOCNAME = "Multi Doc Template";
        const string DOC2NAME = "Supporting Document";
        const string DSROLENAME = "Class Member";

        const string WEBHOOKURL = "https://webhook.site/8dfb7fc0-d11b-4fa6-bd61-5e9f67c49f60";

        public static void AdjustExpiration()
        {
            Console.Write("Enter expiration adjustment time in seconds: ");
            string sec = Console.ReadLine();
            if (string.IsNullOrEmpty(sec))
                return;

            try
            {
                int time = int.Parse(sec);
                ECAR.DocuSign.Status.AdjustExpiration(time);
                Console.WriteLine("Waiting {0} seconds...", time);
                System.Threading.Thread.Sleep(time * 1000);
            }
            catch(Exception ex)
            {
                Console.WriteLine("ERROR! " + ex.Message);
            }
        }

        public static void GetEnvelopeData()
        {
            Console.WriteLine("DocuSign Testing...GetEnvelopeData");
            Logger.Log("Entering GetEnvelopeData method.",
                System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name,
                LogLevel.Debug);

            Console.Write("Enter envelope ID (leave blank to exit): ");
            string envID = Console.ReadLine();
            if (string.IsNullOrEmpty(envID))
                return;
            try
            {
                if (!DocuSignConfig.Ready)
                {
                    DocuSignConfig.AccountID = ConfigurationManager.AppSettings["DS_AccountId"];
                    DocuSignConfig.ClientID = ConfigurationManager.AppSettings["DS_ClientID"];
                    DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID"];
                    DocuSignConfig.AuthServer = ConfigurationManager.AppSettings["DS_AuthServer"];
                    DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings["DS_RSAKeyFile"]);
                }

                EnvelopeModel env = ECAR.DocuSign.Status.DSGetEnvelope(envID);
                Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(env));
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, System.Reflection.MethodBase.GetCurrentMethod().Name, LogLevel.Error);
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }

        }

        public static void RunSingleTemplate(string[] args)
        {
            Console.WriteLine("DocuSign Testing...");
            Logger.Log("Entering single template method.", 
                System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name+"."+System.Reflection.MethodBase.GetCurrentMethod().Name, 
                LogLevel.Debug);

            Console.Write("Enter recipient full name (leave blank to exit): ");
            string signerName = Console.ReadLine();
            if (string.IsNullOrEmpty(signerName))
                return;

            Console.Write("Enter recipient email address: ");
            string signerEmail = Console.ReadLine();

            Console.Write("Enter recipient's ID/tracking number for embedded sign (leave blank for email): ");
            string signerId = Console.ReadLine();

            Console.Write("Stamp ID on envelope? (Y/N): ");
            bool stamp = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
            Console.WriteLine();

            Console.Write("Allow envelope reassignment? (Y/N): ");
            bool reassign = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
            Console.WriteLine();

            Console.Write("Enable Print & Sign? (Y/N): ");
            bool wetsign = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
            Console.WriteLine();

            Console.Write("Toggle send as user ('Official Sender'/'Settlement Services')? (Y/N): ");
            bool toggleSendAs = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
            Console.WriteLine();

            try
            {
                if (toggleSendAs || !DocuSignConfig.Ready)
                {
                    string key = "DS_UserGUID";
                    if(toggleSendAs)
                    {
                        if (DocuSignConfig.UserGUID != ConfigurationManager.AppSettings["DS_UserGUID_JS"])
                            key += "_JS";
                    }

                    DocuSignConfig.AccountID = ConfigurationManager.AppSettings["DS_AccountId"];
                    DocuSignConfig.ClientID = ConfigurationManager.AppSettings["DS_ClientID"];
                    DocuSignConfig.UserGUID = ConfigurationManager.AppSettings[key];
                    DocuSignConfig.AuthServer = ConfigurationManager.AppSettings["DS_AuthServer"];
                    DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings["DS_RSAKeyFile"]);
                }

                DocumentModel dsDoc = new DocumentModel
                {
                    DSEmailSubject = "Please dSign this document.",
                    DSEmailBody = "This is a custom email body for the email from DocuSign",
                    DSRoleName = DSROLENAME,
                    DSTemplateName = BULKDOCNAME,
                    SignerEmail = signerEmail,
                    SignerId = signerId,
                    SignerName = signerName,
                    DSStampEnvelopeId = stamp,
                    DSAllowReassign = reassign,
                    DSAllowPrintAndSign = wetsign
                };

                // US# 426250: Set up presets for member SSN and DOB
                ECAR.DocuSign.Models.DocPreset ssn = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "MEMBER_SSN",
                    Type = ECAR.DocuSign.Models.Presets.Ssn,
                    Value = "123-45-6789",
                    Locked = true
                };

                ECAR.DocuSign.Models.DocPreset tin = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "MEMBER_FIN",
                    Type = ECAR.DocuSign.Models.Presets.Text,
                    Value = "N/A",
                    Locked = true
                };

                ECAR.DocuSign.Models.DocPreset dob = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "MEMBER_DOB",
                    Type = ECAR.DocuSign.Models.Presets.Date,
                    Value = "1/2/1999",
                    Locked = true
                };

                ECAR.DocuSign.Models.DocPreset form1_fld1 = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "FORM1_FIELD1",
                    Type = ECAR.DocuSign.Models.Presets.Text,
                    Value = signerEmail,
                    Locked = true
                };

                List<ECAR.DocuSign.Models.DocPreset> tabs = new List<ECAR.DocuSign.Models.DocPreset> { ssn, tin, dob, form1_fld1 };

                if (!string.IsNullOrEmpty(signerId))
                {
                    Logger.Log("Calling EmbeddedTemplateSign.", System.Reflection.MethodBase.GetCurrentMethod().Name, LogLevel.Debug);
                    string returnUrl = "https://www.epiqglobal.com";

                    // Call library to initiate DocuSign; after signing, DocuSign will return to URL provided.
                    //  - envelopeId parameter will be passed back by ECAR.DocuSign to the CheckStatus action
                    //  - the returned dsDoc object will also have the envelope ID and document ID
                    string viewUrl = ECAR.DocuSign.TemplateSign.EmbeddedTemplateSign(returnUrl, ref dsDoc, tabs);

                    // Redirect to DocuSign URL
                    ShowPage(viewUrl);
                    Console.WriteLine("DocuSign ceremony is open in new browser window.");
                }
                else
                {
                    ECAR.DocuSign.Models.NotificationCallBackModel hook = new NotificationCallBackModel
                    {
                        WebHookUrl = WEBHOOKURL,
                        EnvelopeEvents = new List<string> {
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_COMPLETED,
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DECLINED,
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DELIVERED
                        }
                    };

                    Logger.Log("Calling EmailedTemplateSign.", System.Reflection.MethodBase.GetCurrentMethod().Name, LogLevel.Debug);
                    int i = 0;
                    //for (;i<20; i++)
                    //{
                        // string status = ECAR.DocuSign.TemplateSign.EmailedTemplateSign(ref dsDoc, tabs);    // no webhook
                        string status = ECAR.DocuSign.TemplateSign.EmailedTemplateSignWithCallBack(ref dsDoc, hook, null, null, tabs);

                        Console.WriteLine("Docusign {1} completed via email.  Status: {0}", status, i);
                    // }
                }

                Console.WriteLine(string.Format("Envelope ID: {0}; status: {1}", dsDoc.DSEnvelopeId, Status.DSCheckStatus(dsDoc.DSEnvelopeId)));
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, System.Reflection.MethodBase.GetCurrentMethod().Name, LogLevel.Error);
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }

        }

        public static void RunSingleTemplatePreview(string[] args)
        {
            Console.WriteLine("DocuSign Testing: SINGLETEMPLATEPREVIEW...");

            Console.Write("Enter recipient full name (leave blank to exit): ");
            string signerName = Console.ReadLine();
            if (string.IsNullOrEmpty(signerName))
                return;

            Console.Write("Enter recipient email address: ");
            string signerEmail = Console.ReadLine();

            Console.Write("Enter recipient's ID/tracking number: ");
            string signerId = Console.ReadLine();

            Console.Write("Stamp ID on envelope? (Y/N): ");
            bool stamp = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
            Console.WriteLine();

            Console.Write("Allow envelope reassignment? (Y/N): ");
            bool reassign = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
            Console.WriteLine();

            Console.Write("Enable Print & Sign? (Y/N): ");
            bool wetsign = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
            Console.WriteLine();

            try
            {
                if (!DocuSignConfig.Ready)
                {
                    DocuSignConfig.AccountID = ConfigurationManager.AppSettings["DS_AccountId"];
                    DocuSignConfig.ClientID = ConfigurationManager.AppSettings["DS_ClientID"];
                    DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID"];
                    DocuSignConfig.AuthServer = ConfigurationManager.AppSettings["DS_AuthServer"];
                    DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings["DS_RSAKeyFile"]);
                }

                DocumentModel dsDoc = new DocumentModel
                {
                    DSEmailSubject = "Please dSign this document.",
                    DSEmailBody = "This is a custom email body for the email from DocuSign",
                    DSRoleName = DSROLENAME,
                    DSTemplateName = BULKDOCNAME,
                    SignerEmail = signerEmail,
                    SignerId = signerId,
                    SignerName = signerName,
                    DSStampEnvelopeId = stamp,
                    DSAllowReassign = reassign,
                    DSAllowPrintAndSign = wetsign
                };

                // US# 426250: Set up presets for member SSN and DOB
                ECAR.DocuSign.Models.DocPreset ssn = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "MEMBER_SSN",
                    Type = ECAR.DocuSign.Models.Presets.Ssn,
                    Value = "123-45-6789",
                    Locked = true
                };

                ECAR.DocuSign.Models.DocPreset tin = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "MEMBER_FIN",
                    Type = ECAR.DocuSign.Models.Presets.Text,
                    Value = "N/A",
                    Locked = true
                };

                ECAR.DocuSign.Models.DocPreset dob = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "MEMBER_DOB",
                    Type = ECAR.DocuSign.Models.Presets.Date,
                    Value = "1/2/1999",
                    Locked = true
                };

                ECAR.DocuSign.Models.DocPreset form1_fld1 = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "FORM1_FIELD1",
                    Type = ECAR.DocuSign.Models.Presets.Text,
                    Value = signerEmail,
                    Locked = true
                };

                List<ECAR.DocuSign.Models.DocPreset> tabs = new List<ECAR.DocuSign.Models.DocPreset> { ssn, tin, dob, form1_fld1 };

                string returnUrl = "https://www.epiqglobal.com";

                ECAR.DocuSign.Models.NotificationCallBackModel hook = new NotificationCallBackModel
                {
                    WebHookUrl = WEBHOOKURL,
                    EnvelopeEvents = new List<string> {
                        ECAR.DocuSign.Models.EnvelopeStatus.STATUS_COMPLETED,
                        ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DECLINED,
                        ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DELIVERED
                    }
                };

                // Call library to initiate DocuSign; after signing, DocuSign will return to URL provided.
                //  - envelopeId parameter will be passed back by ECAR.DocuSign to the CheckStatus action
                //  - the returned dsDoc object will also have the envelope ID and document ID
                string viewUrl = ECAR.DocuSign.TemplateSign.CreatePreviewURL(ref dsDoc, returnUrl, true, hook, null, null, tabs);

                // Redirect to DocuSign URL
                ShowPage(viewUrl);
                Console.WriteLine("DocuSign preview is open in new browser window. Press any key to continue or 'Y' to send the envelope....");
                char key = Console.ReadKey().KeyChar;
                Console.WriteLine();

                if (key == 'Y' || key == 'y')
                {
                    bool bRes = ECAR.DocuSign.TemplateSign.SendPreviewedEnvelope(dsDoc);
                    Console.WriteLine("DocuSign envelope " + (bRes ? "sent!" : "ERROR!"));
                }

                Console.WriteLine(string.Format("Envelope ID: {0}; status: {1}", dsDoc.DSEnvelopeId, Status.DSCheckStatus(dsDoc.DSEnvelopeId)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }

        }

        public static void RunPacketSendTemplate(string[] args)
        {
            Console.WriteLine("DocuSign Testing - MULTI DOC PACKET SEND ...");

            Console.Write("Enter recipient full name (leave blank to exit): ");
            string signerName = Console.ReadLine();
            if (string.IsNullOrEmpty(signerName))
                return;

            Console.Write("Enter recipient email address: ");
            string signerEmail = Console.ReadLine();

            Console.Write("Enter recipient's ID/tracking number for embedded sign (leave blank for email): ");
            string signerId = Console.ReadLine();

            Console.Write("Stamp ID on envelope? (Y/N): ");
            bool stamp = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
            Console.WriteLine();

            Console.Write("Allow envelope reassignment? (Y/N): ");
            bool reassign = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
            Console.WriteLine();

            Console.Write("Enable Print & Sign? (Y/N): ");
            bool wetsign = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
            Console.WriteLine();

            try
            {
                if (!DocuSignConfig.Ready)
                {
                    DocuSignConfig.AccountID = ConfigurationManager.AppSettings["DS_AccountId"];
                    DocuSignConfig.ClientID = ConfigurationManager.AppSettings["DS_ClientID"];
                    DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID"];
                    DocuSignConfig.AuthServer = ConfigurationManager.AppSettings["DS_AuthServer"];
                    DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings["DS_RSAKeyFile"]);
                }

                DocumentPacketModel dsDoc = new DocumentPacketModel
                {
                    DSEmailSubject = "Please sign this document packet (2 docs).",
                    DSRoleName = DSROLENAME,
                    DSTemplateList = new List<string> { DOCNAME, DOC2NAME },
                    SignerEmail = signerEmail,
                    SignerId = signerId,
                    SignerName = signerName,
                    DSStampEnvelopeId = stamp,
                    DSAllowReassign = reassign,
                    DSAllowPrintAndSign = wetsign
                };

                // Set up presets for member SSN and DOB - field names are same in both templates
                ECAR.DocuSign.Models.DocPreset ssn = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "MEMBER_SSN",
                    Type = ECAR.DocuSign.Models.Presets.Ssn,
                    Value = "123-45-6789",
                    Locked = true
                };

                ECAR.DocuSign.Models.DocPreset dob = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "MEMBER_DOB",
                    Type = ECAR.DocuSign.Models.Presets.Date,
                    Value = "1/1/1990",
                    Locked = true
                };

                List<ECAR.DocuSign.Models.DocPreset> tabs = new List<ECAR.DocuSign.Models.DocPreset> { ssn, dob };

                if (!string.IsNullOrEmpty(signerId))
                {
                    string returnUrl = "https://www.epiqglobal.com";

                    // Call library to initiate DocuSign; after signing, DocuSign will return to URL provided.
                    //  - envelopeId parameter will be passed back by ECAR.DocuSign to the CheckStatus action
                    //  - the returned dsDoc object will also have the envelope ID and document ID
                    string viewUrl = ECAR.DocuSign.TemplateSign.EmbeddedPacketSign(returnUrl, ref dsDoc, null, null, tabs);

                    // Redirect to DocuSign URL
                    ShowPage(viewUrl);
                    Console.WriteLine("DocuSign ceremony is open in new browser window.");
                }
                else
                {
                    ECAR.DocuSign.Models.NotificationCallBackModel hook = new NotificationCallBackModel
                    {
                        WebHookUrl = WEBHOOKURL,
                        EnvelopeEvents = new List<string> {
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_COMPLETED,
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DECLINED,
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DELIVERED
                        }
                    };
                    // string status = ECAR.DocuSign.TemplateSign.EmailedTemplateSign(ref dsDoc, tabs);

                    string status = ECAR.DocuSign.TemplateSign.EmailedPacketSign(
                        ref dsDoc, hook, null, null, tabs);

                    Console.WriteLine("Docusign completed via email.  Status: {0}", status);
                }

                Console.WriteLine(string.Format("Envelope ID: {0}; status: {1}", dsDoc.DSEnvelopeId, Status.DSCheckStatus(dsDoc.DSEnvelopeId)));
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }

        }

        public static string RunBulkSingleTemplate(string[] args)
        {
            Console.WriteLine("DocuSign Testing... BULK SEND SINGLE TEMPLATE");
            Console.Write("Enter number of recipients...");
            int numRec = int.Parse(Console.ReadLine());

            if (numRec <= 0)
                return null;

            try
            {
                BulkSendDocumentList dsBulkList = new BulkSendDocumentList
                {
                    BulkRecipientList = new List<BulkSendRecipientModel>(),
                    BulkBatchName = "CONSOLETESTER_BULKBATCH",
                    DSBatchTemplateName = BULKDOCNAME,
                    BulkEmailSubject = "Batch send DocuSign",
                    BulkEmailBody = "Batch send email body",
                };

                while (numRec > 0)
                {
                    Console.WriteLine("------ RECIPIENT {0} ", numRec--);
                    Console.Write("Enter recipient full name (leave blank to exit): ");
                    string signerName = Console.ReadLine();
                    if (string.IsNullOrEmpty(signerName))
                        return null;

                    BulkSendRecipientModel bulkRec = new BulkSendRecipientModel
                    {
                        SignerName = signerName
                    };

                    Console.Write("Enter recipient email address: ");
                    bulkRec.SignerEmail = Console.ReadLine();

                    Console.Write("Enter recipient's ID/tracking number: ");
                    bulkRec.SignerId = Console.ReadLine();

                    bulkRec.DSRoleName = DSROLENAME;

                    // Set up presets for member SSN and DOB
                    ECAR.DocuSign.Models.DocPreset ssn = new ECAR.DocuSign.Models.DocPreset
                    {
                        Label = "MEMBER_SSN",
                        Type = ECAR.DocuSign.Models.Presets.Ssn,
                        Locked = true
                    };
                    Console.Write("Enter recipient SSN (###-##-####): ");
                    ssn.Value = Console.ReadLine();

                    ECAR.DocuSign.Models.DocPreset consent = new ECAR.DocuSign.Models.DocPreset
                    {
                        Label = "MEMBER_CONSENT_YES",
                        Type = ECAR.DocuSign.Models.Presets.Checkbox,
                        Value = "true",
                        Locked = true
                    };

                    ECAR.DocuSign.Models.DocPreset dob = new ECAR.DocuSign.Models.DocPreset
                    {
                        Label = "MEMBER_DOB",
                        Type = ECAR.DocuSign.Models.Presets.Date,
                        Locked = true
                    };
                    Console.Write("Enter recipient DOB (##/##/####): ");
                    dob.Value = Console.ReadLine();

                    bulkRec.Presets = new List<ECAR.DocuSign.Models.DocPreset>();
                    if (!string.IsNullOrEmpty(ssn.Value))
                        bulkRec.Presets.Add(ssn);
                    if (!string.IsNullOrEmpty(consent.Value))
                        bulkRec.Presets.Add(consent);
                    if (!string.IsNullOrEmpty(dob.Value))
                        bulkRec.Presets.Add(dob);


                    Console.Write("Enter custom email subject for this recipient: ");
                    bulkRec.CustomEmailSubject = Console.ReadLine();

                    Console.Write("Enter custom email text for this recipient: ");
                    bulkRec.CustomEmailBody = Console.ReadLine();

                    dsBulkList.BulkRecipientList.Add(bulkRec);
                }

                Console.Write("Stamp ID on envelope? (Y/N): ");
                dsBulkList.DSStampEnvelopeId = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
                Console.WriteLine();

                Console.Write("Allow envelope reassignment? (Y/N): ");
                dsBulkList.DSAllowReassign = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
                Console.WriteLine();

                Console.Write("Enable Print & Sign? (Y/N): ");
                dsBulkList.DSAllowPrintAndSign = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
                Console.WriteLine();

                if (!DocuSignConfig.Ready)
                {
                    DocuSignConfig.AccountID = ConfigurationManager.AppSettings["DS_AccountId"];
                    DocuSignConfig.ClientID = ConfigurationManager.AppSettings["DS_ClientID"];
                    DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID"];
                    DocuSignConfig.AuthServer = ConfigurationManager.AppSettings["DS_AuthServer"];
                    DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings["DS_RSAKeyFile"]);
                }


                ECAR.DocuSign.Models.NotificationCallBackModel hook = new NotificationCallBackModel
                {
                    WebHookUrl = WEBHOOKURL,
                    EnvelopeEvents = new List<string> {
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_COMPLETED,
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DECLINED,
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DELIVERED
                        }
                };


                // Call library to initiate DocuSign; 
                string batchId = ECAR.DocuSign.TemplateSign.BulkSendTemplate(ref dsBulkList, hook);

                Console.WriteLine(string.Format("List ID: {0}; Batch ID: {1}", dsBulkList.DSListId, batchId));
                return batchId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }
            return null;
        }

        public static string QuickRunBulkSingleTemplate(string[] args)
        {
            Console.WriteLine("DocuSign Testing... BULK SEND SINGLE TEMPLATE");
            Console.Write("Enter number of recipients...: 1");
            int numRec = 1;

            if (numRec <= 0)
                return null;

            try
            {
                BulkSendDocumentList dsBulkList = new BulkSendDocumentList
                {
                    BulkRecipientList = new List<BulkSendRecipientModel>(),
                    BulkBatchName = "CONSOLETESTER_BULKBATCH",
                    DSBatchTemplateName = BULKDOCNAME,
                    BulkEmailSubject = "Batch send DocuSign",
                    BulkEmailBody = "Batch send email body",                    
                };

                while (numRec > 0)
                {
                    Console.WriteLine("------ RECIPIENT {0} ", numRec--);
                    string signerName = "jj ss";
                    Console.WriteLine("Enter recipient full name (leave blank to exit): " + signerName);
                    if (string.IsNullOrEmpty(signerName))
                        return null; 

                    BulkSendRecipientModel bulkRec = new BulkSendRecipientModel
                    {
                        SignerName = signerName
                    };

                    Console.Write("Enter recipient email address: ");
                    bulkRec.SignerEmail = "jshankar@epiqglobal.com";

                    Console.Write("Enter recipient's ID/tracking number: ");
                    bulkRec.SignerId = "123123";
                    Console.WriteLine(bulkRec.SignerId);

                    bulkRec.DSRoleName = DSROLENAME;

                    // Set up presets for member SSN and DOB
                    ECAR.DocuSign.Models.DocPreset ssn = new ECAR.DocuSign.Models.DocPreset
                    {
                        Label = "MEMBER_SSN",
                        Type = ECAR.DocuSign.Models.Presets.Ssn,
                        Locked = true
                    };
                    Console.Write("Enter recipient SSN (###-##-####): ");
                    ssn.Value = "111-11-1111";
                    Console.WriteLine(ssn.Value);

                    ECAR.DocuSign.Models.DocPreset consent = new ECAR.DocuSign.Models.DocPreset
                    {
                        Label = "MEMBER_CONSENT_YES",
                        Type = ECAR.DocuSign.Models.Presets.Checkbox,
                        Value = "true",
                        Locked = false
                    };

                    ECAR.DocuSign.Models.DocPreset dob = new ECAR.DocuSign.Models.DocPreset
                    {
                        Label = "MEMBER_DOB",
                        Type = ECAR.DocuSign.Models.Presets.Date,
                        Locked = true
                    };
                    Console.Write("Enter recipient DOB (##/##/####): ");
                    dob.Value = "11/11/1977";
                    Console.WriteLine(dob.Value);

                    bulkRec.Presets = new List<ECAR.DocuSign.Models.DocPreset>();
                    if (!string.IsNullOrEmpty(ssn.Value))
                        bulkRec.Presets.Add(ssn);
                    if (!string.IsNullOrEmpty(consent.Value))
                        bulkRec.Presets.Add(consent);
                    if (!string.IsNullOrEmpty(dob.Value))
                        bulkRec.Presets.Add(dob);


                    Console.Write("Enter custom email subject for this recipient: ");
                    bulkRec.CustomEmailSubject = Console.ReadLine();

                    Console.Write("Enter custom email text for this recipient: ");
                    bulkRec.CustomEmailBody = Console.ReadLine();

                    dsBulkList.BulkRecipientList.Add(bulkRec);
                }

                Console.Write("Stamp ID on envelope? (Y/N): N");
                dsBulkList.DSStampEnvelopeId = false;
                Console.WriteLine();

                Console.Write("Allow envelope reassignment? (Y/N): N");
                dsBulkList.DSAllowReassign = false;
                Console.WriteLine();

                Console.Write("Enable Print & Sign? (Y/N): N");
                dsBulkList.DSAllowPrintAndSign = false;
                Console.WriteLine();

                if (!DocuSignConfig.Ready)
                {
                    DocuSignConfig.AccountID = ConfigurationManager.AppSettings["DS_AccountId"];
                    DocuSignConfig.ClientID = ConfigurationManager.AppSettings["DS_ClientID"];
                    DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID"];
                    DocuSignConfig.AuthServer = ConfigurationManager.AppSettings["DS_AuthServer"];
                    DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings["DS_RSAKeyFile"]);
                }


                ECAR.DocuSign.Models.NotificationCallBackModel hook = new NotificationCallBackModel
                {
                    WebHookUrl = WEBHOOKURL,
                    EnvelopeEvents = new List<string> {
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_COMPLETED,
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DECLINED,
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DELIVERED
                        }
                };


                // Call library to initiate DocuSign; 
                string batchId = ECAR.DocuSign.TemplateSign.BulkSendTemplate(ref dsBulkList, hook);

                Console.WriteLine(string.Format("List ID: {0}; Batch ID: {1}", dsBulkList.DSListId, batchId));
                return batchId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }
            return null;
        }

        public static string RunBulkPacketTemplate(string[] args)
        {
            Console.WriteLine("DocuSign Testing... BULK SEND MULTIPLE TEMPLATE");
            Console.Write("Enter number of recipients...");
            int numRec = int.Parse(Console.ReadLine());

            if (numRec <= 0)
                return null;

            try
            {
                BulkSendPacketList  dsBulkList = new BulkSendPacketList
                {
                    BulkPacketRecipientList = new List<BulkSendPacketRecipientModel>(),
                    BulkBatchName = "CONSOLETESTER_BULKBATCH",
                    DSBatchPacketTemplates = new List<string> { BULKDOCNAME, DOC2NAME },
                    BulkEmailSubject = "Batch send DocuSign",
                    BulkEmailBody = "Batch send email body",                    
                };

                while (numRec > 0)
                {
                    Console.WriteLine("------ RECIPIENT {0} ", numRec--);
                    Console.Write("Enter recipient full name (leave blank to exit): ");
                    string signerName = Console.ReadLine();
                    if (string.IsNullOrEmpty(signerName))
                        return null;

                    BulkSendPacketRecipientModel bulkRec = new BulkSendPacketRecipientModel
                    {
                        SignerName = signerName
                    };

                    Console.Write("Enter recipient email address: ");
                    bulkRec.SignerEmail = Console.ReadLine();

                    Console.Write("Enter recipient's ID/tracking number: ");
                    bulkRec.SignerId = Console.ReadLine();

                    bulkRec.DSRoleName = DSROLENAME;

                    // Set up presets for member SSN and DOB
                    ECAR.DocuSign.Models.DocPreset ssn = new ECAR.DocuSign.Models.DocPreset
                    {
                        Label = "MEMBER_SSN",
                        Type = ECAR.DocuSign.Models.Presets.Ssn,
                        Locked = true
                    };
                    Console.Write("Enter recipient SSN (###-##-####): ");
                    ssn.Value = Console.ReadLine();

                    ECAR.DocuSign.Models.DocPreset consent = new ECAR.DocuSign.Models.DocPreset
                    {
                        Label = "MEMBER_CONSENT_YES",
                        Type = ECAR.DocuSign.Models.Presets.Checkbox,
                        Value = "true",
                        Locked = true
                    };

                    ECAR.DocuSign.Models.DocPreset dob = new ECAR.DocuSign.Models.DocPreset
                    {
                        Label = "MEMBER_DOB",
                        Type = ECAR.DocuSign.Models.Presets.Date,
                        Locked = true
                    };

                    Console.Write("Enter recipient DOB (##/##/####): ");
                    dob.Value = Console.ReadLine();
                    bulkRec.Presets = new List<ECAR.DocuSign.Models.DocPreset>();
                    if (!string.IsNullOrEmpty(ssn.Value))
                        bulkRec.Presets.Add(ssn);
                    if (!string.IsNullOrEmpty(consent.Value))
                        bulkRec.Presets.Add(consent);
                    if (!string.IsNullOrEmpty(dob.Value))
                        bulkRec.Presets.Add(dob);

                    Console.Write("Enter custom email subject for this recipient: ");
                    bulkRec.CustomEmailSubject = Console.ReadLine();

                    Console.Write("Enter custom email text for this recipient: ");
                    bulkRec.CustomEmailBody = Console.ReadLine();

                    dsBulkList.BulkPacketRecipientList.Add(bulkRec);
                }

                Console.Write("Stamp ID on envelope? (Y/N): ");
                dsBulkList.DSStampEnvelopeId = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
                Console.WriteLine();

                Console.Write("Allow envelope reassignment? (Y/N): ");
                dsBulkList.DSAllowReassign = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
                Console.WriteLine();

                Console.Write("Enable Print & Sign? (Y/N): ");
                dsBulkList.DSAllowPrintAndSign = (Console.ReadKey().KeyChar.ToString().ToUpper() == "Y");
                Console.WriteLine();

                if (!DocuSignConfig.Ready)
                {
                    DocuSignConfig.AccountID = ConfigurationManager.AppSettings["DS_AccountId"];
                    DocuSignConfig.ClientID = ConfigurationManager.AppSettings["DS_ClientID"];
                    DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID"];
                    DocuSignConfig.AuthServer = ConfigurationManager.AppSettings["DS_AuthServer"];
                    DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings["DS_RSAKeyFile"]);
                }


                ECAR.DocuSign.Models.NotificationCallBackModel hook = new NotificationCallBackModel
                {
                    WebHookUrl = WEBHOOKURL,
                    EnvelopeEvents = new List<string> {
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_COMPLETED,
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DECLINED,
                            ECAR.DocuSign.Models.EnvelopeStatus.STATUS_DELIVERED
                        }
                };


                // Call library to initiate DocuSign; after signing, DocuSign will return to URL provided.
                //  - envelopeId parameter will be passed back by ECAR.DocuSign to the CheckStatus action
                //  - the returned dsDoc object will also have the envelope ID and document ID
                string batchId = ECAR.DocuSign.TemplateSign.BulkSendPacket(ref dsBulkList, hook);

                Console.WriteLine(string.Format("List ID: {0}; BatchID: {1}", dsBulkList.DSListId, batchId));
                return batchId;
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }
            return null;
        }

        public static void GetBatchEnvelopes()
        {
            Console.WriteLine("DocuSign Testing...GetBatchEnvelopes");
            Logger.Log("Entering GetBatchEnvelopes method.",
                System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name,
                LogLevel.Debug);

            Console.Write("Enter batch ID (leave blank to exit): ");
            string batchId = Console.ReadLine();
            if (string.IsNullOrEmpty(batchId))
                return;
            try
            {
                if (!DocuSignConfig.Ready)
                {
                    DocuSignConfig.AccountID = ConfigurationManager.AppSettings["DS_AccountId"];
                    DocuSignConfig.ClientID = ConfigurationManager.AppSettings["DS_ClientID"];
                    DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID"];
                    DocuSignConfig.AuthServer = ConfigurationManager.AppSettings["DS_AuthServer"];
                    DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings["DS_RSAKeyFile"]);
                }

                List<EnvelopeModel> envList = ECAR.DocuSign.Status.DSGetBulkBatchEnvelopes(batchId);
                foreach(EnvelopeModel env in envList)
                    Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(env));
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, System.Reflection.MethodBase.GetCurrentMethod().Name, LogLevel.Error);
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }
        }

        public static void GetFieldData()
        {
            Console.WriteLine("DocuSign Testing...GetFieldData");
            Logger.Log("Entering GetFieldData method.",
                System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name + "." + System.Reflection.MethodBase.GetCurrentMethod().Name,
                LogLevel.Debug);

            Console.Write("Enter envelope ID (leave blank to exit): ");
            string envId = Console.ReadLine();
            if (string.IsNullOrEmpty(envId))
                return;
            try
            {
                Console.Write("Enter field name: ");
                string fieldName = Console.ReadLine();
                
                if (!DocuSignConfig.Ready)
                {
                    DocuSignConfig.AccountID = ConfigurationManager.AppSettings["DS_AccountId"];
                    DocuSignConfig.ClientID = ConfigurationManager.AppSettings["DS_ClientID"];
                    DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID"];
                    DocuSignConfig.AuthServer = ConfigurationManager.AppSettings["DS_AuthServer"];
                    DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings["DS_RSAKeyFile"]);
                }

                Console.WriteLine("Field: {0} \t Value: {1}.", fieldName, 
                    ECAR.DocuSign.Status.DSGetDocumentCheckBoxField(fieldName, envId));
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message, System.Reflection.MethodBase.GetCurrentMethod().Name, LogLevel.Error);
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }
        }
        public static void GetBatchStatus(string batchId)
        {
            Console.WriteLine("ECAR DocuSign Testing... BATCH STATUS");

            try
            {
                if (!DocuSignConfig.Ready)
                {
                    DocuSignConfig.AccountID = ConfigurationManager.AppSettings["DS_AccountId"];
                    DocuSignConfig.ClientID = ConfigurationManager.AppSettings["DS_ClientID"];
                    DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID"];
                    DocuSignConfig.AuthServer = ConfigurationManager.AppSettings["DS_AuthServer"];
                    DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings["DS_RSAKeyFile"]);
                }

                // Get batch status
                List<ECAR.DocuSign.Models.EnvelopeModel> envs = null;

                envs = ECAR.DocuSign.Status.DSGetBulkBatchEnvelopes(batchId);
                if(envs == null)
                {
                    Console.WriteLine("-------- CHECKED TOO SOON.  NO ENVELOPES RETURNED. -----------");
                    return;
                }

                int i = 0;
                foreach(ECAR.DocuSign.Models.EnvelopeModel env in envs)
                {
                    Console.WriteLine("-------- Envelope #{0} -----------", i++);
                    Console.WriteLine("\tEnvelope ID: {0}", env.EnvelopeId);
                    Console.WriteLine("\tEnvelope Sent: {0}", env.SentDateTime);
                    Console.WriteLine("\tEnvelope Delivered: {0}", env.DeliveredDateTime);
                    Console.WriteLine("\tEnvelope Status: {0}", env.Status);
                    Console.WriteLine("\tEnvelope Status Changed on: {0}", env.StatusChangedDateTime);
                    Console.WriteLine("\tEnvelope Delivered: {0}", env.DeliveredDateTime);

                    List<EnvelopeDocumentModel> Docs = ECAR.DocuSign.Status.DSGetAllDocuments(env.EnvelopeId);
                    if (Docs.Count > 0)
                    {
                        Console.WriteLine("\t-------- Documents in this envelope  -----------", Docs.Count);
                        foreach (EnvelopeDocumentModel doc in Docs)
                        {
                            Console.WriteLine("\t\t Document ID: {0}", doc.DSDocumentGUID);
                            Console.WriteLine("\t\t Document Number: {0}", doc.DocumentId);
                            Console.WriteLine("\t\t Document Name: {0}", doc.Name);
                            Console.WriteLine("\t\t Document Order: {0}", doc.Order);
                            Console.WriteLine("\t\t Document Type: {0}", doc.Type);
                            Console.WriteLine("\t\t-------- ");
                        }
                    }

                    Dictionary<string, string> customFields = ECAR.DocuSign.Status.DSGetEnvelopeCustomFields(env.EnvelopeId);
                    if (customFields.Count > 0)
                    {
                        Console.WriteLine("\t-------- Custom fields in this envelope  -----------", customFields.Count);
                        foreach (KeyValuePair<string, string> item in customFields)
                        {
                            Console.WriteLine("\t\t Custom Field ID: {0}, Value: {1}", item.Key, item.Value);
                        }
                        Console.WriteLine("\t-------- ");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }

        }

        internal static void ShowPage(string url)
        {
            Process.Start(url);

            //using (Process browser = new Process())
            //{
            //    browser.StartInfo.FileName = "";
            //    browser.StartInfo.Arguments = url;
            //    browser.StartInfo.UseShellExecute = false;
            //    browser.StartInfo.RedirectStandardOutput = true;
            //    browser.Start();

            //    Console.WriteLine(browser.StandardOutput.ReadToEnd());

            //    browser.WaitForExit();
            //};
        }

        internal static byte[] ReadContent(string fileName)
        {
            byte[] buff = null;
            string path = Path.Combine(
                                    System.Reflection.Assembly.GetExecutingAssembly().Location, @"..\..\..\Resources", fileName);
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (BinaryReader br = new BinaryReader(stream))
                {
                    long numBytes = new FileInfo(path).Length;
                    buff = br.ReadBytes((int)numBytes);
                }
            }
            return buff;
        }
    }
}
