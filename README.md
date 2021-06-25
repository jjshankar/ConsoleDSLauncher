# ECAR.DocuSign
### Last release: May 13, 2021 (ver. 1.0.11)

A library to easily connect to DocuSign services and embed signing within your web application.  

This library also provides support for working with the DocuSign document and perform the following:
1. Check document signature status
2. Download the completed document from DocuSign servers
3. Extract user entered data from specific (named) data fields in the DocuSign document 
4. Pre-fill fields in the DocuSign document (and optionally lock) before presenting for signature

### [Link to current version on Azure DevOps Artifacts](https://dev.azure.com/epiqsystems/ECAR/_packaging?_a=package&feed=EC.Packages&view=versions&package=ECAR.DocuSign&protocolType=NuGet)


# Getting Started
Getting started with **ECAR.DocuSign** is as easy as 1..2..3
1.	Add reference to `ECAR.DocuSign` in your project
2.	Configure DocuSign authentication parameters 
3.	Call the method!


# Sample Code
[Sample Controller](Sample.md)


# Setup and Signing (using a DocuSign template)
## This is how you configure DocuSign authentication
```csharp
    if (!ECAR.DocuSign.DocuSignConfig.Ready)
    {
        ECAR.DocuSign.DocuSignConfig.AccountID = "«Your DocuSign Account ID»";
        ECAR.DocuSign.DocuSignConfig.ClientID = "«Your DocuSign Client ID»";
        ECAR.DocuSign.DocuSignConfig.UserGUID = "«Your DocuSign User ID»";
        ECAR.DocuSign.DocuSignConfig.AuthServer = "«Your DocuSign Authentication Server»";
        ECAR.DocuSign.DocuSignConfig.RSAKey = "«Content (NOT file name) of your DocuSign RSA KeyFile»";
    }
```

## This is how you set up the document to send
```csharp
    DocumentModel dsDoc = new DocumentModel
    {
        DSEmailSubject = "«Email Subject»",
        DSRoleName = "«Signer role name»",
        DSTemplateName = "«DocuSign template name»",
        SignerEmail = "«Recipient email»",
        SignerName = "«Recipient's name»",

        // DocuSign does not use the following attribute, but keeps it linked to the doc
        SignerId = "«Your application's tracking ID for this recipient»"       
    };
```

## This is how you prefill fields (if required)
The field names must be defined in your DocuSign template
- Pass the preset fields as a `List<DocPreset>` (even if passing a single preset)

```csharp
    DocPreset ssn = new DocPreset { Label = "MEMBER_SSN", Type= Presets.Ssn, Value= "123-45-6789", Locked = true };
    DocPreset dob = new DocPreset { Label = "MEMBER_DOB", Type = Presets.Date, Value = "1/1/1991", Locked = false };
    DocPreset check = new DocPreset { Label = "MEMBER_CONSENT_YES", Type = Presets.Checkbox, Value = "true" };
    DocPreset text = new DocPreset { Label = "MEMBER_EXPLAINS_UNIVERSE", Type = Presets.Text, Value = "Care to explain?" };

    List<DocPreset> tabPresets = new List<DocPreset> { ssn, dob, check, text };
```

## This is how you make the call to initiate the DocuSign ceremony
First, you set up the return URL where DocuSign should return to after the recipient finishes the signing ceremony.
- The DocuSign envelope ID value for this document will be passed back by **ECAR.DocuSign** as a parameter to this action for you to use it if needed (automatically appends `?envelopeId={id}` to the URL).

```csharp
    string returnUrl = "«Your application home page» " + "«Controller/Action to return to after DocuSign finishes»";
```

Next, you call the `EmbeddedTemplateSign` method passing in the `DocumentModel` object by *reference*.
- **ECAR.DocuSign** populates metadata about the DocuSign document in the returned object (e.g. envelope ID and document ID)
- Your application may need to save this information to retrieve data from this document in the future
- This call returns the URL for the DocuSign ceremony 

```csharp
    string docuSignUrl = ECAR.DocuSign.TemplateSign.EmbeddedTemplateSign(returnUrl, ref dsDoc, tabPresets);
```

Finally, redirect your users to the DocuSign page to start the signing ceremony.

```csharp
    return Redirect(docuSignUrl);
```

**ECAR.DocuSign** automatically redirects the user's browser to your application URL that was specified in the `returnUrl` argument.

## This is how you make the call to send an email from DocuSign for asynchronous signing
Simply call the `EmailedTemplateSign` method passing in the `DocumentModel` object by *reference* (optionally pass the preset fields).
- **ECAR.DocuSign** populates metadata about the DocuSign document in the returned object (e.g. envelope ID and document ID)
- Your application must save this information to poll the signature status for this document in the future
- The DocuSign status for the envelope is returned from this method 

