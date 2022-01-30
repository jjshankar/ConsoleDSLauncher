# ECAR.DocuSign
### Last release: January 31, 2022 (ver. 1.0.31)

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


# Starter Code 
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
        DSEmailBody = "«Email Body»",
        DSRoleName = "«Signer role name»",
        DSTemplateName = "«DocuSign template name»",
        SignerEmail = "«Recipient email»",
        SignerName = "«Recipient's name»",

        // DocuSign does not use the following attribute, but keeps it linked to the doc
        SignerId = "«Your application's tracking ID for this recipient»",

        // Optional configuration value to show/hide envelope ID in the sent document (default = true)
        DSStampEnvelopeID = false,

        // Optional configuration value to block recipients from reassigning envelopes sent to them (default = true).
        DSAllowReassign = false,

        // Optional configuration value block recipients from printing and signing (wet sign) envelopes sent to them (default = true).
        DSAllowPrintAndSign = false;

    };
```
*The `DSStampEnvelopeID` property requires a corresponding setting in DocuSign Settings under **Sending Settings** to* "Include Envelope ID by default".  *Contact your DocuSign admin to enable this setting.*

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

Overloaded versions of signing methods are available that accept the reminder and expiration objects.  You may pass empty (`null`) objects for either argument.

```csharp
    // Embedded signing 
    string viewUrl = ECAR.DocuSign.TemplateSign.EmbeddedTemplateSign(returnUrl, ref dsDoc, rem, exp, tabs);
```
```csharp
    // Email signing
    string status = ECAR.DocuSign.TemplateSign.EmailedTemplateSign(ref dsDoc, rem, exp, tabs);
```


# Previews
## This is how you create a preview of the DocuSign view the recipient would see
Simply call the `CreatePreviewURL` method passing in the `DocumentModel` object by *reference* and other objects.  You will need to supply a boolean value indicating if this would be sent by mail.  The `AppReturnUrl` agrument is ignored by DocuSign, but required.
- This method returns the preview URL that you can show in an `<iframe>`.
- **ECAR.DocuSign** populates metadata about the DocuSign document in the returned object (e.g. envelope ID, document ID and preview URL)
- Your application must save this information to send this document after preview

```csharp
    string previewUrl = ECAR.DocuSign.TemplateSign.CreatePreviewURL(ref dsDoc, appReturnUrl, sendByMail, rem, exp, hook, tabs);
```

## This is how you send the previewed DocuSign envelope to the recipient 
To send the document that you previewed (after approval), call the `SendPreviewedEnvelope` method, and pass in a `DocumentModel` object containing the envelope ID of the preview.  A special constructor exists for the `DocumentModel` object that takes just an envelope ID argument.
- This method returns true when successful, or an exception in case of errors.

```csharp
    DocumentModel doc = new DocumentModel(previewEnvelopeId);
    bool bResult = SendPreviewedEnvelope(doc);
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
        System.Diagnostics.Trace.TraceInformation("DocuSign envelope id: {0} was set to status: {1} on {2}", env.EnvelopeId, env.Status, env.StatusChangedDateTime);
        
        ...
    }
```

***NOTE: EXAMINE THE RETURNED*** `EnvelopeModel` ***MODEL FOR OBJECTS AND PROPERTIES THAT ARE AVAILABLE TO THE WEBHOOK METHOD.***

# Sending multiple documents in one envelope
Sending document packets that consist of many DocuSign templates, just create a new instance of the `DocumentPacketModel` object and populate it with the names of the templates you would like to include in the envelope (using its `DSTemplateList` property).
```csharp
    DocumentPacketModel dsDocPacket = new DocumentPacketModel
    {
        DSEmailSubject = "Please sign this document packet (2 docs).",
        DSRoleName = DSROLENAME,
        DSTemplateList = new List<string> { DOCNAME, DOC2NAME },
        SignerEmail = signerEmail,
        SignerId = signerId,
        SignerName = signerName
    };
```

Set up the preset tabs, the return URL (for embedded sign) or notification (for email sign) objects, set up reminder and expiration if you require them, and call the corresponding `...PacketSign` methods.
```csharp
    // Template sign
    string viewUrl = ECAR.DocuSign.TemplateSign.EmbeddedPacketSign(returnUrl, ref dsDocPacket, null, null, tabs);

    // Email sign
    string status = ECAR.DocuSign.TemplateSign.EmailedPacketSign(ref dsDocPacket, hook, null, null, tabs);
