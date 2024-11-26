using BlazorWebApp.Components;
using BlazorWebApp.Components.Account;
using BlazorWebApp.Data;
using BlazorWebApp.DemoUsage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

/***********************
 * Added for this demo *
 ***********************/

SetupRuntime.Configure(builder);
SetupDeployment.Configure(builder);

/***************************
 * End added for this demo *
 ***************************/


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options => {
   options.DefaultScheme = IdentityConstants.ApplicationScheme;
   options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
   app.UseWebAssemblyDebugging();
   app.UseMigrationsEndPoint();
}
else
{
   app.UseExceptionHandler("/Error", createScopeForErrors: true);
   // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
   app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorWebApp.Client._Imports).Assembly);

/***********************
 * Added for this demo *
 ***********************/

SetupRuntime.Use(app);
await SetupDeployment.Sync(app);

/***************************
 * End added for this demo *
 ***************************/

/***********************
 * REMOVED for this demo *
 ***********************/

// Add additional endpoints required by the Identity /Account Razor components.
//app.MapAdditionalIdentityEndpoints();

/***************************
 * End REMOVED for this demo *
 ***************************/

app.Run();
