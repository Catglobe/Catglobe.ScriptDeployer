# Catglobe.ScriptDeployer
Easily handle development and deployment of sites that needs to run CgScripts on a Catglobe site

This helper library makes it trivial to run and maintain 3 seperate branches of a site:
* Development
* Staging
* Production

# Installation

```
npm install catglobe.cgscript.runtime
npm install catglobe.cgscript.deployment
```

## Runtime setup

Runtime requires the user to log in to the Catglobe site, and then the server will call the CgScript with the user's credentials.

### Catglobe setup

Adjust the following cgscript with the parentResourceId, clientId, clientSecret and name of the client and the requested scopes for your purpose and execute it on your Catglobe site.
```cgscript
number parentResourceId = 42; //for this library to work, this MUST be a folder
string clientId = "some id, a guid works, but any string is acceptable"; //use your own id -> store this in appsettings.json
bool canKeepSecret = true; //demo is a server app, so we can keep secrets
string clientSecret = "secret";
bool askUserForConsent = false;
string layout = "";
Array RedirectUri = {"https://staging.myapp.com/signin-oidc", "https://localhost:7176/signin-oidc"};
Array PostLogoutRedirectUri = {"https://staging.myapp.com/signout-callback-oidc", "https://localhost:7176/signout-callback-oidc"};
Array scopes = {"email", "profile", "roles", "openid", "offline_access"};
Array optionalscopes = {};
LocalizedString name = new LocalizedString({"da-DK": "Min Demo App", "en-US": "My Demo App"}, "en-US");

OidcAuthenticationFlow_createOrUpdate(parentResourceId, clientId, clientSecret, askUserForConsent, 
	canKeepSecret, layout, RedirectUri, PostLogoutRedirectUri, scopes, optionalscopes, name);
```

Remember to set it up TWICE using 2 different `parentResourceId`, `clientId`!
Once for the production site (where URIs point to production site) and once for the staging and development (where URIs point to both staging and dev).

### asp.net setup

Add the following to the appsettings.json with the scopes you made above and your Catglobe site url.
```json
"CatglobeOidc": {
  "Authority": "https://mysite.catglobe.com/",
  "ClientId": "Production id",
  "ResponseType": "code",
  "DefaultScopes": [ "email", "offline_access", "roles", "and others from above, except profile and openid " ],
  "SaveTokens": true
},
"CatglobeApi": {
  "FolderResourceId": deploymentFolderId,
  "Site": "https://mysite.catglobe.com/"
}
```

and in appsettings.Staging.json:

```json
"CatglobeOidc": {
  "ClientId": "stagingAndDevelopment id",
},
"CatglobeApi": {
  "FolderResourceId": stagingAndDevelopmentFolderId,
}
```

and in appsettings.Development.json:
```json
"CatglobeOidc": {
  "ClientId": "stagingAndDevelopment id",
},
"CatglobeApi": {
  "FolderResourceId": stagingAndDevelopmentFolderId,
}
```

You do NOT want to commit the `ClientSecret` to your source repository, so you should add it to your user secrets or environment variables.

For example you can execute the following in the project folder to add the secrets to the user secrets for development mode:
```cli
dotnet user-secrets set "CatglobeOidc:ClientSecret" "the client secret"
```

and in production/staging, you can set the secrets as environment variables.

```cli
env DOTNET_CatglobeOidc__ClientSecret "the client secret"
```

In your start procedure, add the following:
```csharp
const string SCHEMENAME = "CatglobeOidc"; //must match the section name in appsettings.json

// Add services to the container.
var services = builder.Services;
services.AddAuthentication(SCHEMENAME)
        .AddOpenIdConnect(SCHEMENAME, oidcOptions => {
            builder.Configuration.GetSection(SCHEMENAME).Bind(oidcOptions);
            oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
         })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
services.AddCgScript(builder.Configuration.GetSection("CatglobeApi"), builder.Environment.IsDevelopment());
```

