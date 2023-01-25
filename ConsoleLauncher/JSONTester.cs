using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ECAR.DocuSign.Models;
using Newtonsoft.Json.Linq;

namespace ConsoleLauncher
{
    /// <summary>
    /// Data model for envelopes retrieved from DocuSign.
    /// </summary>
    public class LOCAL_EnvelopeModel
    {
        /// <summary>
        /// The date and time the item was last modified (UTC/ISO).
        /// </summary>
        public string LastModifiedDateTime { get; internal set; }

        /// <summary>
        /// If the envelope is set to expire
        /// </summary>
        public bool ExpireEnabled { get; internal set; }

        /// <summary>
        /// The expiration DateTime, if exists (UTC/ISO).
        /// </summary>
        public string ExpireDateTime { get; internal set; }

        /// <summary>
        /// Expiration ticks in minutes.
        /// </summary>
        public int ExpireAfter { get; internal set; }

        /// <summary>
        /// The date and time the envelope or template was voided, if exists (UTC/ISO).
        /// </summary>
        public string VoidedDateTime { get; internal set; }

        /// <summary>
        /// The date and time the status changed (UTC/ISO)..
        /// </summary>
        public string StatusChangedDateTime { get; internal set; }

        /// <summary>
        /// Indicates the envelope's current status. 
        ///</summary>
        public string Status { get; internal set; }

        /// <summary>
        /// The date and time the envelope was sent (UTC/ISO).
        /// </summary>
        public string SentDateTime { get; internal set; }

        /// <summary>
        /// The GUID identifier for the envelope.
        /// </summary>
        public string EnvelopeId { get; internal set; }

        /// <summary>
        /// The reason the envelope or template was voided, if exists.
        /// </summary>
        public string VoidedReason { get; internal set; }

        /// <summary>
        /// Specifies the date and time this item was completed, if exists (UTC/ISO).
        /// </summary>
        public string CompletedDateTime { get; internal set; }

        /// <summary>
        /// Indicates the date and time the item was created (UTC/ISO).
        /// </summary>
        public string CreatedDateTime { get; internal set; }

        /// <summary>
        /// Specifies the subject of the email that is sent to all recipients, if exists. 
        /// </summary>
        public string EmailSubject { get; internal set; }

        /// <summary>
        /// Text that is included in email body for all envelope recipients, if exists.
        /// </summary>
        public string EmailBlurb { get; internal set; }

        /// <summary>
        /// The date and time the recipient declined the document, if exists (UTC/ISO).
        /// </summary>
        public string DeclinedDateTime { get; internal set; }

        /// <summary>
        /// Key Value pair of custom fields contained in this envelope.
        /// </summary>
        public Dictionary<string, string> EnvelopeCustomFields { get; internal set; }

    }

    public static class JSONTester
    {
        private static LOCAL_EnvelopeModel Process_local(string jsonPayload)
        {
            JToken root = JObject.Parse(jsonPayload);
            LOCAL_EnvelopeModel envelopeModel = new LOCAL_EnvelopeModel
            {
                Status = (string)(root.SelectToken("status") ?? ""),
                CompletedDateTime = (string)(root.SelectToken("completedDateTime") ?? ""),
                LastModifiedDateTime = (string)(root.SelectToken("lastModifiedDateTime") ?? ""),
                EmailSubject = (string)(root.SelectToken("emailSubject") ?? ""),
                ExpireEnabled = bool.Parse((string)(root.SelectToken("expireEnabled") ?? "false")),
                ExpireDateTime = (string)(root.SelectToken("expireDateTime") ?? ""),
                ExpireAfter = (int)(root.SelectToken("expireAfter") ?? 0),
                VoidedDateTime = (string)(root.SelectToken("voidedDateTime") ?? ""),
                StatusChangedDateTime = (string)(root.SelectToken("statusChangedDateTime") ?? ""),
                SentDateTime = (string)(root.SelectToken("sentDateTime") ?? ""),
                EnvelopeId = (string)(root.SelectToken("envelopeId") ?? ""),
                VoidedReason = (string)(root.SelectToken("voidedReason") ?? ""),
                CreatedDateTime = (string)(root.SelectToken("createdDateTime") ?? ""),
                DeclinedDateTime = (string)(root.SelectToken("declinedDateTime") ?? ""),
            };

            // IEnumerable<JToken> customFields = root.SelectTokens("$..customFields.textCustomFields[?(@.fieldId != 0)].fieldId");
            JToken customFields = root["customFields"];
            if (customFields != null)
            {
                Dictionary<string, string> dictCustomFields = new Dictionary<string, string>();

                JEnumerable<JToken> listCustomFields = customFields["listCustomFields"].Children();
                JEnumerable<JToken> textCustomFields = customFields["textCustomFields"].Children();

                foreach (JToken titem in textCustomFields)
                    dictCustomFields.Add((string)titem["name"], (string)titem["value"]);

                foreach (JToken litem in listCustomFields)
                    dictCustomFields.Add((string)litem["name"], (string)litem["value"]);

                // Return the dictionary
                envelopeModel.EnvelopeCustomFields = dictCustomFields;
            }
            else
                Console.WriteLine("No custom fields!");

            return envelopeModel;
        }

