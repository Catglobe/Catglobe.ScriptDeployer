using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlazorWebApp.CgScript;
using Catglobe.CgScript.Common;
using Catglobe.CgScript.Runtime;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Yarp.ReverseProxy.Transforms;

namespace BlazorWebApp.DemoUsage;

internal class SetupRuntime
{
   private const string SCHEMENAME = "CatglobeOidc"; //must match the section name in appsettings.json

   public static void Configure(WebApplicationBuilder builder)
   {
      // Add services to the container.
      builder.Services.AddAuthentication(SCHEMENAME)
          .AddOpenIdConnect(SCHEMENAME, oidcOptions => {
             // ........................................................................
             // The OIDC handler must use a sign-in scheme capable of persisting 
             // user credentials across requests.
             oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
          })
          .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

      builder.Services.AddSingleton<CookieOidcRefresher>();
      // attaches a cookie OnValidatePrincipal callback to get
      // a new access token when the current one expires, and reissue a cookie with the
      // new access token saved inside. If the refresh fails, the user will be signed
      // out. OIDC connect options are set for saving tokens and the offline access
      // scope.
      builder.Services.AddOptions<CookieAuthenticationOptions>(CookieAuthenticationDefaults.AuthenticationScheme).Configure<CookieOidcRefresher>((cookieOptions, refresher) => {
         cookieOptions.Events.OnValidatePrincipal = context => refresher.ValidateOrRefreshCookieAsync(context, SCHEMENAME);
      });

      builder.Services.AddAuthorization();

      builder.Services.AddHttpForwarder();

      builder.Services.AddCgScript(builder.Configuration.GetSection("CatglobeApi"));
      
      //and this is custom to this specific example... The rest above can be reused for pretty much any site
      builder.Services.AddSingleton<IWeatherForecaster, ServerWeatherForecaster>();
      builder.Services.AddSingleton<IPublicWeatherForecaster, ServerPublicWeatherForecaster>();
   }

   public static void Use(WebApplication app)
   {
      app.MapGroup("/authentication").MapLoginAndLogout();

      //Add this, if you need the browser (blazor wasm or javascript) to be able to call CgScript
      //add     <PackageReference Include="Microsoft.Extensions.ServiceDiscovery.Yarp" Version="9.0.0" />
      var site = app.Services.GetRequiredService<IOptions<CgScriptOptions>>().Value.Site;
      app.MapForwarder("/api/cgscript", site+"api/cgscript", transformBuilder =>
      {
         transformBuilder.AddRequestTransform(async transformContext =>
         {
            var accessToken = await transformContext.HttpContext.GetTokenAsync("access_token");
            transformContext.ProxyRequest.Headers.Authorization = new("Bearer", accessToken);
         });
      }).RequireAuthorization();
   }
}

