/**
 * Environment configuration for production.
 * Values should be replaced during CI/CD deployment.
 */
export const environment = {
  production: true,
  
  msalConfig: {
    auth: {
      clientId: 'YOUR_SPA_CLIENT_ID',
      authority: 'https://login.microsoftonline.com/common',
      redirectUri: 'https://your-production-domain.com',
      postLogoutRedirectUri: 'https://your-production-domain.com'
    },
    cache: {
      cacheLocation: 'localStorage',
      storeAuthStateInCookie: false
    }
  },
  
  apiConfig: {
    scopes: ['api://YOUR_API_CLIENT_ID/access_as_user'],
    uri: 'https://your-api-domain.com/api'
  }
};
