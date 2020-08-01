using System;
using System.Collections.Generic;
using System.Text;
using ECAR.DocuSign.Models;
using DocuSign.eSign.Client;
using DocuSign.eSign.Client.Auth;
using static DocuSign.eSign.Client.Auth.OAuth.UserInfo;
using System.Linq;
using ECAR.DocuSign.Properties;

namespace ECAR.DocuSign.Security
{
    internal static class Authenticate
    {
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

                // Write JWT token, server path & expiry back to config 
                DocuSignConfig.AccessToken = accessToken;
                DocuSignConfig.BasePath = acct.BaseUri;
                DocuSignConfig.AccessTokenExpiration = DateTime.Now.AddSeconds(authToken.expires_in.Value);                
            }

            // Return refreshed/existing token
            return accessToken;
        }
    }
}
