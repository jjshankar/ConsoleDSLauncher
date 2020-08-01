# ECAR.DocuSign
A library to easily connect to DocuSign services and embed signing within your web application.  

This library also provides support for working with the DocuSign document and perform the following:
1. Check document signature status
2. Download the completed document from DocuSign servers
3. Extract user entered data from specific (named) data fields in the DocuSign document 
4. Pre-fill fields in the DocuSign document (and optionally lock) before presenting for signature

# Getting Started
Getting started with ECAR.DocuSign is as easy as 1..2..3
1.	Add reference to ECAR.DocuSign in your project
2.	Configure DocuSign authentication parameters 
3.	Call the method!


# Sample Code

## This is how you configure DocuSign authentication
```csharp
    if (!ECAR.DocuSign.DocuSignConfig.Ready)
    {
        ECAR.DocuSign.DocuSignConfig.AccountID = ""«Your DocuSign Account ID»";
        ECAR.DocuSign.DocuSignConfig.ClientID = ""«Your DocuSign Client ID»";
        ECAR.DocuSign.DocuSignConfig.UserGUID = ""«Your DocuSign User ID»";
        ECAR.DocuSign.DocuSignConfig.AuthServer = "«Your DocuSign Authentication Server»";
        ECAR.DocuSign.DocuSignConfig.RSAKey = "«Content (NOT file name) of your DocuSign RSA KeyFile»";
    }
```

## This is how you set up the document you need to send
```csharp
    DocumentModel dsDoc = new DocumentModel
    {
        DSEmailSubject = "«Email Subject» ",
        DSRoleName = "«Signer role name»",
        DSTemplateName = "«DocuSign template name»",
        SignerEmail = "«Recipient email»",
        SignerName = "«Recipient's name»",
        SignerId = "«Your application's tracking ID for this recipient»"       // DocuSign does not use this field, but keeps it linked to the doc
    };
```

### This is how you prefill fields (if required)
The field names must be defined in your DocuSign template
- Pass the preset fields as a `List<DocPreset>` (even if passing a single preset)
```csharp
    DocPreset ssn = new DocPreset { Label = "MEMBER_SSN", Type= Presets.Ssn, Value= "123-45-6789", Locked = true };
    DocPreset dob = new DocPreset { Label = "MEMBER_DOB", Type = Presets.Date, Value = "1/1/1991", Locked = false };
    DocPreset check = new DocPreset { Label = "MEMBER_CONSENT_YES", Type = Presets.Checkbox, Value = "true" };
    DocPreset text = new DocPreset { Label = "MEMBER_EXPLAINS_UNIVERSE", Type = Presets.Text, Value = "Yeah! We got it." };

    List<DocPreset> tabs = new List<DocPreset> { ssn, dob, check, text };
```

## This is how you make the call to initiate the DocuSign ceremony
First you set up the return URL where DocuSign should return to after the recipient finishes the signing ceremony.

- The DocuSign envelope ID value for this document will be passed back by ECAR.DocuSign as a paramter to this action (automatically appends `?envelopeId={id}` to the URL) for you to use it if needed.

```csharp
    string returnUrl = "«Your application home page» " + "«Controller/Action to return to after DocuSign finishes»";
```

Next, you call the `EmbeddedTemplateSign` method passing in the `DocumentModel` object by *reference*.
- ECAR.DocuSign populates metadata about the DocuSign document in the returned object (e.g. envelope ID and document ID)
- Your application may need to save this information to retrieve data from this document in the future

```csharp
    string docuSignUrl = ECAR.DocuSign.TemplateSign.EmbeddedTemplateSign(returnUrl, ref dsDoc, tabs);
```

This call returns the DocuSign URL that you will redirect your users to.
```csharp
    return Redirect(docuSignUrl);
```

## This is how you check the signature status of a DocuSign envelope
```csharp
    string status = ECAR.DocuSign.Status.DSCheckStatus(envelopeId);

```
## This is how you extract data from named fields in your document
```csharp
    // Get the DocumentModel from the envelope
    DocumentModel doc = ECAR.DocuSign.Status.DSGetDocInfo(envelopeId);

    // Get data from various control types
    string FName = ECAR.DocuSign.Status.DSGetDocumentFirstNameField("MEMBER_FN", envelopeId, doc.DSDocumentId);
    string LName = ECAR.DocuSign.Status.DSGetDocumentLastNameField("MEMBER_LN", envelopeId, doc.DSDocumentId);

    string Consent = ECAR.DocuSign.Status.DSGetDocumentCheckBoxField("MEMBER_CONSENT_YES", envelopeId, doc.DSDocumentId);

    string SSN = ECAR.DocuSign.Status.DSGetDocumentSsnField("MEMBER_SSN", envelopeId, doc.DSDocumentId);
    string DOB = ECAR.DocuSign.Status.DSGetDocumentDateField("MEMBER_DOB", envelopeId, doc.DSDocumentId);

    string Explanation = ECAR.DocuSign.Status.DSGetDocumentTextField("MEMBER_EXPLAINS_UNIVERSE", envelopeId, doc.DSDocumentId);

    string SignStatus = ECAR.DocuSign.Status.DSGetDocumentSignHereField("MEMBER_SIGNATURE", envelopeId, doc.DSDocumentId);
    string SignDate = ECAR.DocuSign.Status.DSGetDocumentDateSignedField("MEMBER_SIGNATURE_DATE", envelopeId, doc.DSDocumentId);
```

## This is how you download a document from DocuSign
```csharp
    Stream results = ECAR.DocuSign.Status.DSGetDocument(envelopeId, documentId);

    // Return for download
    return File(results, "application/pdf", "«Whatever you want the document name to be»");
```

# Limitations/Known issues
- Supports only an embedded ceremony, performed live from the calling application
- Supports only the use of a template uploaded to DocuSign
- Supports only one document per DocuSign envelope

# Future enhancements
- Email document for signing asynchronously 
- Prepare and present a custom document (passed in from the calling application) to the recipient

# Contribute
Please share your feedback/suggestions
- [Jesus Shankar](mailto:jshankar@epiqglobal.com)
