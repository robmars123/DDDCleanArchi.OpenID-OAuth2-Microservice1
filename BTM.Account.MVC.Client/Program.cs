using BTM.Account.Application.Factories.HttpRequest;
using BTM.Account.Infrastructure.Dependencies;
using BTM.Account.Infrastructure.Factories;
using BTM.Account.Shared.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.Net.Http.Headers;

namespace BTM.Account.MVC.Client;

public static class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.AddServiceDefaults();

        // Add services to the container..
        RegisterFactories(builder);

        builder.Services.AddControllersWithViews()
                    .AddJsonOptions(configure => configure.JsonSerializerOptions.PropertyNamingPolicy = null);

        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        // create an HttpClient used for accessing the API
        builder.Services.AddHttpClient("AccountAPI", client =>
        {
            client.BaseAddress = new Uri(builder.Configuration["AccountAPI"]);
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
        });
        builder.Services.AddOpenIdConnectAccessTokenManagement();

        builder.Services.AddHttpClient("IDPClient", client =>
        {
            //IdentityServer URL
            client.BaseAddress = new Uri("https://localhost:5001/");
        });

        //Authentication builder
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
        {
            options.AccessDeniedPath = "/Account/AccessDenied";
        })
        .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
        {
            options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.Authority = "https://localhost:5001/";//this launchingsettings of IDP.
            //this is the client id of the MVC app.
            options.ClientId = "Account.MVC.Client"; //this is set in IDP.Config.cs
            options.ClientSecret = "mysecret";//this is set in IDP.Config.cs
            options.ResponseType = "code";//this is set in IDP.Config.cs

            options.GetClaimsFromUserInfoEndpoint = true;
            options.ClaimActions.Remove("aud");
            options.ClaimActions.DeleteClaim("sid");
            options.ClaimActions.DeleteClaim("idp");

            options.TokenValidationParameters = new()
            {
                NameClaimType = "name",
                RoleClaimType = "role"
            };

            options.SaveTokens = true;
            // and refresh token
            options.Scope.Add("offline_access");
        });

        //Authorization builder
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(GlobalConstants.Roles.Admin, policy =>
        policy.RequireClaim("role", GlobalConstants.Roles.Admin));
        });


        var app = builder.Build();

        app.MapDefaultEndpoints();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.Run();
    }

    private static void RegisterFactories(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IRequestFactory, RequestFactory>();
    }
}
