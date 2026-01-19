using CapManagement.Server.DbContexts;
using CapManagement.Server.IRepository;
using CapManagement.Server.IService;
using CapManagement.Server.Repository;
using CapManagement.Server.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
    
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text.Json.Serialization;

using QuestPDF.Infrastructure;
using CapManagement.Server.AppicationUserModels;
using CapManagement.Server.DbContexts.Seed;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.OpenApi.Models;



var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// ===== Add DbContext =====
builder.Services.AddDbContext<FleetDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));




// ===== Register Repositories & Services =====
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddHttpClient<RdwCarService, RdwCarService>();

builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<IDriverService, DriverService>();

builder.Services.AddScoped<IDriverRepository, DriverRepository>();

builder.Services.AddScoped<IExpenseReportService, ExpenseReportService>();


builder.Services.AddScoped<IContractService, ContractService>();

builder.Services.AddScoped<IContractRepository, ContractRepository>();

builder.Services.AddScoped<IEarningService, EarningService>();
builder.Services.AddScoped<IEarningRepository, EarningRepository>();

builder.Services.AddScoped<ISettlmentService, SettlmentService>();
builder.Services.AddScoped<ISettlementRepository, SettlmentRepository>();

builder.Services.AddScoped<IExpenseRepository, ExpenseRepository>();

builder.Services.AddScoped<IExpenseService, ExpenseService>();

builder.Services.AddScoped<IUserInviteRepository, UserInviteRepository>();

builder.Services.AddScoped<IUserInviteService, UserInviteService>();

builder.Services.AddScoped<IEmailSender, SendGridEmailSender>();

builder.Services.AddScoped<IAuthService, AuthService>();


// regiseration of applicationuser and application role
//builder.Services.AddIdentity<ApplicationUser, ApplicationRole>();

builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password rules (real-world defaults)
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;

    // User rules
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<FleetDbContext>()
.AddDefaultTokenProviders();


// below is the reigstration for jwt token

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})



.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
});






builder.Services.AddScoped<PdfService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                "http://localhost:7033"  // client URL in dev
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

// ===== Add services to the container =====
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// ===== Add Swagger =====
builder.Services.AddEndpointsApiExplorer();



//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CapManagement API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter: Bearer {JWT token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
       
    });


var app = builder.Build();

// ===== Configure the HTTP request pipeline =====
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();

    // Swagger only in dev
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CapManagement API v1");
        c.RoutePrefix = "swagger"; // UI at /swagger
    });
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();

app.UseAuthorization();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");




// SEEDING GOES HERE (IMPORTANT)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    await RoleSeeder.SeedAsync(
        services.GetRequiredService<RoleManager<ApplicationRole>>());

    await OwnerSeeder.SeedAsync(
        services.GetRequiredService<UserManager<ApplicationUser>>());
}



app.Run();
