//using Microsoft.Azure.Storage;
//using Microsoft.Azure.Storage.Blob;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Reflection;
//using System.Runtime.CompilerServices;
//using System.Threading.Tasks;
//using System.Web;

//namespace ConsoleLauncher
//{
//    public class DocumentStore
//    {
//        private const string _connectionString = "DefaultEndpointsProtocol=https;AccountName=ecarwebcu;AccountKey=sbRkv+Vf66LWHv/TXa/5tNFHwZ+GdrkCPv3iQ4oX+kZHHayNdVf2dAQj9UrBGkkWpTrAsEeiTj2UTfzhvLYknA==;EndpointSuffix=core.windows.net";

//        // Common Domain Name URI e.g.: "https://str-cu.ecar.epiqglobal.com"
//        private Uri _baseUri;

//        // Raw URI to the Azure storage container: https://ecarwebcu.blob.core.windows.net
//        private Uri _rawUri;

//        private CloudStorageAccount _storageAccount;
//        private CloudBlobContainer _blobContainer;
//        private string _projectName = "";
//        private string _containerName = "$web";
//        private bool _ready = false;

//        // public bool Exists { get { return _ready; }  }


//        public DocumentStore(string ProjectName) 
//        {
//            _projectName = ProjectName;

//            _storageAccount = CloudStorageAccount.Parse(_connectionString);

//            // Configuration dllConfig = ConfigurationManager.OpenExeConfiguration(Assembly.GetExecutingAssembly().CodeBase);

//            // Each project must have its own root Container under the Azure storage account
//            // string uri = dllConfig.AppSettings.Settings["AzureStorageBaseURI"].Value;
//            string uri = "https://str-cu.ecar.epiqglobal.com";
//            _baseUri = new Uri(uri);

//            // uri = dllConfig.AppSettings.Settings["AzureStorageRawURI"].Value;
//            uri = "https://ecarwebcu.blob.core.windows.net";
//            _rawUri = new Uri(uri + (uri.EndsWith("/") ? "" : "/") + _containerName); // ProjectName);
//        }

//        private void ValidateConnection()
//        {
//            // Catches StorageException and throws it back out
//            _ready = false;
//            try
//            {
//                //_blobContainer = new CloudBlobContainer(_baseUri);
//                //try
//                //{
//                //    _ready = _blobContainer.Exists();
//                //}
//                //catch { }

//                if (!_ready )
//                {
//                    // Check raw access
//                    CloudBlobClient blobClient = _storageAccount.CreateCloudBlobClient();
//                    _blobContainer = blobClient.GetContainerReference(_containerName);
//                    _ready = _blobContainer.Exists();
//                }
//                return;
//            }
//            catch(StorageException ex)
//            {
//                throw (new Exception(ex.Message, ex.InnerException));
//            }
//        }

//        public List<Document> GetDocumentList(string FolderName, bool Recursive = false)
//        {
//            try
//            {
//                // Initiate connection
//                if (!_ready)
//                    ValidateConnection();

//                List<Document> retList = null;

//                // Check for input
//                FolderName = string.IsNullOrEmpty(FolderName) ? _projectName : 
//                    (FolderName.StartsWith(_projectName) ? FolderName : _projectName + "/" + FolderName);

//                // Enumerate
//                if (_ready)
//                {
//                    CloudBlobDirectory rootDir = _blobContainer.GetDirectoryReference(FolderName);
//                    IEnumerable<IListBlobItem> files = rootDir.ListBlobs();

//                    if (files.Count() > 0)
//                    {
//                        retList = new List<Document>();

//                        foreach (IListBlobItem item in files)
//                        {
//                            if (item is CloudBlob)
//                            {
//                                CloudBlob file = (CloudBlob)item;
//                                string fileName = "";
//                                int i = 0;

//                                // Get the last segment (it has the file name)
//                                fileName = file.Uri.Segments[file.Uri.Segments.Length - 1];

