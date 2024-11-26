# Catglobe.ScriptDeployer
Easily handle development and deployment of sites that needs to run CgScripts on a catglobe site

This helper library makes it trivial to run and maintain 3 seperate branches of a site:
* Development
* Staging
* Production

# Installation

```
npm install catglobe.cgscript.deployment
npm install catglobe.cgscript.runtime
```

# Usage of the library

## Development

Development takes place on a developers personal device, which means that the developer can run the site locally and test it before deploying it to the staging server.

The authentication model is therefore that the developer logs into the using his own personal account. This account needs to have enough permission to set impersonation as configured on the scripts.

```cgscript
number parentResourceId = 42;
string clientId = "13BAC6C1-8DEC-46E2-B378-90E0325F8132"; //use your own id -> store this in appsettings.Development.json
bool canKeepSecret = true; //demo is a server app, so we can keep secrets
string clientSecret = "secret";
bool askUserForConsent = false;
string layout = "";
Array RedirectUri = {"https://localhost:7176/authentication/login-callback"};
Array PostLogoutRedirectUri = {"https://localhost:7176/authentication/logout-callback"};
Array scopes = {"email", "profile", "roles", "openid", "offline_access"};
Array optionalscopes = {};
LocalizedString name = new LocalizedString({"da-DK": "Min Demo App", "en-US": "My Demo App"}, "en-US");

OidcAuthenticationFlow_createOrUpdate(parentResourceId, clientId, clientSecret, askUserForConsent, 
	canKeepSecret, layout, RedirectUri, PostLogoutRedirectUri, scopes, optionalscopes, name);
```

## Staging and Deployment

```cgscript
number parentResourceId = 42;
string clientId = "DA431000-F318-4C55-9458-96A5D659866F"; //use your own id
string clientSecret = "verysecret";
number impersonationResourceId = User_getCurrentUser().ResourceId;
Array scopes = {"scriptdeployment:w"};
LocalizedString name = new LocalizedString({"da-DK": "Min Demo App", "en-US": "My Demo App"}, "en-US");
OidcServer2ServerClient_createOrUpdate(parentResourceId, clientId, clientSecret, impersonationResourceId, scopes, name);
```

# How it works?

* get existing map
* get list of current scripts
* find new and pre-deploy (create resources and get id)
* find deleted and delete
* generate all scripts final form
* compare sha of each and find changed and update as needed

## File name mapping to security

It is possible to specify which user a script runs under and if the script needs a user to be logged in.

See the documentation for ScriptFromFileOnDisk for details.