```csharp
    string result = ECAR.DocuSign.TemplateSign.EmailedTemplateSign(ref dsDoc, tabPresets);
```


# Reminders and Expirations
## This is how you set up DocuSign to send reminders and expiration notices 
Use the corresponding models for reminders (`ReminderModel`) or expiration (`ExpirationModel`) to set up the parameters and call the overloaded methods for signing.

```csharp
    ReminderModel rem = new ReminderModel
    {
        ReminderEnabled = true,
        ReminderDelayDays = 1,
        ReminderFrequencyDays = 1
    };
```
```csharp
    ExpirationModel exp = new ExpirationModel
    {
        ExpirationEnabled = true,
        ExpireAfterDays = 3,
        ExpireWarnDays = 1
    };
```

Overloaded versions of signing methods are available that accept the reminder and expiration objects.  *Note: You may pass empty (`null`) objects for either argument.*
```csharp
    // Embedded signing 
    string viewUrl = ECAR.DocuSign.TemplateSign.EmbeddedTemplateSign(returnUrl, ref dsDoc, rem, exp, tabs);
```
```csharp
    // Email signing
    string status = ECAR.DocuSign.TemplateSign.EmailedTemplateSign(ref dsDoc, rem, exp, tabs);
```


# Using Webhooks
## This is how you set up a callback webhook method for asynchronous (email) signing
For email signing, DocuSign offers a method to receive envelope data as soon as its status changes. Utilizing a publicly accessible callback method (webhook), the calling application can receive notifications from DocuSign for specific changes to the envelope's status.

Set up the URL (must be `https://`) of the controller/action in a `NotificationCallBackModel` object and specify the envelope actions that trigger the callback.
```csharp
    // Create call back object
    NotificationCallBackModel notificationCallBack = new NotificationCallBackModel {
        // Set up events to monitor - full list shown here
        EnvelopeEvents = new List<string> {"Sent", "Completed", "Declined", "Delivered", "Voided"},

        // Webhook URL must be SSL (https://) 
        WebHookUrl = "«Your application/callback service home page» " + "«Controller/Action to call»"
    };
```

Call the overloaded signing method and pass the `NotificationCallBackModel` object.
```csharp
    // Call email signing method with callback
    string status = ECAR.DocuSign.TemplateSign.EmailedTemplateSignWithCallBack(ref dsDoc, rem, exp, 
        notificationCallBack, tabs);
```

DocuSign will automatically `POST` to the webhook action whenever the envelope status changes to one of the values passed in the event list.  You can process the event data (JSON) either directly, or by calling the `Process` method.
```csharp
    // in your webhook Controller/Action (HTTPPOST)
    {
        ...

        // Read Request body
        long bufLen = Request.InputStream.Length;
        byte[] buffer = new byte[bufLen];
        long readCount = Request.InputStream.Read(buffer, 0, (int)bufLen);

        // Convert byte[] to string
        string json = System.Text.Encoding.Default.GetString(buffer);

        // Call ECAR.DocuSign method to process the JSON and return an EnvelopeModel object
        ECAR.DocuSign.Models.EnvelopeModel env = ECAR.DocuSign.CallBack.Process(json);

        // Use the data in the EnvelopeModel object as you need
        
        ...
    }
```


# Status and Retrieval
## This is how you retrieve a list of DocuSign envelopes from a given date
```csharp
    // Past x days (30 days in this example)
    DateTime startDate = DateTime.Now.AddDays(-30);

    // ...or set up custom date
    startDate = new DateTime(«year», «month», «date»);

    // Retreive the envelopes
    List<EnvelopeModel> envModelList = ECAR.DocuSign.Status.DSGetAllEnvelopes((DateTime)startDate);

    // Iterate and list ALL properties
    foreach (EnvelopeModel env in envModelList)
    {
        Console.WriteLine("=======  Envelope ID: {0}  =======", env.EnvelopeId);
        Console.WriteLine("\tStatus: {0}", env.Status);
        Console.WriteLine("\tCompletedDateTime: {0}", env.CompletedDateTime);
        Console.WriteLine("\tCreatedDateTime: {0}", env.CreatedDateTime);
        Console.WriteLine("\tDeclinedDateTime: {0}", env.DeclinedDateTime);
        Console.WriteLine("\tEmailBlurb: {0}", env.EmailBlurb);
        Console.WriteLine("\tEmailSubject: {0}", env.EmailSubject);
        Console.WriteLine("\tExpireAfter: {0}", env.ExpireAfter);
        Console.WriteLine("\tExpireDateTime: {0}", env.ExpireDateTime);
        Console.WriteLine("\tExpireEnabled: {0}", env.ExpireEnabled);
        Console.WriteLine("\tLastModifiedDateTime: {0}", env.LastModifiedDateTime);
        Console.WriteLine("\tSentDateTime: {0}", env.SentDateTime);
        Console.WriteLine("\tStatusChangedDateTime: {0}", env.StatusChangedDateTime);
        Console.WriteLine("\tVoidedDateTime: {0}", env.VoidedDateTime);
        Console.WriteLine("\tVoidedReason: {0}", env.VoidedReason);
    }
```