//                                Document doc = new Document
//                                {
//                                    DocId = (++i).ToString(),
//                                    DocName = HttpUtility.UrlDecode(fileName),
//                                    DocUri = _baseUri.AbsoluteUri + file.Name,                      // file.Uri.AbsoluteUri,
//                                    DocVirtualPath = file.Uri.LocalPath.Replace(fileName, "").Replace("/"+_containerName, ""),
//                                    DocType = "application/" + fileName.Substring(fileName.LastIndexOf(".") + 1)
//                                };
//                                retList.Add(doc);

//                            }
//                            if (item is CloudBlobDirectory)
//                            {
//                                CloudBlobDirectory dir = (CloudBlobDirectory)item;

//                                // recurse
//                                if (Recursive)
//                                    retList.AddRange(GetDocumentList(dir.Prefix, Recursive));
//                            }
//                        }
//                    }

//                }
//                return retList;
//            }
//            catch (StorageException ex)
//            {
//                throw (new Exception(ex.Message, ex.InnerException));
//            }
//        }

//        public async Task<List<Document>> GetDocumentListAsync(string FolderName, bool Recursive = false)
//        {
//            try
//            {
//                // Initiate connection
//                if (!_ready)
//                    ValidateConnection();

//                List<Document> retList = null;
//                BlobContinuationToken continuationToken = null;

//                // Check for input
//                FolderName = string.IsNullOrEmpty(FolderName) ? _projectName :
//                    (FolderName.StartsWith(_projectName) ? FolderName : _projectName + "/" + FolderName);

//                if (_ready)
//                {
//                    BlobResultSegment resultSegment = await _blobContainer.ListBlobsSegmentedAsync(FolderName, false, BlobListingDetails.Metadata, null, continuationToken, null, null);

//                    if (resultSegment.Results.Count() > 0)
//                    {
//                        retList = new List<Document>();

//                        foreach (IListBlobItem item in resultSegment.Results)
//                        {
//                            if (item is CloudBlob)
//                            {
//                                CloudBlob file = (CloudBlob)item;
//                                string fileName = "";
//                                int i = 0;

//                                // Get the last segment (it has the file name)
//                                fileName = file.Uri.Segments[file.Uri.Segments.Length - 1];

//                                Document doc = new Document
//                                {
//                                    DocId = (++i).ToString(),
//                                    DocName = HttpUtility.UrlDecode(fileName),
//                                    DocUri = file.Uri.AbsoluteUri,
//                                    // DocVirtualPath = ,
//                                    DocType = "application/" + fileName.Substring(fileName.LastIndexOf(".") + 1)
//                                };
//                                retList.Add(doc);

//                            }
//                            if (item is CloudBlobDirectory)
//                            {
//                                CloudBlobDirectory dir = (CloudBlobDirectory)item;

//                                // recurse
//                                if (Recursive)
//                                    retList.AddRange(await GetDocumentListAsync(dir.Prefix, Recursive));
//                            }
//                        }
//                    }

//                }
//                return retList;
//            }
//            catch(StorageException ex)
//            {
//                throw (new Exception(ex.Message, ex.InnerException));
//            }
//        }


//        public async Task<List<Document>> ListBlobsHierarchicalListingAsync(string prefix)
//        {
//            // List blobs in segments.
//            Console.WriteLine("List blobs (hierarchical listing):");
//            Console.WriteLine();

//            // Enumerate the result segment returned.
//            BlobContinuationToken continuationToken = null;
//            BlobResultSegment resultSegment = null;

//            List<Document> retList = null;

//            if (!_ready)
//                ValidateConnection();

//            if (_ready)
//            {
//                try
//                {
//                    // Call ListBlobsSegmentedAsync recursively and enumerate the result segment returned, while the continuation token is non-null.
//                    // When the continuation token is null, the last segment has been returned and execution can exit the loop.
//                    // Note that blob snapshots cannot be listed in a hierarchical listing operation.
//                    do
//                    {
//                        resultSegment = await _blobContainer.ListBlobsSegmentedAsync(prefix, false, BlobListingDetails.Metadata, null, null, null, null);

