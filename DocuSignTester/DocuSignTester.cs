using System;
using System.Collections.Generic;
using ECAR.DocuSign.Models;
using ECAR.DocuSign;
using System.Configuration;
using System.IO;
using System.Diagnostics;

namespace DocuSignTester
{
    class DocuSignTester
    {
        public static void Run(string mode = "DEV")
        {
            Console.WriteLine("DocuSign Testing...");

            string signerName;
            string signerEmail;
            string signerId;

            //if (args.Length >= 3)
            //{
            //    signerName = args[0];
            //    signerEmail = args[1];
            //    signerId = args[2];

            //    Console.WriteLine("Recipient full name input: {0}", signerName);
            //    Console.WriteLine("Recipient email: {0}", signerName);
            //    Console.WriteLine("Recipient ID: {0}", signerName);
            //}
            //else
            {
                Console.Write("Enter recipient full name (leave blank to exit): ");
                signerName = Console.ReadLine();
                if (string.IsNullOrEmpty(signerName))
                    return;

                Console.Write("Enter recipient email address: ");
                signerEmail = Console.ReadLine();

                Console.Write("Enter recipient ID (any value; leave blank for email workflow): ");
                signerId = Console.ReadLine();
            }

            try
            {
                // The document template and role should be pre-created in DocuSign
                const string DOCNAME = "NCAA HIPAA Consent Form";
                const string DSROLENAME = "Class Member";

                if (!SetConfig(mode))
                    throw new Exception("Configuration failed to set.");

                DocumentModel dsDoc = new DocumentModel
                {
                    DSEmailSubject = "Please dSign this document.",
                    DSRoleName = DSROLENAME,
                    DSTemplateName = DOCNAME,
                    SignerEmail = signerEmail,
                    SignerId = signerId,
                    SignerName = signerName
                };

                // Set up presets for member SSN and DOB (fields must exist in template doc)
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
                    Value = " ",
                    Locked = true
                };

                ECAR.DocuSign.Models.DocPreset dob = new ECAR.DocuSign.Models.DocPreset
                {
                    Label = "MEMBER_DOB",
                    Type = ECAR.DocuSign.Models.Presets.Date,
                    Value = "1/1/1990",
                    Locked = true
                };

                List<ECAR.DocuSign.Models.DocPreset> tabs = new List<ECAR.DocuSign.Models.DocPreset> { ssn, tin, dob };

                ReminderModel rem = new ReminderModel
                {
                    ReminderEnabled = true,
                    ReminderDelayDays = 1,
                    ReminderFrequencyDays = 1
                };

                string returnUrl = "https://www.epiqglobal.com";

                if (!string.IsNullOrEmpty(signerId))
                {
                    // Call library to initiate DocuSign; after signing, DocuSign will redirect to the page specified as returnUrl.
                    //  - envelopeId parameter will be passed back by ECAR.DocuSign to the CheckStatus action
                    //  - the returned dsDoc object will also have the envelope ID and document ID
                    string viewUrl = ECAR.DocuSign.TemplateSign.EmbeddedTemplateSign(returnUrl, ref dsDoc, rem, null, tabs);

                    // Redirect to DocuSign URL
                    ShowPage(viewUrl);

                    Console.WriteLine("DocuSign open in a new browser window.  Complete the signature ceremony.");
                    Console.WriteLine(string.Format("Envelope ID: {0}; status: {1}", dsDoc.DSEnvelopeId, Status.DSCheckStatus(dsDoc.DSEnvelopeId)));
                }
                else
                {
                    // Call library to initiate DocuSign via email
                    string status = ECAR.DocuSign.TemplateSign.EmailedTemplateSign(ref dsDoc, rem, null, tabs);
                    Console.WriteLine("DocuSign email workflow complete.  Status {0}.", status);
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }

        }

