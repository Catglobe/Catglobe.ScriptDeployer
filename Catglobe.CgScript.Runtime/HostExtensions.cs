using System.Diagnostics.CodeAnalysis;
using Catglobe.CgScript.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Catglobe.CgScript.Runtime;

/// <summary>
/// Setup methods
/// </summary>
public static class HostExtensions
{
   /// <summary>
   /// Add CgScript support
   /// </summary>
   public static IServiceCollection AddCgScript(this IServiceCollection services, Action<CgScriptOptions>? configurator = null)
   {
      if (configurator is not null) services.Configure(configurator);
      return AddCommonCgScript(services);
   }
   /// <summary>
   /// Add CgScript support
   /// </summary>
   [RequiresUnreferencedCode("Options")]
   public static IServiceCollection AddCgScript(this IServiceCollection services, IConfiguration namedConfigurationSection)
   {
      services.Configure<CgScriptOptions>(namedConfigurationSection);
      return AddCommonCgScript(services);
   }

   private static IServiceCollection AddCommonCgScript(IServiceCollection services)
   {
      services.AddHttpContextAccessor();
      services.AddHttpClient<IScriptMapping, ScriptMapping>((sp, httpClient) => {
         var site = sp.GetRequiredService<IOptions<CgScriptOptions>>().Value.Site;
         httpClient.BaseAddress = new(site + "/api/CgScriptDeployment/");
      });

      services.AddScoped<CgScriptAuthHandler>();
      services.AddHttpClient<ICgScriptApiClient, CgScriptApiClient>((sp, httpClient) => {
                  var site = sp.GetRequiredService<IOptions<CgScriptOptions>>().Value.Site;
                  httpClient.BaseAddress = new(site + "/api/CgScript/");
               })
              .AddHttpMessageHandler<CgScriptAuthHandler>();
      return services;
   }
}

