using System;
using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;
using System.Linq;
using ECAR.DocuSign.Properties;

namespace ECAR.DocuSign.Security
{
    internal static class Authenticate
    {
        private static ApiClient __apiClient;
        private static EnvelopesApi __envelopesApi;
        private static TemplatesApi __templatesApi;
        private static BulkEnvelopesApi __bulkEnvelopesApi;

        public static string GetToken()
        {
            if (!DocuSignConfig.Ready)
                throw new Exception(Resources.DSCONFIG_NOT_SET);

            // Read config values 
            string clientId = DocuSignConfig.ClientID;
            string userGuid = DocuSignConfig.UserGUID;
            string authServer = DocuSignConfig.AuthServer;
            byte[] rsaKey = DocuSignConfig.RSAKey;

            // Previous token (if set)
            string accessToken = DocuSignConfig.AccessToken;
            DateTime expiry = DocuSignConfig.AccessTokenExpiration;

            // Check if we need to refresh token
            if (string.IsNullOrEmpty(accessToken) || expiry == null || expiry < DateTime.Now.AddSeconds(5))
            {
                ApiClient apiClient = new ApiClient();

                // Get new token
                OAuth.OAuthToken authToken = apiClient.RequestJWTUserToken(clientId,
                                userGuid,
                                authServer,
                                rsaKey,
                                1);

                accessToken = authToken.access_token;

                // Validate
                apiClient.SetOAuthBasePath(DocuSignConfig.AuthServer);
                OAuth.UserInfo userInfo = apiClient.GetUserInfo(authToken.access_token);
                Account acct = null;

                var accounts = userInfo.Accounts;
                acct = accounts.FirstOrDefault(a => a.IsDefault == "true");

                // Write JWT token, server path & expiry back to in-memory config 
                DocuSignConfig.AccessToken = accessToken;
                DocuSignConfig.BasePath = acct.BaseUri;
                DocuSignConfig.AccessTokenExpiration = DateTime.Now.AddSeconds(authToken.expires_in.Value);                
            }

            // Return refreshed/existing token
            return accessToken;
        }

        public static EnvelopesApi CreateEnvelopesApiClient()
        {
            // Get access token
            string accessToken = GetToken();

            if (__envelopesApi == null)
            {
                if (__apiClient == null)
                {
                    // Create new ApiClient and set config
                    __apiClient = new ApiClient(DocuSignConfig.BasePath + DocuSignConfig.APISuffix);
                    __apiClient.Configuration.AddDefaultHeader("Authorization", "Bearer " + accessToken);
                }

                __envelopesApi = new EnvelopesApi(__apiClient);
            }

            return __envelopesApi;
        }

        public static TemplatesApi CreateTemplateApiClient()
        {
            // Get access token
            string accessToken = GetToken();

            if (__templatesApi == null)
            {
                if (__apiClient == null)
                {
                    // Create new ApiClient and set config
                    __apiClient = new ApiClient(DocuSignConfig.BasePath + DocuSignConfig.APISuffix);
                    __apiClient.Configuration.AddDefaultHeader("Authorization", "Bearer " + accessToken);
                }

                __templatesApi = new TemplatesApi(__apiClient);
            }

            return __templatesApi;
        }

        public static BulkEnvelopesApi CreateBulkEnvelopesApiClient()
        {
            // Get access token
            string accessToken = GetToken();

            if (__bulkEnvelopesApi == null)
            {
                if (__apiClient == null)
                {
                    // Create new ApiClient and set config
                    __apiClient = new ApiClient(DocuSignConfig.BasePath + DocuSignConfig.APISuffix);
                    __apiClient.Configuration.AddDefaultHeader("Authorization", "Bearer " + accessToken);
                }

                __bulkEnvelopesApi = new BulkEnvelopesApi(__apiClient);
            }

            return __bulkEnvelopesApi;
        }

    }
}