        public static void GetEnvelopes(string mode = "DEV")
        {
            try
            {
                DateTime? startDate = null;

                //if (args.Length == 1 && !string.IsNullOrEmpty(args[0]))
                //{
                //    startDate = DateTime.Parse(args[0]);
                //    Console.WriteLine("Using start date as: {0}", startDate.ToString());
                //}
                //else

                if (mode == "DEV")
                    Console.WriteLine("--- NOTE:  Development mode lists only top 10 envelopes. ---");
                else
                    Console.WriteLine("--- WARNING: Production mode restricts API calls to 1000 per hour. ---");

                Console.WriteLine();
                Console.Write("Enter start date to pull envelopes as mm/dd/yyyy (leave blank to exit): ");
                string readDate = Console.ReadLine();
                if (String.IsNullOrEmpty(readDate))
                    return;

                try
                {
                    startDate = DateTime.Parse(readDate);
                }
                catch
                {
                    startDate = new DateTime(2020, 1, 1);
                    Console.WriteLine("Invalid date.  Using start date as: {0}", startDate.ToString());
                }

                if (!SetConfig(mode))
                    throw new Exception("Configuration failed to set");

                List<ECAR.DocuSign.Models.EnvelopeModel> envModelList = ECAR.DocuSign.Status.DSGetAllEnvelopes((DateTime)startDate);

                int i = 0;
                foreach (EnvelopeModel envelope in envModelList)
                {
                    Console.WriteLine("=======  Envelope ID: {0}  =======", envelope.EnvelopeId);
                    Console.WriteLine("\tStatus: {0}", envelope.Status);
                    Console.WriteLine("\tCompletedDateTime: {0}", envelope.CompletedDateTime);
                    Console.WriteLine("\tCreatedDateTime: {0}", envelope.CreatedDateTime);
                    Console.WriteLine("\tDeclinedDateTime: {0}", envelope.DeclinedDateTime);
                    Console.WriteLine("\tEmailBlurb: {0}", envelope.EmailBlurb);
                    Console.WriteLine("\tEmailSubject: {0}", envelope.EmailSubject);
                    Console.WriteLine("\tExpireAfter: {0}", envelope.ExpireAfter);
                    Console.WriteLine("\tExpireDateTime: {0}", envelope.ExpireDateTime);
                    Console.WriteLine("\tExpireEnabled: {0}", envelope.ExpireEnabled);
                    Console.WriteLine("\tLastModifiedDateTime: {0}", envelope.LastModifiedDateTime);
                    Console.WriteLine("\tSentDateTime: {0}", envelope.SentDateTime);
                    Console.WriteLine("\tStatusChangedDateTime: {0}", envelope.StatusChangedDateTime);
                    Console.WriteLine("\tVoidedDateTime: {0}", envelope.VoidedDateTime);
                    Console.WriteLine("\tVoidedReason: {0}", envelope.VoidedReason);

                    foreach (EnvelopeRecipientModel recipient in ECAR.DocuSign.Status.DSGetAllRecipients(envelope.EnvelopeId))
                    {
                        Console.WriteLine("\t------- Recipient ID: {0} ------------", recipient.RecipientId);
                        Console.WriteLine("\t\tClientUserId: {0}", recipient.ClientUserId);
                        Console.WriteLine("\t\tDeclinedDateTime: {0}", recipient.DeclinedDateTime);
                        Console.WriteLine("\t\tDeclinedReason: {0}", recipient.DeclinedReason);
                        Console.WriteLine("\t\tDeliveredDateTime: {0}", recipient.DeliveredDateTime);
                        Console.WriteLine("\t\tEmail: {0}", recipient.Email);
                        Console.WriteLine("\t\tName: {0}", recipient.Name);
                        Console.WriteLine("\t\tRecipientType: {0}", recipient.RecipientType);
                        Console.WriteLine("\t\tRoleName: {0}", recipient.RoleName);
                        Console.WriteLine("\t\tSignatureName: {0}", recipient.SignatureName);
                        Console.WriteLine("\t\tSignedDateTime: {0}", recipient.SignedDateTime);
                        Console.WriteLine("\t\tStatus: {0}", recipient.Status);
                        Console.WriteLine("\t\tDSUserGUID: {0}", recipient.DSUserGUID);
                    }

                    foreach(EnvelopeDocumentModel doc in ECAR.DocuSign.Status.DSGetAllDocuments(envelope.EnvelopeId))
                    {
                        Console.WriteLine("\t------- Document ID: {0} ------------", doc.DocumentId);
                        Console.WriteLine("\t\tEnvelopeId: {0}", doc.EnvelopeId);
                        Console.WriteLine("\t\tOrder: {0}", doc.Order);
                        Console.WriteLine("\t\tName: {0}", doc.Name);
                        Console.WriteLine("\t\tType: {0}", doc.Type);
                        Console.WriteLine("\t\tDSDocumentGUID: {0}", doc.DSDocumentGUID);
                    }

                    Console.WriteLine("======= {0} ========", ++i);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(string.Format("Exception: {0}", ex.Message));
            }
        }

        internal static bool SetConfig(string mode = "DEV")
        {
            mode = string.IsNullOrEmpty(mode) ? "DEV" : ((mode == "PROD") ? mode : "DEV");

            if (!DocuSignConfig.Ready)
            {
                DocuSignConfig.AccountID = ConfigurationManager.AppSettings[mode + "_DS_AccountId"];
                DocuSignConfig.ClientID = ConfigurationManager.AppSettings[mode + "_DS_ClientID"];
                DocuSignConfig.UserGUID = ConfigurationManager.AppSettings[mode + "_DS_UserGUID"];
                DocuSignConfig.AuthServer = ConfigurationManager.AppSettings[mode + "_DS_AuthServer"];
                DocuSignConfig.RSAKey = ReadContent(ConfigurationManager.AppSettings[mode + "_DS_RSAKeyFile"]);
            }

            return DocuSignConfig.Ready;
        }

        internal static void ShowPage(string url)
        {
            // Process.Start(url);

            var psi = new ProcessStartInfo
            {
                FileName = "cmd",
                WindowStyle = ProcessWindowStyle.Hidden,
                UseShellExecute = false,
                CreateNoWindow = true,
                Arguments = $"/c start {url}"
            };
            Process.Start(psi);
        }

        internal static byte[] ReadContent(string fileName)
        {
            byte[] buff = null;

            string loc = System.Reflection.Assembly.GetExecutingAssembly().Location;
            loc = loc.Substring(0, loc.IndexOf("bin\\"));

            string path = Path.Combine(loc, "Resources", fileName);
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