```

# Bulk/Batch Sending
For sending the same document or packet to multiple recipients, utilizing DocuSign's bulk send function is recommended.  This reduces the number of calls made to the DocuSign API and provides a better user experience - the caller need not wait for all envelopes to finish sending; they can fire off the bulk send call and monitor for status changes in the webhook method.

To do this, set up the corresponding bulk send object for a single document or multi-document packet.  In each of these objects, you will need to populate the recipient data and presets for each recipient in the corresponding `List<>` objects.

```csharp
    // Single Document
    BulkSendDocumentList dsBulkDocList = new BulkSendDocumentList
    {
        BulkRecipientList = new List<BulkSendRecipientModel>(),     // set up recipient list
        BulkBatchName = "«Custom name for your batch»",
        DSBatchTemplateName = "«The template you want to send»",
        BulkEmailSubject = "«Custom email subject for your batch»",
        BulkEmailBody = "«Custom email body text for your batch»",
    };

    // Multi-doc packet
    BulkSendPacketList  dsBulkPacketList = new BulkSendPacketList
    {
        BulkPacketRecipientList = new List<BulkSendPacketRecipientModel>(),    // set up recipient list
        BulkBatchName = "«Custom name for your batch»",
        DSBatchPacketTemplates = new List<string> { "«Template 1»", "«Template 2»", ... },
        BulkEmailSubject = "«Custom email subject for your batch»",
        BulkEmailBody = "«Custom email body text for your batch»",
    };
```

Once the data is prepared, set up the notification object, and call the corresponding `BulkSend...` methods.  The ID of the batch returned from DocuSign is sent back to the caller.  The List ID and Batch ID are also populated in the bulk document list sent to the call.

DocuSign automatically adds a custom field for the Batch ID (`"BulkBatchId"`) to each envelope in the batch.  In additon, each envelope in the batch will also contain custom fields for List ID (`"BULK_MAILING_LIST_ID"`) and Signer ID (`"BULK_MAILING_SIGNER_ID"`).  You can retrieve these per envelope to match it back to a batch, list and/or a signer.  See below for custom field retrieval.

```csharp
    // Single doc
    string batchId = ECAR.DocuSign.TemplateSign.BulkSendTemplate(ref dsBulkDocList, hook);

    // Doc Packet
    string batchId = ECAR.DocuSign.TemplateSign.BulkSendPacket(ref dsBulkPacketList, hook);
```

The caller may store the batch ID (returned value or the `DSBatchID` property) after this call for status queries in the future.  

***NOTE: DOCUSIGN TAKES A WHILE TO DISPATCH ALL THE DOCUMENTS IN THE BATCH. USE YOUR JUDGMENT BEFORE INITIATING THE STATUS QUERY FOR A BATCH.***

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

## This is how you retrieve a list of DocuSign envelopes from a specific batch
```csharp
    // Get batch status for a given batchID
    List<EnvelopeModel> envs = ECAR.DocuSign.Status.DSGetBulkBatchEnvelopes(batchId);

    foreach(EnvelopeModel env in envs)
    {
        Console.WriteLine("=======  Envelope ID: {0}  =======", env.EnvelopeId);
        Console.WriteLine("\tEnvelope Sent: {0}", env.SentDateTime);
        Console.WriteLine("\tEnvelope Delivered: {0}", env.DeliveredDateTime);
        Console.WriteLine("\tEnvelope Status: {0}", env.Status);
        Console.WriteLine("\tEnvelope Status Changed on: {0}", env.StatusChangedDateTime);
        Console.WriteLine("\tEnvelope Delivered: {0}", env.DeliveredDateTime);
    }  
```

## This is how you check the signature status of a DocuSign envelope
```csharp
    string status = ECAR.DocuSign.Status.DSCheckStatus(envelopeId);
```

## This is how you retrieve a dictionary of custom fields in a DocuSign envelope
```csharp
    Dictionary<string, string> customFields = ECAR.DocuSign.Status.DSGetEnvelopeCustomFields(envelopeId);
```
Query the returned collection for the Batch ID (Key: `"BulkBatchId"`), List ID (Key: `"BULK_MAILING_LIST_ID"`) and Signer ID (Key: `"BULK_MAILING_SIGNER_ID"`) to get the corresponding values.

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

## This is how you resend a previously sent envelope
Call the `DSResendEnvelope` method and pass in the ID of the envelope to resend.  The envelope must be in one of `created`, `sent` or `delivered` states.
- This method returns true when successful, or an exception if the envelope is in an invalid state or in case of errors.

```csharp
    bool bResult = ECAR.DocuSign.Status.DSResendEnvelope(createdEnvelopeId);
