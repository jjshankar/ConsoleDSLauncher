using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ECAR.Framework.AzureStaticContent;
using Newtonsoft.Json;

namespace ConsoleLauncher
{
    class AzureStorageTester
    {
        public static void Run(string[] args)
        {
            string sProjName = "";
            string sFolder = "";
            bool bRecurse = false;

            DocumentStore ds = null;
            List<Document> dl = null;

            do
            {
                try
                {
                    Console.Write("[DocumentObjectReturn] Enter project name (top level container) - leave blank to exit: ");
                    sProjName = Console.ReadLine();

                    if (string.IsNullOrEmpty(sProjName))
                        break;

                    Console.Write("Enter folder name (top level): ");
                    sFolder = Console.ReadLine();

                    Console.Write("Recurse? (Y/N): ");
                    string s = Console.ReadLine();
                    bRecurse = (s == "y" || s == "Y");

                    Console.Write("Async? (Y/N): ");
                    string async = Console.ReadLine();
                    Console.WriteLine();

                    ds = new DocumentStore(sProjName);

                    #region DocumentObjectReturn

                    if (async.ToLower() == "y")
                        dl = ds.GetDocumentListAsync(sFolder, bRecurse).Result;
                    else
                        dl = ds.GetDocumentList(sFolder, bRecurse);

                    if (dl == null)
                        Console.WriteLine("No files exist!");
                    else
                    {
                        foreach (Document doc in dl)
                        {
                            Console.WriteLine("{0}: {1} -- {2}", doc.DocName, doc.DocUri, doc.DocVirtualPath);
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.InnerException != null)
                        Console.WriteLine(ex.InnerException.Message);
                }

                Console.WriteLine("---- x ----");
                Console.WriteLine();
            }
            while (!string.IsNullOrEmpty(sProjName));

            #region Anonymous_Access
            //Console.Write("Test anonymous access? (Y/N): ");
            //string a = Console.ReadLine();
            //if (a == "y" || a == "Y")
            //{
            //    Console.WriteLine();
            //    Console.WriteLine("Testing Anonymous access...");
            //    try
            //    {
            //        ds = new DocumentStore("");
            //        dl = ds.AccessTopLevelFolders();
            //        if (dl == null)
            //            Console.WriteLine("Cannot access top level folder!");
            //        else
            //        {
            //            foreach (Document doc in dl)
            //            {
            //                Console.WriteLine("{0}: {1} -- {2}", doc.DocName, doc.DocUri, doc.DocVirtualPath);
            //            }
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message);
            //        if (ex.InnerException != null)
            //            Console.WriteLine(ex.InnerException.Message);
            //    }
            //}
            #endregion
        }

        public static void RunJSON(string[] args)
        {
            string sProjName = "";
            string sFolder = "";
            bool bRecurse = false;

            DocumentStore ds = null;

            const string CONNECTIONSTRING = "DefaultEndpointsProtocol=https;AccountName=ecarwebcu;AccountKey=sbRkv+Vf66LWHv/TXa/5tNFHwZ+GdrkCPv3iQ4oX+kZHHayNdVf2dAQj9UrBGkkWpTrAsEeiTj2UTfzhvLYknA==;EndpointSuffix=core.windows.net";
            const string CONTAINERNAME = "$web";
            const string BASEURI = "str-cu.ecar.epiqglobal.com";

            do
            {
                try
                {
                    Console.Write("[JSONReturn] Enter project name (top level container) - leave blank to exit: ");
                    sProjName = Console.ReadLine();

                    if (string.IsNullOrEmpty(sProjName))
                        break;

                    Console.Write("Enter folder name (top level): ");
                    sFolder = Console.ReadLine();

                    Console.Write("Recurse? (Y/N): ");
                    string s = Console.ReadLine();
                    bRecurse = (s == "y" || s == "Y");

                    Console.Write("Async? (Y/N): ");
                    string async = Console.ReadLine();
                    Console.WriteLine();

                    ds = new DocumentStore(CONNECTIONSTRING, BASEURI, CONTAINERNAME, sProjName);

                    #region JSONReturn

                    // Call JSON method
                    if (async.ToLower() == "y")
                        Console.WriteLine(ds.GetDocumentListJSONAsync(sFolder, bRecurse).Result);
                    else
                        Console.WriteLine(ds.GetDocumentListJSON(sFolder, bRecurse));

                    #endregion                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    if (ex.InnerException != null)
                        Console.WriteLine(ex.InnerException.Message);
                }

                Console.WriteLine("---- x ----");
                Console.WriteLine();
            }
            while (!string.IsNullOrEmpty(sProjName));
        }
    }
}
