// using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Configure log4net using the .config file
[assembly: log4net.Config.XmlConfigurator(Watch = true)]
// This will cause log4net to look for a configuration file
// called ConsoleApp.exe.config in the application base
// directory (i.e. the directory containing ConsoleApp.exe)

namespace ConsoleLauncher
{
    class Program
    {
        const string MESSAGE = "\n********** SELECT OPTION ********** \n" +
                               "    1.  Single Template\n" +
                               "    2.  Single Template with Preview\n" +
                               "    3.  Packet Template\n" +
                               "    4.  Bulk Single Template\n" +
                               "    5.  Bulk Packet Template\n" +
                               "    6.  QUICKRUN Bulk Single Template\n" +
                               "" +
                               "    b.  Get Batch envelopes data\n" +
                               "    e.  Get Envelope Data\n" +
                               "    f.  Get Envelope Fields\n" +
                               // "    e.  Get Envelope Data\n" +
                               "*********************************** \n" +
                               "    t.  Test batch ID\n" +
                               "    j.  Test web callback with JSON file\n" +
                               "*********************************** \n";
        static void Main(string[] args)
        {
            DocuSignRun(args);

            // DateTimeTesting();

            // AzureStorageTester.Run(args);
            // AzureStorageTester.RunJSON(args);

            // GoogleApiHelper.Run(args);

            // JSONTester.RunJSONTester();

        }

        public static void DocuSignRun(string[] args)
        {
            do
            {
                Console.WriteLine(MESSAGE);
                Console.Write("Enter option...");

                char option = Console.ReadKey().KeyChar;
                string batchId = "";
                Console.WriteLine();

                switch (option)
                {

                    case '1':
                        ECARDocuSignTester.RunSingleTemplate(args);
                        break;
                    case '2':
                        ECARDocuSignTester.RunSingleTemplatePreview(args);
                        break;
                    case '3':
                        ECARDocuSignTester.RunPacketSendTemplate(args);
                        break;
                    case '4':
                        batchId = ECARDocuSignTester.RunBulkSingleTemplate(args);
                        break;
                    case '5':
                        batchId = ECARDocuSignTester.RunBulkPacketTemplate(args);
                        break;
                    case '6':
                        batchId = ECARDocuSignTester.QuickRunBulkSingleTemplate(args);
                        break;
                    // test
                    case 't':
                    case 'T':
                        Console.Write("Enter batch ID (leave blank to exit): ");
                        batchId = Console.ReadLine();
                        break;
                    // test
                    case 'b':
                    case 'B':
                        ECARDocuSignTester.GetBatchEnvelopes();
                        batchId = "";
                        break;
                    case 'e':
                    case 'E':
                        ECARDocuSignTester.GetEnvelopeData();
                        break;
                    case 'f':
                    case 'F':
                        ECARDocuSignTester.GetFieldData();
                        break;
                    case 'j':
                    case 'J':
                        Console.Write("UNSUPPORTED!");
                        break;

                    default:
                        batchId = "";
                        break;
                }

                if (!string.IsNullOrEmpty(batchId))
                {
                    Console.Write("Press any key to get status of the batch {0}", batchId);
                    Console.Read();

                    Console.Write("Waiting for ");
                    string msg = "";
                    int i = 10;
                    do
                    {
                        msg = i.ToString() + " seconds before checking status....";
                        Console.Write(msg);
                        System.Threading.Thread.Sleep(1000);
                        foreach (char c in msg.ToCharArray())
                            Console.Write("\b");
                    }
                    while (--i > 0);
                    Console.WriteLine(msg + "Done!");
                    ECARDocuSignTester.GetBatchStatus(batchId);
                }

                //string templateName = "NCAA HIPAA Consent Form";
                //ECAR.DocuSign.Models.DocumentModel doc = new ECAR.DocuSign.Models.DocumentModel {                 
                //    DSEmailSubject = "Please dSign this document.",
                //    DSRoleName = "Class Member",
                //    DSTemplateName = templateName
                //};

                //Console.Write("Enter recipient full name (leave blank to exit): ");
                //doc.SignerName = "Jesus Shankar"; // Console.ReadLine();
                //Console.WriteLine(doc.SignerName);
                //if (string.IsNullOrEmpty(doc.SignerName))
                //    return;

                //Console.Write("Enter recipient email address: ");
                //doc.SignerEmail = Console.ReadLine();
                //Console.WriteLine(doc.SignerEmail);

                //Console.Write("Enter recipient ID (any): ");
                //doc.SignerId = Console.ReadLine();

                //// DocuSignTester.SendTemplate(doc);

                //DocuSignTester.SendBulkTemplate(new List<ECAR.DocuSign.Models.DocumentModel> { doc, new ECAR.DocuSign.Models.DocumentModel {
                //        DSRoleName = "Class Member",
                //        SignerName = "Jesus Shankar",
                //        SignerEmail = "jshankar@foo.com",
                //        SignerId = "123321"
                //    }
                //}, templateName);

                Console.WriteLine("Press any key to continue or ESC to exit...");
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                    break;
                else
                    ECARDocuSignTester.AdjustExpiration();
            }
            while (true);
        }

        public static void DateTimeTesting()
        {
            DateTime date = DateTime.Now.AddMinutes(5);
            DateTime expiry = DateTime.Parse("Friday, September 17, 2021 10:03:44 AM");

            // Console.WriteLine(date.ToString("d"));
            Console.WriteLine("Date+5min: " + date.ToString("F", DateTimeFormatInfo.InvariantInfo));
            Console.WriteLine("Expiry: " + expiry.ToString("F", DateTimeFormatInfo.InvariantInfo));
            Console.WriteLine("Is expiry < date+5 ?: {0}", (expiry < date).ToString());

        }


    }
}