```

## This is how you void a previously sent envelope
Call the `DSVoidEnvelope` method pass in the ID of the envelope to cancel/void.  The method also requires a non-empty string specifying the reason for voiding.  The envelope must *not* be in a `draft` or `completed` states.
- This method returns true when successful, or an exception if the envelope is in an invalid state or in case of errors.

```csharp
    bool bResult = ECAR.DocuSign.Status.DSVoidEnvelope(createdEnvelopeID, voidedReason)
```
***NOTE: THIS ACTION IS IRREVERSIBLE!***


# Change Sending User
## This is how to switch the DocuSign user that sends the envelopes 
To change the DocuSign user that sends the envelopes, simply change the configuration value and call the standard methods.  All user IDs must exist as a valid user for your account in DocuSign.
- Changing the user ID regenerates the DocuSign JWT. Use this feature sparingly.

```csharp
    if(toggleSendAs)
        DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID_2"];
    else
        DocuSignConfig.UserGUID = ConfigurationManager.AppSettings["DS_UserGUID_1"];
```

***NOTE: THE USER LAST SET WILL BE USED FOR ALL SUBSEQUENT CALLS. EXERCISE CAUTION WHILE USING THIS FEATURE.***


# Limitations/known issues
- Supports only the use of a template created in DocuSign (template must exist in DocuSign)
- ~~Does not support deferred sending (create now, send later) for DocuSign envelopes~~ *Available with 7/14/2021 release (>1.0.19)*

# Completed enhancements
- [x] Email document for signing asynchronously. *Available with 10/1/2020 release (>1.0.5)*
- [x] Retrieve a list of DocuSign envelopes. *Available with 10/9/2020 release (>1.0.7)*
- [x] Retrieve a list of envelope recipients. *Available with 10/20/2020 release (>1.0.8)*
- [x] Retrieve a list of envelope documents. *Available with 10/20/2020 release (>1.0.8)*
- [x] Support for reminders and expirations. *Available with 5/11/2021 release (>1.0.11)*
- [x] Added support for emailing DocuSign envelopes (async signing). *Available with 5/11/2021 release (>1.0.11)*
- [x] Added support for webhook callback from DocuSign for status change events for emailed envelopes. *Available with 6/25/2021 release (>1.0.18)*
- [x] Added support for DocuSign preview, resends and voiding. *Available with 7/14/2021 release (>1.0.19)*
- [x] Added support for sending multi-template document packets. *Available with 7/23/2021 release (>1.0.20)*
- [x] Added support for batch sending single and document packets to multiple recipients. *Available with 7/23/2021 release (>1.0.20)*
- [x] Custom fields to return the signer ID and batch ID for an envelope sent as part of a batch for matching. *Available with 8/25/2021 release (>1.0.21)*
- [x] Custom email subject and body for single emails. *Available with 8/30/2021 release (>1.0.24)*
- [x] Added option to suppress envelope ID stamping in mailed envelopes. *Available with 9/10/2021 release (>1.0.25)*
- [x] Extended envelope ID stamping to bulk envelopes. *Available with 9/22/2021 release (>1.0.27)*
- [x] Added option to disallow envelope reassignment by recipients. *Available with 9/22/2021 release (>1.0.27)*
- [x] Receive custom fields in the callback JSON so it can be cached to reduce DocuSign API calls. *Available with 11/29/2021 release (>1.0.28)*
- [x] Added support to suppress/enable wet signing (print and sign) in DocuSign. *Available with 11/29/2021 release (>1.0.28)*
- [x] Added support to change the DocuSign SendAs user within the same session. *Available with 11/29/2021 release (>1.0.28)*
- [x] Added support to retrieve decline reason (if supplied) in the webhook Process method. *Available with 1/31/2022 release (>1.0.31)*

# Future enhancements
- [ ] Prepare and present a custom document (passed in from the calling application) to the recipient

# Maintenance releases
- [x] Updated to support .NETCore3.1. *Available with 03/02/2021 release (>1.0.10)*
- ~~Updated to use DocuSign.eSign.DLL v5.2. *Available with 03/02/2021 release (>1.0.10)*~~
- ~~Updated to use DocuSign.eSign.DLL v5.6.2. *Available with 7/23/2021 release (>1.0.20)*~~
- [x] Updated to use DocuSign.eSign.DLL v5.8.0. *Available with 1/31/2022 release (>1.0.31)*

# Contribute
Share your feedback/suggestions/requests
- [Jesus Shankar](mailto:jshankar@epiqglobal.com)
