using System.Security.Claims;
using Microsoft.AspNetCore.Localization;

namespace BlazorWebApp.DemoUsage;

/// <summary>
/// Pull the "locale" and "culture" claims from the user and use that as the uiCulture and culture.
/// If either is missing, the other is used as a fallback. If both are missing, or the locale is not in the list of known cultures, it does nothing.
/// <example><code>
/// host.UseRequestLocalization(o =&gt; {
///   var cultures = ...;
///   o.AddSupportedCultures(cultures)
///   .AddSupportedUICultures(cultures)
///   .SetDefaultCulture(cultures[0]);
/// //insert before the final default provider (the AcceptLanguageHeaderRequestCultureProvider)
/// o.RequestCultureProviders.Insert(o.RequestCultureProviders.Count - 1, new OidcClaimsCultureProvider {Options = o});
/// });
/// </code></example>
/// </summary>
public class OidcClaimsCultureProvider : RequestCultureProvider
{
   ///<inheritdoc/>
   public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext) => Task.FromResult(GetCultureFromClaims(httpContext));

   private static ProviderCultureResult? GetCultureFromClaims(HttpContext ctx)
   {
      var userCulture   = ctx.User.FindFirstValue("culture");
      var userUiCulture = ctx.User.FindFirstValue("locale") ?? userCulture;
      if (userUiCulture == null) goto noneFound;

      return new(userCulture ?? userUiCulture, userUiCulture);
      noneFound:
      return null;
   }
}
