/**
 * Environment configuration for development.
 * Replace placeholders with your Azure AD app registration values.
 */
export const environment = {
  production: false,
  
  // Azure AD / Microsoft Entra ID Configuration
  msalConfig: {
    auth: {
      // Your SPA app registration Client ID
      clientId: 'de265c95-9f62-4465-8caf-6a4cd974830c',
      // Use 'common' for multi-tenant, or specific tenant ID for single-tenant
      authority: 'https://login.microsoftonline.com/common',
      // Must match redirect URI configured in Azure AD
      redirectUri: 'http://localhost:4200',
      postLogoutRedirectUri: 'http://localhost:4200'
    },
    cache: {
      cacheLocation: 'localStorage',
      storeAuthStateInCookie: false
    }
  },
  
  // Protected resource scopes
  apiConfig: {
    // Your API app registration scope
    scopes: ['api://171d9cb6-72eb-4a41-b83c-14d4807c66c5/access_as_user'],
    uri: 'https://localhost:7001/api'
  }
};
