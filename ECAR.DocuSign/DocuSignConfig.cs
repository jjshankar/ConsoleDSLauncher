using System;
using System.Collections.Generic;
using System.Text;

namespace ECAR.DocuSign
{
    /// <summary>
    /// Configuration for ECAR.DocuSign access.
    /// </summary>
    public static class DocuSignConfig
    {
        /// <summary>
        /// Your DocuSign organization number (also known as Account ID).
        /// </summary>
        public static string AccountID { get; set; }

        /// <summary>
        /// Your DocuSign integration key (also known as Client ID).
        /// </summary>
        public static string ClientID { get; set; }

        /// <summary>
        /// DocuSign User ID to send as (also known as API Username).
        /// </summary>
        public static string UserGUID { get; set; }

        /// <summary>
        /// DocuSign server to connect to.
        /// Use: 'account-d.docusign.com' for Development.
        /// Use: 'account.docusign.com' for Production.
        /// </summary>
        public static string AuthServer { get; set; }

        /// <summary>
        /// DocuSign RSA Security key.  
        /// Pass contents of the key file as UTF8 byte array (not the file name).
        /// </summary>
        public static byte[] RSAKey { get; set; }

        /// <summary>
        /// DocuSign Access Token (read only).
        /// </summary>
        public static string AccessToken { get; internal set; }

        /// <summary>
        /// DocuSign Access Token Expiration Date (read only).
        /// </summary>
        public static DateTime AccessTokenExpiration { get; internal set; }

        /// <summary>
        /// DocuSign server path to use (internal).
        /// </summary>
        internal static string BasePath { get; set; }

        /// <summary>
        /// DocuSign service endpoint (internal).
        /// </summary>
        internal static string APISuffix
        {
            get { return "/restapi";  }
        }

        /// <summary>
        /// Configuration readiness indicator.
        /// </summary>
        public static bool Ready
        {
            get
            {
                // Validate if config is ready
                if (string.IsNullOrEmpty(AccountID) || string.IsNullOrEmpty(ClientID) || string.IsNullOrEmpty(UserGUID) ||
                    string.IsNullOrEmpty(AuthServer) || string.IsNullOrEmpty(RSAKey.ToString()))
                    return false;

                return true;
            }
        }
    }
}