Optionally, setup refresh-token refreshing as part of the cookie handling:
```csharp
services.AddSingleton<CookieOidcRefresher>();
services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme).Configure<CookieOidcRefresher>((cookieOptions, refresher) => {
   cookieOptions.Events.OnValidatePrincipal = context => refresher.ValidateOrRefreshCookieAsync(context, SCHEMENAME);
});
```
You can find the CookieOidcRefresher [here](https://github.com/dotnet/blazor-samples/blob/main/9.0/BlazorWebAppOidc/BlazorWebAppOidc/CookieOidcRefresher.cs).

## Deployment

Deployment requires the a server side app to log in to the Catglobe site, and then the app will sync the scripts with the Catglobe site.

This app does NOT need to be a asp.net app, it can be a console app. e.g. if you have a db migration pre-deployment app.

### Catglobe setup

Adjust the following cgscript with the impersonationResourceId, parentResourceId, clientId, clientSecret and name of the client for your purpose and execute it on your Catglobe site.
You should not adjust scope for this.
```cgscript
number parentResourceId = 42;
string clientId = "DA431000-F318-4C55-9458-96A5D659866F"; //use your own id
string clientSecret = "verysecret";
number impersonationResourceId = User_getCurrentUser().ResourceId;
Array scopes = {"scriptdeployment:w"};
LocalizedString name = new LocalizedString({"da-DK": "Min Demo App", "en-US": "My Demo App"}, "en-US");
OidcServer2ServerClient_createOrUpdate(parentResourceId, clientId, clientSecret, impersonationResourceId, scopes, name);
```

Remember to set it up TWICE using 2 different `parentResourceId` and `ClientId`! Once for the production site and once for the staging site.

### App setup

Edit deployment environment in your hosting environment for both your staging and production site (remember to use 2 different sets of setup) to include:
```json
env DOTNET_CatglobeDeployment__ClientSecret "the client secret"
env DOTNET_CatglobeDeployment__ClientId "the client id"
env DOTNET_CatglobeDeployment__FolderResourceId "the parentResourceId"
```
and edit your appsettings.json for your deployment project to include this:
```json
"CatglobeDeployment": {
  "Authority": "https://mysite.catglobe.com/",
  "ScriptFolder": "./CgScript"
}
```

You do NOT want to commit the `ClientSecret` to your source repository, so you should add it to your user secrets or environment variables.

In your start procedure, add the following:
```csharp
builder.Services.AddCgScriptDeployment(builder.Configuration.GetSection("CatglobeDeployment"));
```

and when suitable for your app, call the following:
```csharp
if (!app.Environment.IsDevelopment())
   await app.Services.GetRequiredService<IDeployer>().Sync(app.Environment.EnvironmentName, default);
```

# Usage of the library

## Development

Development takes place on a developers personal device, which means that the developer can run the site locally and test it before deploying it to the staging server.

At this stage the scripts are NOT synced to the server, but are instead dynamically executed on the server.

The authentication model is therefore that the developer logs into the using his own personal account. This account needs to have the questionnaire script dynamic execution access (plus any access required by the script).

All scripts are executed as the developer account and impersonation or public scripts are not supported!

## Staging and Deployment

Setup `deployment` and sync your scripts to the Catglobe site.

# FAQ

## File name mapping to security

It is possible to specify which user a script runs under and if the script needs a user to be logged in.

See the documentation for ScriptFromFileOnDisk for details.

## Can I adapt my scripts to do something special in development mode?

Yes, the scripts runs through a limited preprocessor that recognizes `#if DEVELOPMENT` and `#endif` directives.

```cgscript
return #if Development "" #endif #IF production "Hello, World!" #ENDIF #if STAGING "Hi there" #endif;
```

Would return empty string for development, "Hello, World!" for production and "Hi there" for staging.

The preprocessor is case insensitive, supports multiline and supports the standard `Environment.EnvironmentName` values.

## Development mode impersonation and public scripts

During development all scripts are executed as the developer account, therefore impersonation or public scripts are not supported!

## You get a 404 on first deployment?

`parentResourceId`/`FolderResourceId` MUST be a folder.

## Where do I find the scopes that my site supports?

See supported scopes in your Catglobe site `https://mysite.catglobe.com/.well-known/openid-configuration` under `scopes_supported`.

## Can I use AOT compilation for my C# with this library?

Yes