## This is how you check the signature status of a DocuSign envelope
```csharp
    string status = ECAR.DocuSign.Status.DSCheckStatus(envelopeId);
```

## This is how you retrieve a list of ALL recipients for an envelope
```csharp
    // Retrieve ALL recipients for an envelope
    List<EnvelopeRecipientModel> allRecipients = ECAR.DocuSign.Status.DSGetAllRecipients(envelopeId);
    
    // Iterate and list properties for the recipients
    foreach (EnvelopeRecipientModel recipient in allRecipients)
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
```

## This is how you extract the list of documents included in an envelope
```csharp
    // Get all documents in the envelope 
    List<EnvelopeDocumentModel> docList = ECAR.DocuSign.Status.DSGetAllDocuments(envelopeId);
    
    // Get the first item in the list (or iterate if more than one document is available)
    //  NOTE: If the envelope is signed, the DocuSign certificate is attached as the last item in this list
    EnvelopeDocumentModel doc = docList[0];
```

## This is how you extract data from named fields in your document
```csharp
    // Get data from various control types
    string FirstName = ECAR.DocuSign.Status.DSGetDocumentFirstNameField("MEMBER_FIRST_NAME", envelopeId, doc.DocumentId);
    string LastName = ECAR.DocuSign.Status.DSGetDocumentLastNameField("MEMBER_LAST_NAME", envelopeId, doc.DocumentId);

    string Consent = ECAR.DocuSign.Status.DSGetDocumentCheckBoxField("MEMBER_CONSENT_YES", envelopeId, doc.DocumentId);

    string SSN = ECAR.DocuSign.Status.DSGetDocumentSsnField("MEMBER_SSN", envelopeId, doc.DocumentId);
    string DOB = ECAR.DocuSign.Status.DSGetDocumentDateField("MEMBER_DOB", envelopeId, doc.DocumentId);

    string Explanation = ECAR.DocuSign.Status.DSGetDocumentTextField("MEMBER_EXPLAINS_UNIVERSE", envelopeId, doc.DocumentId);

    string SignStatus = ECAR.DocuSign.Status.DSGetDocumentSignHereField("MEMBER_SIGNATURE", envelopeId, doc.DocumentId);
    string SignDate = ECAR.DocuSign.Status.DSGetDocumentDateSignedField("MEMBER_SIGNATURE_DATE", envelopeId, doc.DocumentId);
```

## This is how you download a document from DocuSign
```csharp
    Stream results = ECAR.DocuSign.Status.DSGetDocument(envelopeId, documentId);

    // Return for download
    return File(results, "application/pdf", "«Whatever you want the document name to be»");
```

# Limitations/known issues
- Supports only the use of a template created in DocuSign (template must exist in DocuSign)
- Does not support deferred sending (create now, send later) for DocuSign envelopes

# Completed enhancements
- [x] ~~Email document for signing asynchronously~~ *Avaialable with 10/1/2020 release (>1.0.5)*
- [x] ~~Retrieve a list of DocuSign envelopes~~ *Avaialable with 10/9/2020 release (>1.0.7)*
- [x] ~~Retrieve a list of envelope recipients~~ *Avaialable with 10/20/2020 release (>1.0.8)*
- [x] ~~Retrieve a list of envelope documents~~ *Avaialable with 10/20/2020 release (>1.0.8)*
- [x] ~~Support for reminders and expirations~~ *Avaialable with 5/11/2020 release (>1.0.11)*

# Future enhancements
- [ ] Prepare and present a custom document (passed in from the calling application) to the recipient

# Maintenance releases
- [x] ~~Updated to use DocuSign.eSign.DLL v5.2~~ *Avaialable with 03/02/2021 release (>1.0.10)*
- [x] ~~Updated to support .NETCore3.1~~ *Avaialable with 03/02/2021 release (>1.0.10)*

# Contribute
Share your feedback/suggestions/requests
- [Jesus Shankar](mailto:jshankar@epiqglobal.com)
