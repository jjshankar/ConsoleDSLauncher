# ECAR.DocuSign
Sample code that uses the **ECAR.DocuSign** library within a controller.


```csharp
namespace MyWebApp.Controllers
{
    public class ECARDocuSignController : Controller
    {
        // The following information is from DocuSign developer portal
        private const string DOCTEMPLATENAME = "NCAA HIPAA Consent Form";
        private const string DSROLENAME = "Class Member";

        // Data for this method: DocModel object (from View)
        public ActionResult EmbeddedTemplateSign(DocModel doc)
        {
            if (!ModelState.IsValid)
                return RedirectToAction("ECARDocuSign", "Home");

            try
            {
                // Configure ECAR.DocuSign 
                SetDSConfig();

                DocumentModel dsDoc = new DocumentModel
                {
                    DSEmailSubject = "Please dSign this document.",
                    DSRoleName = DSROLENAME,
                    DSTemplateName = DOCTEMPLATENAME,
                    SignerEmail = doc.SignerEmail,
                    SignerId = doc.SignerId,
                    SignerName = doc.SignerName
                };

                DocPreset ssn = new DocPreset { Label = "MEMBER_SSN", Type= Presets.Ssn, Value= "123-45-6789", Locked = true };
                DocPreset dob = new DocPreset { Label = "MEMBER_DOB", Type = Presets.Date, Value = "1/1/1991"};
                DocPreset checkY = new DocPreset { Label = "MEMBER_CONSENT_YES", Type = Presets.Checkbox, Value = "true"};
                DocPreset text = new DocPreset { Label = "fake field", Type = Presets.Text, Value = "Field requested does not exist in doc; fails silently." };

                List<DocPreset> tabs = new List<DocPreset> { ssn, dob, check, checkY, text };

                // Construct return URL to an action (argument envelopeId will be automatically passed in by ECAR.DocuSign)
                string returnUrl = MyAppConfig.GetConfiguration("AppHomeUrl") + "/ECARDocuSign/CheckStatus";

                // Call library to initiate DocuSign; after signing, DocuSign will return to CheckStatus action.
                //  - envelopeId parameter will be passed back by ECAR.DocuSign to the CheckStatus action
                //  - the returned dsDoc object will also have the envelope ID and document ID
                string viewUrl = ECAR.DocuSign.TemplateSign.EmbeddedTemplateSign(returnUrl, ref dsDoc, tabs);

                // Redirect to DocuSign URL
                return Redirect(viewUrl);
            }
            catch (Exception ex)
            {
                ErrorModel err = new ErrorModel
                {
                    Code = ex.HResult.ToString(),
                    Message = ex.Message,
                    Source = ex.Source,
                    ErrorContent = ex.StackTrace
                };

                return View("Error", err);
            }
        }

        // Data for this method: Envelope ID
        public ActionResult CheckStatus(string envelopeId)
        {
            try
            {
                // Configure ECAR.DocuSign 
                SetDSConfig();

                // Get document status
                string status = ECAR.DocuSign.Status.DSCheckStatus(envelopeId);
                ViewBag.Result = status;

                // Get document data
                DocumentModel doc = ECAR.DocuSign.Status.DSGetDocInfo(envelopeId);                
                ViewBag.Consent = ECAR.DocuSign.Status.DSGetDocumentCheckBoxField("MEMBER_CONSENT_YES", envelopeId, doc.DSDocumentId);
                ViewBag.FName = ECAR.DocuSign.Status.DSGetDocumentFirstNameField("MEMBER_FN", envelopeId, doc.DSDocumentId);
                ViewBag.MName = ECAR.DocuSign.Status.DSGetDocumentTextField("MEMBER_MI", envelopeId, doc.DSDocumentId);
                ViewBag.LName = ECAR.DocuSign.Status.DSGetDocumentLastNameField("MEMBER_LN", envelopeId, doc.DSDocumentId);
                ViewBag.Suffix = ECAR.DocuSign.Status.DSGetDocumentTextField("MEMBER_SUFFIX", envelopeId, doc.DSDocumentId);
                ViewBag.SSN = ECAR.DocuSign.Status.DSGetDocumentSsnField("MEMBER_SSN", envelopeId, doc.DSDocumentId);
                ViewBag.DOB = ECAR.DocuSign.Status.DSGetDocumentDateField("MEMBER_DOB", envelopeId, doc.DSDocumentId);
                ViewBag.SignStatus = ECAR.DocuSign.Status.DSGetDocumentSignHereField("MEMBER_SIGNATURE", envelopeId, doc.DSDocumentId);
                ViewBag.SignDate = ECAR.DocuSign.Status.DSGetDocumentDateSignedField("MEMBER_SIGNATURE_DATE", envelopeId, doc.DSDocumentId);
                ViewBag.SignName = ECAR.DocuSign.Status.DSGetDocumentFirstNameField("MEMBER_SIGNATURE_FN", envelopeId, doc.DSDocumentId)
                    + " " + ECAR.DocuSign.Status.DSGetDocumentLastNameField("MEMBER_SIGNATURE_LN", envelopeId, doc.DSDocumentId);

                return View("../Home/About");
            }
            catch (Exception ex)
            {
                ErrorModel err = new ErrorModel
                {
                    Code = ex.HResult.ToString(),
                    Message = ex.Message,
                    Source = ex.Source,
                    ErrorContent = ex.StackTrace
                };

                return View("Error", err);
            }
        }

        // Data for this method: Envelope ID, Document ID
        public ActionResult GetDocument(string envelopeId, string documentId)
        {
            try
            {
                // Configure ECAR.DocuSign 
                SetDSConfig();

                // Get document content stream
                Stream results = ECAR.DocuSign.Status.DSGetDocument(envelopeId, documentId);

                // Return for download
                return File(results, "application/pdf", envelopeId + ".pdf");
            }
            catch (Exception ex)
            {
                ErrorModel err = new ErrorModel
                {
                    Code = ex.HResult.ToString(),
                    Message = ex.Message,
                    Source = ex.Source,
                    ErrorContent = ex.StackTrace
                };

                return View("Error", err);
            }
        }

        // This method reads file contents from the project's Resources directory
        internal static byte[] ReadContent(string fileName)
        {
            byte[] buff = null;
            string path = Path.Combine(HostingEnvironment.ApplicationPhysicalPath, "Resources", fileName);
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

        // Subroutine to set the DocuSign configution parameters (if not already set)
        internal bool SetDSConfig()
        {
            if (!ECAR.DocuSign.DocuSignConfig.Ready)
            {
                ECAR.DocuSign.DocuSignConfig.AccountID = MyAppConfig.GetConfiguration("DS_AccountId");
                ECAR.DocuSign.DocuSignConfig.ClientID = MyAppConfig.GetConfiguration("DS_ClientID");
                ECAR.DocuSign.DocuSignConfig.UserGUID = MyAppConfig.GetConfiguration("DS_UserGUID");
                ECAR.DocuSign.DocuSignConfig.AuthServer = MyAppConfig.GetConfiguration("DS_AuthServer");
                ECAR.DocuSign.DocuSignConfig.RSAKey = ReadContent(MyAppConfig.GetConfiguration("DS_RSAKeyFile"));
            }

            return ECAR.DocuSign.DocuSignConfig.Ready;
        }
    }
}
```