//                        if (resultSegment.Results.Count() > 0)
//                        {
//                            retList = new List<Document>();

//                            foreach (var blobItem in resultSegment.Results)
//                            {
//                                Console.WriteLine("************************************");
//                                Console.WriteLine(blobItem.Uri);

//                                // A hierarchical listing returns both virtual directories and blobs.
//                                // Call recursively with the virtual directory prefix to enumerate the contents of each virtual directory.
//                                if (blobItem is CloudBlobDirectory)
//                                {
//                                    // PrintVirtualDirectoryProperties((CloudBlobDirectory)blobItem);
//                                    CloudBlobDirectory dir = blobItem as CloudBlobDirectory;
//                                    retList.AddRange(await ListBlobsHierarchicalListingAsync(dir.Prefix));
//                                }
//                                else
//                                {
//                                    CloudBlob file = (CloudBlob)blobItem;
//                                    string fileName = "";
//                                    int i = 0;

//                                    // Get the last segment (it has the file name)
//                                    fileName = file.Uri.Segments[file.Uri.Segments.Length - 1];

//                                    Document doc = new Document
//                                    {
//                                        DocId = (++i).ToString(),
//                                        DocName = HttpUtility.UrlDecode(fileName),
//                                        DocUri = file.Uri.AbsoluteUri,
//                                        // DocVirtualPath = ,
//                                        DocType = "application/" + fileName.Substring(fileName.LastIndexOf(".") + 1)
//                                    };
//                                    retList.Add(doc);
//                                }
//                            }
//                        }
//                        Console.WriteLine();

//                        // Get the continuation token, if there are additional segments of results.
//                        continuationToken = resultSegment.ContinuationToken;

//                    } while (continuationToken != null);
//                }
//                catch (StorageException e)
//                {
//                    Console.WriteLine(e.Message);
//                    Console.ReadLine();
//                    throw;
//                }
//            }
//            return retList;
//        }


//        public List<Document> AccessTopLevelFolders(string FolderName = "", bool Recursive = false)
//        {
//            try
//            {
//                // Initiate connection
//                CloudBlobContainer blobContainer = new CloudBlobContainer(_baseUri);

//                List<Document> retList = null;

//                // Enumerate
//                CloudBlobDirectory rootDir = blobContainer.GetDirectoryReference(FolderName);
//                IEnumerable<IListBlobItem> files = rootDir.ListBlobs();

//                if (files.Count() > 0)
//                {
//                    retList = new List<Document>();

//                    foreach (IListBlobItem item in files)
//                    {
//                        if (item is CloudBlob)
//                        {
//                            CloudBlob file = (CloudBlob)item;
//                            string fileName = "";
//                            int i = 0;

//                            // Get the last segment (it has the file name)
//                            fileName = file.Uri.Segments[file.Uri.Segments.Length - 1];

//                            Document doc = new Document
//                            {
//                                DocId = (++i).ToString(),
//                                DocName = HttpUtility.UrlDecode(fileName),
//                                DocUri = file.Uri.AbsoluteUri,
//                                DocVirtualPath = file.Uri.LocalPath.Replace(fileName, ""),
//                                DocType = "application/" + fileName.Substring(fileName.LastIndexOf(".") + 1)
//                            };
//                            retList.Add(doc);

//                        }
//                        if (item is CloudBlobDirectory)
//                        {
//                            CloudBlobDirectory dir = (CloudBlobDirectory)item;

//                            // recurse
//                            if (Recursive)
//                                retList.AddRange(AccessTopLevelFolders(dir.Prefix, Recursive));
//                        }
//                    }
//                }
                
//                return retList;
//            }
//            catch (StorageException ex)
//            {
//                throw (new Exception(ex.Message, ex.InnerException));
//            }
//        }

//    }
//}