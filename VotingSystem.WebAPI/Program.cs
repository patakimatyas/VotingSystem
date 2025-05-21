using Microsoft.EntityFrameworkCore;
using VotingSystem.DataAccess;
using VotingSystem.WebAPI.Infrastructure;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using VotingSystem.DataAccess.Models;
using VotingSystem.DataAccess.Config;
using System.Security.Claims;


var builder = WebApplication.CreateBuilder(args);

// 1) Always load your base file…
builder.Configuration
       .SetBasePath(builder.Environment.ContentRootPath)
       .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// 2) Then layer on the env-specific one
builder.Configuration
       .AddJsonFile(
           $"appsettings.{builder.Environment.EnvironmentName}.json",
           optional: true,
           reloadOnChange: true
       );


// register database and services
builder.Services.AddDataAccess(builder.Configuration);

builder.Services.AddWebApiAutoMapper();

// Identity + EF Core integration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<VotingSystemDbContext>()
    .AddDefaultTokenProviders();

// swagger and mvc support
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthorization();

var jwtSection = builder.Configuration.GetSection("JwtSettings");
var jwtSettings = jwtSection.Get<JwtSettings>() ?? throw new ArgumentNullException(nameof(JwtSettings));
builder.Services.Configure<JwtSettings>(jwtSection);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidAudience = jwtSettings.Audience,
        ValidIssuer = jwtSettings.Issuer,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7044") // Blazor app URL
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// swagger
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// database migration
if(!app.Environment.IsEnvironment("IntegrationTest"))
{
    using (var scope = app.Services.CreateScope()) // new lifetime 
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<VotingSystemDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        context.Database.Migrate();

        DbInitializer.Initialize(context, userManager);
    }
}


if (app.Environment.IsDevelopment())
{
    Console.WriteLine("DeveloperExceptionPage enabled.");
    app.UseDeveloperExceptionPage();
}
app.Run();
public partial class Program { }

