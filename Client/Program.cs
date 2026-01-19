using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using CapManagement.Client;
using CapManagement.Client.IServiceClient;
using CapManagement.Client.ServiceClient;
using Radzen;
using System.Text.Json;
using CapManagement.Client.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using CapManagement.Client.ServiceClient.UserService; // Add this if missing

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");



builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();



//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });



builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5247/")
});




//builder.Services.AddHttpClient<ICarServiceClient, CarServiceClient>(client =>
//{
//    client.BaseAddress = new Uri("http://localhost:5247/");
//})
//.AddHttpMessageHandler<JwtAuthorizationMessageHandler>();


//builder.Services.AddScoped<ICompanyServiceClient, CompanyServiceClient>();
builder.Services.AddHttpClient<ICompanyServiceClient, CompanyServiceClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5247/");
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddScoped<CurrentUserService>();

builder.Services.AddScoped<IDriverSericeClient, DriverServiceClient>();

builder.Services.AddScoped<ICarServiceClient, CarServiceClient>();

builder.Services.AddScoped<IContractServiceClient, ContractServiceClient>();

builder.Services.AddScoped<ISettlmentClientService, SettlmentServiceClient>();

builder.Services.AddScoped<IExpenseServiceClient, ExpenseServiceClient>();


builder.Services.AddScoped<IExpenseReportServiceClient, ExpenseReportServiceClient>();
builder.Services.AddScoped<IEarningServiceClient, EarningServiceClient>();
builder.Services.AddScoped<LocalStorageService>();

builder.Services.AddScoped<IAuthServiceClient, AuthServiceClient>();


// below is the registration for Authenticationstateprovider and jwtAuth
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthStateProvider>());



// below is the registration for jwtauthorizationMessageHalder
builder.Services.AddScoped<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient("AuthorizedAPI", client =>
{
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// above is the registration for jwtautmessage


builder.Services.AddSingleton<JsonSerializerOptions>(new JsonSerializerOptions
{
    PropertyNamingPolicy = null, // null = PascalCase (matches DTO properties like "Status")
    // Optional: Add if your enums need string serialization (e.g., "Active" instead of 0)
    // Converters = { new JsonStringEnumConverter() }
});

await builder.Build().RunAsync();
