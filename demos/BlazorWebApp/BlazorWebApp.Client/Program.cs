using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization(o=>o.DeserializationCallback = ProcessLanguageAndCultureFromClaims(o.DeserializationCallback));

static Func<AuthenticationStateData?, Task<AuthenticationState>> ProcessLanguageAndCultureFromClaims(Func<AuthenticationStateData?, Task<AuthenticationState>> authenticationStateData) =>
   state => {
      var tsk = authenticationStateData(state);
      if (!tsk.IsCompletedSuccessfully) return tsk;
      var authState = tsk.Result;
      if (authState?.User is not { } user) return tsk;
      var userCulture   = user.FindFirst("culture")?.Value;
      //Console.WriteLine($"New culture = {userCulture ?? "unset"}. Old = {CultureInfo.DefaultThreadCurrentCulture?.Name ?? "unset"}");
      var userUiCulture = user.FindFirst("locale")?.Value ?? userCulture;
      //Console.WriteLine($"New locale = {userUiCulture ?? "unset"}. Old = {CultureInfo.DefaultThreadCurrentUICulture?.Name ?? "unset"}");
      if (userUiCulture == null) return tsk;

      CultureInfo.DefaultThreadCurrentCulture   = new(userCulture ?? userUiCulture);
      CultureInfo.DefaultThreadCurrentUICulture = new(userUiCulture);
      return tsk;
   };

await builder.Build().RunAsync();
