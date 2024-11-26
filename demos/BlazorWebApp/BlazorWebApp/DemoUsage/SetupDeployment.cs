using Catglobe.CgScript.Deployment;

namespace BlazorWebApp.DemoUsage;

internal class SetupDeployment
{
   public static void Configure(WebApplicationBuilder builder)
   {
      builder.Services.AddCgScriptDeployment(builder.Configuration.GetSection("CatglobeDeployment"));
   }

   public static async Task Sync(WebApplication app)
   {
      await app.Services.GetRequiredService<IDeployer>().Sync(default);
   }
}