        public static void RunJSONTester()
        {
            //string jsonPayload = "{" +
            //"	\"status\": \"completed\"," +
            //"	\"documentsUri\": \"/envelopes/eb8b533f-4d93-4df2-afda-0c91ae452cbd/documents\"," +
            //"	\"recipientsUri\": \"/envelopes/eb8b533f-4d93-4df2-afda-0c91ae452cbd/recipients\"," +
            //"	\"attachmentsUri\": \"/envelopes/eb8b533f-4d93-4df2-afda-0c91ae452cbd/attachments\"," +
            //"	\"envelopeUri\": \"/envelopes/eb8b533f-4d93-4df2-afda-0c91ae452cbd\"," +
            //"	\"emailSubject\": \"Batch send DocuSign\"," +
            //"	\"emailBlurb\": \"Batch send email body\"," +
            //"	\"envelopeId\": \"eb8b533f-4d93-4df2-afda-0c91ae452cbd\"," +
            //"	\"signingLocation\": \"online\"," +
            //"	\"customFieldsUri\": \"/envelopes/eb8b533f-4d93-4df2-afda-0c91ae452cbd/custom_fields\"," +
            //"	\"notificationUri\": \"/envelopes/eb8b533f-4d93-4df2-afda-0c91ae452cbd/notification\"," +
            //"	\"enableWetSign\": \"true\"," +
            //"	\"allowMarkup\": \"false\"," +
            //"	\"allowReassign\": \"false\"," +
            //"	\"createdDateTime\": \"2021-09-27T21:46:32.71Z\"," +
            //"	\"lastModifiedDateTime\": \"2021-09-27T21:46:32.71Z\"," +
            //"	\"deliveredDateTime\": \"2021-09-27T21:48:30.427Z\"," +
            //"	\"initialSentDateTime\": \"2021-09-27T21:46:34.117Z\"," +
            //"	\"sentDateTime\": \"2021-09-27T21:46:34.117Z\"," +
            //"	\"completedDateTime\": \"2021-09-27T21:48:51.72Z\"," +
            //"	\"statusChangedDateTime\": \"2021-09-27T21:48:51.72Z\"," +
            //"	\"documentsCombinedUri\": \"/envelopes/eb8b533f-4d93-4df2-afda-0c91ae452cbd/documents/combined\"," +
            //"	\"certificateUri\": \"/envelopes/eb8b533f-4d93-4df2-afda-0c91ae452cbd/documents/certificate\"," +
            //"	\"templatesUri\": \"/envelopes/eb8b533f-4d93-4df2-afda-0c91ae452cbd/templates\"," +
            //"	\"expireEnabled\": \"true\"," +
            //"	\"expireDateTime\": \"2022-01-25T21:46:34.117Z\"," +
            //"	\"expireAfter\": \"120\"," +
            //"	\"sender\": {" +
            //"		\"userName\": \"Official Sender\"," +
            //"		\"userId\": \"70b66a1b-14fe-4e0b-ab63-3b48cff8b369\"," +
            //"		\"accountId\": \"600f564a-9029-4926-8faf-79eb73dbc93d\"," +
            //"		\"email\": \"jshankar@epiqglobal.com\"" +
            //"	}," +
            //"	\"customFields\": {" +
            //"		\"textCustomFields\": [" +
            //"			{" +
            //"				\"fieldId\": \"10614137158\"," +
            //"				\"name\": \"templateUsageRestriction\"," +
            //"				\"show\": \"false\"," +
            //"				\"required\": \"false\"," +
            //"				\"value\": \"allOptions\"" +
            //"			}," +
            //"			{" +
            //"				\"fieldId\": \"10614137159\"," +
            //"				\"name\": \"BULK_MAILING_LIST_ID\"," +
            //"				\"show\": \"false\"," +
            //"				\"required\": \"false\"," +
            //"				\"value\": \"2d00ade1-dfdd-4eba-9e66-dbec9ac3ea88\"" +
            //"			}," +
            //"			{" +
            //"				\"fieldId\": \"10614137160\"," +
            //"				\"name\": \"BULK_MAILING_SIGNER_ID\"," +
            //"				\"show\": \"false\"," +
            //"				\"required\": \"false\"," +
            //"				\"value\": \"12314\"" +
            //"			}," +
            //"			{" +
            //"				\"fieldId\": \"10614137161\"," +
            //"				\"name\": \"BulkBatchId\"," +
            //"				\"show\": \"false\"," +
            //"				\"required\": \"false\"," +
            //"				\"value\": \"a63a0128-4976-4e3a-be0c-b3ab57b9bc95\"" +
            //"			}" +
            //"		]," +
            //"		\"listCustomFields\": []" +
            //"	}," +
            //"	\"recipients\": {" +
            //"		\"signers\": [" +
            //"			{" +
            //"				\"tabs\": {" +
            //"					\"signHereTabs\": [" +
            //"						{" +
            //"							\"stampType\": \"signature\"," +
            //"							\"name\": \"SignHere\"," +
            //"							\"tabLabel\": \"MEMBER_SIGNATURE\"," +
            //"							\"scaleValue\": \"1\"," +
            //"							\"optional\": \"false\"," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"4\"," +
            //"							\"xPosition\": \"119\"," +
            //"							\"yPosition\": \"159\"," +
            //"							\"width\": \"0\"," +
            //"							\"height\": \"0\"," +
            //"							\"tabId\": \"62fcec59-eb91-4400-8a3a-7ecb70770789\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"status\": \"signed\"," +
            //"							\"tabType\": \"signhere\"" +
            //"						}" +
            //"					]," +
            //"					\"dateSignedTabs\": [" +
            //"						{" +
            //"							\"name\": \"DateSigned\"," +
            //"							\"value\": \"2021-09-27T21:48:51Z\"," +
            //"							\"tabLabel\": \"MEMBER_SIGNATURE_DATE\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"4\"," +
            //"							\"xPosition\": \"456\"," +
            //"							\"yPosition\": \"166\"," +
            //"							\"width\": \"0\"," +
            //"							\"height\": \"0\"," +
            //"							\"tabId\": \"068476f6-232a-4848-b12f-2c9a53b6843b\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"datesigned\"" +
            //"						}" +
            //"					]," +
            //"					\"textTabs\": [" +
            //"						{" +
            //"							\"validationPattern\": \"\"," +
            //"							\"validationMessage\": \"\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"requireAll\": \"false\"," +
            //"							\"value\": \"\"," +
            //"							\"required\": \"false\"," +
            //"							\"locked\": \"false\"," +
            //"							\"concealValueOnDocument\": \"false\"," +
            //"							\"disableAutoSize\": \"false\"," +
            //"							\"maxLength\": \"2\"," +
            //"							\"tabLabel\": \"MEMBER_MI\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"268\"," +
            //"							\"yPosition\": \"174\"," +
            //"							\"width\": \"24\"," +
            //"							\"height\": \"11\"," +
            //"							\"tabId\": \"32c4e9f1-ed1e-4725-98d3-51f3f50bbb3d\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"text\"" +
            //"						}," +
            //"						{" +
            //"							\"validationPattern\": \"\"," +
            //"							\"validationMessage\": \"\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"requireAll\": \"false\"," +
            //"							\"value\": \"\"," +
            //"							\"required\": \"false\"," +
            //"							\"locked\": \"false\"," +
            //"							\"concealValueOnDocument\": \"false\"," +
            //"							\"disableAutoSize\": \"false\"," +
            //"							\"maxLength\": \"25\"," +
            //"							\"tabLabel\": \"REP_FN\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"44\"," +
            //"							\"yPosition\": \"235\"," +
            //"							\"width\": \"240\"," +
            //"							\"height\": \"11\"," +
            //"							\"tabId\": \"efbd5d16-1cc5-4131-8511-9af254df2af5\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"text\"" +
            //"						}," +
            //"						{" +
            //"							\"validationPattern\": \"\"," +
            //"							\"validationMessage\": \"\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"requireAll\": \"false\"," +
            //"							\"value\": \"\"," +
            //"							\"required\": \"false\"," +
            //"							\"locked\": \"false\"," +
            //"							\"concealValueOnDocument\": \"false\"," +
            //"							\"disableAutoSize\": \"false\"," +
            //"							\"maxLength\": \"4\"," +
            //"							\"tabLabel\": \"REP_SUFFIX\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"535\"," +
            //"							\"yPosition\": \"235\"," +
            //"							\"width\": \"30\"," +
            //"							\"height\": \"11\"," +
            //"							\"tabId\": \"7ea324bd-db8d-4ddd-8566-5080dc6acbc9\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"text\"" +
            //"						}," +
            //"						{" +
            //"							\"validationPattern\": \"\"," +
            //"							\"validationMessage\": \"\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"requireAll\": \"false\"," +
            //"							\"value\": \"\"," +
            //"							\"required\": \"false\"," +
            //"							\"locked\": \"false\"," +
            //"							\"concealValueOnDocument\": \"false\"," +
            //"							\"disableAutoSize\": \"false\"," +
            //"							\"maxLength\": \"25\"," +
            //"							\"tabLabel\": \"REP_LN\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"304\"," +
            //"							\"yPosition\": \"235\"," +
            //"							\"width\": \"240\"," +
            //"							\"height\": \"11\"," +
            //"							\"tabId\": \"9f945e9a-90f2-4336-a867-84c51de5e4b4\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"text\"" +
            //"						}," +
            //"						{" +
            //"							\"validationPattern\": \"\"," +
            //"							\"validationMessage\": \"\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"requireAll\": \"false\"," +
            //"							\"value\": \"\"," +
            //"							\"required\": \"false\"," +
            //"							\"locked\": \"false\"," +
            //"							\"concealValueOnDocument\": \"false\"," +
            //"							\"disableAutoSize\": \"false\"," +
            //"							\"maxLength\": \"2\"," +
            //"							\"tabLabel\": \"REP_MI\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"268\"," +
            //"							\"yPosition\": \"235\"," +
            //"							\"width\": \"24\"," +
            //"							\"height\": \"11\"," +
            //"							\"tabId\": \"cb9fb674-5568-4f76-b51f-8b4dc90723c6\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"text\"" +
            //"						}," +
            //"						{" +
            //"							\"validationPattern\": \"\"," +
            //"							\"validationMessage\": \"\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"requireAll\": \"false\"," +
            //"							\"value\": \"\"," +
            //"							\"required\": \"false\"," +
            //"							\"locked\": \"false\"," +
            //"							\"concealValueOnDocument\": \"false\"," +
            //"							\"disableAutoSize\": \"false\"," +
            //"							\"maxLength\": \"4\"," +
            //"							\"tabLabel\": \"MEMBER_SUFFIX\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"535\"," +
            //"							\"yPosition\": \"174\"," +
            //"							\"width\": \"30\"," +
            //"							\"height\": \"11\"," +
            //"							\"tabId\": \"e94b8259-a4b5-4997-8b84-b0791884ec05\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"text\"" +
            //"						}," +
            //"						{" +
            //"							\"validationPattern\": \"\"," +
            //"							\"validationMessage\": \"\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"requireAll\": \"false\"," +
            //"							\"value\": \"\"," +
            //"							\"required\": \"false\"," +
            //"							\"locked\": \"false\"," +
            //"							\"concealValueOnDocument\": \"false\"," +
            //"							\"disableAutoSize\": \"false\"," +
            //"							\"maxLength\": \"150\"," +
            //"							\"tabLabel\": \"MEMBER_REP_RELATIONSHIP\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"4\"," +
            //"							\"xPosition\": \"333\"," +
            //"							\"yPosition\": \"243\"," +
            //"							\"width\": \"264\"," +
            //"							\"height\": \"165\"," +
            //"							\"tabId\": \"e812d82f-9a43-477e-8980-4161aaeaf4ba\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"text\"" +
            //"						}," +
            //"						{" +
            //"							\"validationPattern\": \"\"," +
            //"							\"validationMessage\": \"\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"requireAll\": \"false\"," +
            //"							\"name\": \"For non U.S. Citizens. \nU.S. Citizens, enter 'N/A'.\"," +
            //"							\"value\": \"N/A\"," +
            //"							\"originalValue\": \"N/A\"," +
            //"							\"required\": \"true\"," +
            //"							\"locked\": \"false\"," +
            //"							\"concealValueOnDocument\": \"false\"," +
            //"							\"disableAutoSize\": \"false\"," +
            //"							\"maxLength\": \"11\"," +
            //"							\"tabLabel\": \"MEMBER_FIN\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"306\"," +
            //"							\"yPosition\": \"312\"," +
            //"							\"width\": \"240\"," +
            //"							\"height\": \"22\"," +
            //"							\"tabId\": \"99e56b18-00e7-4b13-ac48-853090d12d77\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"text\"" +
            //"						}" +
            //"					]," +
            //"					\"ssnTabs\": [" +
            //"						{" +
            //"							\"validationPattern\": \"\"," +
            //"							\"validationMessage\": \"\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"requireAll\": \"false\"," +
            //"							\"name\": \"For U.S. Citizens.\"," +
            //"							\"value\": \"123-34-3321\"," +
            //"							\"originalValue\": \"123-34-3321\"," +
            //"							\"required\": \"true\"," +
            //"							\"locked\": \"false\"," +
            //"							\"concealValueOnDocument\": \"false\"," +
            //"							\"disableAutoSize\": \"false\"," +
            //"							\"maxLength\": \"11\"," +
            //"							\"tabLabel\": \"MEMBER_SSN\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"49\"," +
            //"							\"yPosition\": \"312\"," +
            //"							\"width\": \"240\"," +
            //"							\"height\": \"22\"," +
            //"							\"tabId\": \"55a2d338-d5fa-484c-b83e-405e1cf12a5f\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"conditionalParentLabel\": \"MEMBER_FIN\"," +
            //"							\"conditionalParentValue\": \"N/A\"," +
            //"							\"tabType\": \"ssn\"" +
            //"						}" +
            //"					]," +
            //"					\"dateTabs\": [" +
            //"						{" +
            //"							\"validationPattern\": \"^(|by DocuSign)((|0)[1-9]|1[0-2])/((|0)[1-9]|[1-2][0-9]|3[0-1])/[0-9]{4}$\"," +
            //"							\"validationMessage\": \"Enter date with format MM/DD/YYYY \"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"requireAll\": \"false\"," +
            //"							\"value\": \"12/22/2001\"," +
            //"							\"originalValue\": \"12/22/2001\"," +
            //"							\"required\": \"true\"," +
            //"							\"locked\": \"false\"," +
            //"							\"concealValueOnDocument\": \"false\"," +
            //"							\"disableAutoSize\": \"false\"," +
            //"							\"maxLength\": \"4000\"," +
            //"							\"tabLabel\": \"MEMBER_DOB\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"50\"," +
            //"							\"yPosition\": \"359\"," +
            //"							\"width\": \"108\"," +
            //"							\"height\": \"22\"," +
            //"							\"tabId\": \"d2f012e7-83e2-47fb-8dc8-115d6d16c430\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"date\"" +
            //"						}" +
            //"					]," +
            //"					\"checkboxTabs\": [" +
            //"						{" +
            //"							\"name\": \"Choose if No\"," +
            //"							\"tabLabel\": \"MEMBER_CONSENT_NO\"," +
            //"							\"selected\": \"false\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size9\"," +
            //"							\"required\": \"false\"," +
            //"							\"locked\": \"false\"," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"3\"," +
            //"							\"xPosition\": \"39\"," +
            //"							\"yPosition\": \"668\"," +
            //"							\"width\": \"0\"," +
            //"							\"height\": \"0\"," +
            //"							\"tabId\": \"7585a45c-c933-4829-9d0a-c45d54e35930\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"conditionalParentLabel\": \"MEMBER_CONSENT_YES\"," +
            //"							\"conditionalParentValue\": \"off\"," +
            //"							\"tabType\": \"checkbox\"," +
            //"							\"tabGroupLabels\": [" +
            //"								\"Checkbox Group 3c555367-1322-4bb5-ace9-8f4f9727357e\"" +
            //"							]" +
            //"						}," +
            //"						{" +
            //"							\"name\": \"Choose if Yes\"," +
            //"							\"tabLabel\": \"MEMBER_CONSENT_YES\"," +
            //"							\"selected\": \"true\"," +
            //"							\"shared\": \"false\"," +
            //"							\"requireInitialOnSharedChange\": \"false\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size9\"," +
            //"							\"required\": \"false\"," +
            //"							\"locked\": \"false\"," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"3\"," +
            //"							\"xPosition\": \"39\"," +
            //"							\"yPosition\": \"648\"," +
            //"							\"width\": \"0\"," +
            //"							\"height\": \"0\"," +
            //"							\"tabId\": \"c08ad818-90e1-41ec-ae63-634383718b2f\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"checkbox\"," +
            //"							\"tabGroupLabels\": [" +
            //"								\"Checkbox Group 3c555367-1322-4bb5-ace9-8f4f9727357e\"" +
            //"							]" +
            //"						}" +
            //"					]," +
            //"					\"firstNameTabs\": [" +
            //"						{" +
            //"							\"name\": \"FirstName\"," +
            //"							\"value\": \"jj\"," +
            //"							\"tabLabel\": \"MEMBER_FN\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"44\"," +
            //"							\"yPosition\": \"174\"," +
            //"							\"width\": \"68\"," +
            //"							\"height\": \"0\"," +
            //"							\"tabId\": \"3b2d6b78-d3fb-4bc3-ab7f-00ea30be38dd\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"firstname\"" +
            //"						}," +
            //"						{" +
            //"							\"name\": \"FullName\"," +
            //"							\"value\": \"jj\"," +
            //"							\"tabLabel\": \"MEMBER_SIGNATURE_FN\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"4\"," +
            //"							\"xPosition\": \"124\"," +
            //"							\"yPosition\": \"207\"," +
            //"							\"width\": \"68\"," +
            //"							\"height\": \"0\"," +
            //"							\"tabId\": \"b74214cf-0728-4bd9-8882-968c66698fb3\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"firstname\"" +
            //"						}" +
            //"					]," +
            //"					\"lastNameTabs\": [" +
            //"						{" +
            //"							\"name\": \"FullName\"," +
            //"							\"value\": \"ss\"," +
            //"							\"tabLabel\": \"MEMBER_LN\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"2\"," +
            //"							\"xPosition\": \"305\"," +
            //"							\"yPosition\": \"174\"," +
            //"							\"width\": \"68\"," +
            //"							\"height\": \"0\"," +
            //"							\"tabId\": \"ecf2b05c-5704-48d3-a380-12d47d5990e2\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"lastname\"" +
            //"						}," +
            //"						{" +
            //"							\"name\": \"LastName\"," +
            //"							\"value\": \"ss\"," +
            //"							\"tabLabel\": \"MEMBER_SIGNATURE_LN\"," +
            //"							\"font\": \"lucidaconsole\"," +
            //"							\"fontColor\": \"black\"," +
            //"							\"fontSize\": \"size14\"," +
            //"							\"localePolicy\": {}," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"4\"," +
            //"							\"xPosition\": \"341\"," +
            //"							\"yPosition\": \"208\"," +
            //"							\"width\": \"68\"," +
            //"							\"height\": \"0\"," +
            //"							\"tabId\": \"c2543847-b8dd-4b52-9534-6cb4f4914131\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"lastname\"" +
            //"						}" +
            //"					]," +
            //"					\"tabGroups\": [" +
            //"						{" +
            //"							\"groupLabel\": \"Checkbox Group 3c555367-1322-4bb5-ace9-8f4f9727357e\"," +
            //"							\"minimumRequired\": \"1\"," +
            //"							\"maximumAllowed\": \"1\"," +
            //"							\"groupRule\": \"SelectExactly\"," +
            //"							\"tabScope\": \"Document\"," +
            //"							\"documentId\": \"1\"," +
            //"							\"recipientId\": \"1\"," +
            //"							\"pageNumber\": \"1\"," +
            //"							\"xPosition\": \"0\"," +
            //"							\"yPosition\": \"0\"," +
            //"							\"width\": \"0\"," +
            //"							\"height\": \"0\"," +
            //"							\"tabId\": \"e891ec3e-68d5-4fee-a9c7-b53d3b850487\"," +
            //"							\"templateRequired\": \"false\"," +
            //"							\"tabType\": \"tabgroup\"" +
            //"						}" +
            //"					]" +
            //"				}," +
            //"				\"creationReason\": \"sender\"," +
            //"				\"isBulkRecipient\": \"false\"," +
            //"				\"requireUploadSignature\": \"false\"," +
            //"				\"name\": \"jj ss\"," +
            //"				\"firstName\": \"\"," +
            //"				\"lastName\": \"\"," +
            //"				\"email\": \"jshankar.epiq@gmail.com\"," +
            //"				\"recipientId\": \"1\"," +
            //"				\"recipientIdGuid\": \"6faa93b8-a1c8-454b-8651-f0ac5a683b10\"," +
            //"				\"requireIdLookup\": \"false\"," +
            //"				\"userId\": \"6fab6c8d-fbc1-4d60-971b-dac240930f5a\"," +
            //"				\"routingOrder\": \"1\"," +
            //"				\"note\": \"\"," +
            //"				\"roleName\": \"Class Member\"," +
            //"				\"status\": \"completed\"," +
            //"				\"completedCount\": \"1\"," +
            //"				\"signedDateTime\": \"2021-09-27T21:48:51.72Z\"," +
            //"				\"deliveredDateTime\": \"2021-09-27T21:48:30.35Z\"," +
            //"               \"declinedDateTime\": \"2022 - 01 - 25T18: 03:44.173Z\"," +
            //"               \"declinedReason\": \"mylongreasonfordecliningtosign\"," +
            //"				\"deliveryMethod\": \"email\"," +
            //"				\"totalTabCount\": \"19\"," +
            //"				\"recipientType\": \"signer\"" +
            //"			}" +
            //"		]," +
            //"		\"agents\": []," +
            //"		\"editors\": []," +
            //"		\"intermediaries\": []," +
            //"		\"carbonCopies\": []," +
            //"		\"certifiedDeliveries\": []," +
            //"		\"inPersonSigners\": []," +
            //"		\"seals\": []," +
            //"		\"witnesses\": []," +
            //"		\"notaries\": []," +
            //"		\"recipientCount\": \"1\"," +
            //"		\"currentRoutingOrder\": \"1\"" +
            //"	}," +
            //"	\"purgeState\": \"unpurged\"," +
            //"	\"envelopeIdStamping\": \"false\"," +
            //"	\"is21CFRPart11\": \"false\"," +
            //"	\"signerCanSignOnMobile\": \"true\"," +
            //"	\"autoNavigation\": \"true\"," +
            //"	\"isSignatureProviderEnvelope\": \"false\"," +
            //"	\"hasFormDataChanged\": \"true\"," +
            //"	\"allowComments\": \"true\"," +
            //"	\"hasComments\": \"false\"," +
            //"	\"allowViewHistory\": \"true\"," +
            //"	\"envelopeMetadata\": {" +
            //"		\"allowAdvancedCorrect\": \"true\"," +
            //"		\"enableSignWithNotary\": \"false\"," +
            //"		\"allowCorrect\": \"true\"" +
            //"	}," +
            //"	\"anySigner\": null," +
            //"	\"envelopeLocation\": \"current_site\"," +
            //"	\"isDynamicEnvelope\": \"false\"" +
            //"}";

            string jsonPayload = new string(ReadContent("JSONPayload.json"));

            EnvelopeModel env = ECAR.DocuSign.CallBack.Process(jsonPayload);

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(env)); 
            
            Console.ReadKey();
        }

        internal static char[] ReadContent(string fileName)
        {
            char[] buff = null;
            string path = Path.Combine(@"C:/Users/jshankar/source/repos/ConsoleLauncher/ConsoleLauncher/", "Resources", fileName);
            using (StreamReader stream = File.OpenText(path))
            {
                buff = new char[stream.BaseStream.Length];
                stream.Read(buff, 0, (int)stream.BaseStream.Length);
            }
            return buff;
        }

        static void WriteDict(Dictionary<string, string> dict)
        {
            Console.WriteLine("Custom fields");
            foreach (KeyValuePair<string, string> kp in dict)
            {
                Console.WriteLine("Field: {0}; Value: {1}", kp.Key, kp.Value);
            }
            Console.WriteLine("---");
        }
    }
}
