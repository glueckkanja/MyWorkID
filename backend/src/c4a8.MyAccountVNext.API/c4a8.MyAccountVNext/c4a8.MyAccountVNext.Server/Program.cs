using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Identity.Web;
using c4a8.MyAccountVNext.API;
using c4a8.MyAccountVNext.API.Services;
using c4a8.MyAccountVNext.Server.Services;
using Azure.Identity;
using Azure.Core;
using c4a8.MyAccountVNext.Server;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.IdentityModel.Tokens;
using c4a8.MyAccountVNext.Server.Options;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using c4a8.MyAccountVNext.Server.Hubs;
using c4a8.MyAccountVNext.Server.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

// Add services to the container.
builder.Services.AddControllers();

VerifiedIdOptions verifiedIdConfig = new();
builder.Configuration.GetSection("VerifiedId").Bind(verifiedIdConfig);
var signingByte = Encoding.UTF8.GetBytes(verifiedIdConfig.JwtSigningKey ?? "GodDamnitYouForgottToSpecifyASigningKey");

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(Strings.VERIFIED_ID_CALLBACK_SCHEMA, options =>
{
    options.TokenValidationParameters.ValidateIssuerSigningKey = true;
    options.TokenValidationParameters.IssuerSigningKey = new SymmetricSecurityKey(signingByte);
    options.TokenValidationParameters.ValidateIssuer = false;
    options.TokenValidationParameters.ValidateAudience = false;
    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
    options.TokenValidationParameters.ClockSkew = TimeSpan.Zero;
})
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers().AddJsonOptions((options) =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddApplicationInsightsTelemetry();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    // Only for development
    options.AddPolicy("AllowAll",
                          policy =>
                          {
                              policy.AllowAnyOrigin()
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod();
                          });
});

builder.Services.AddConfig(builder.Configuration);
builder.Services.AddGraphClient(builder.Configuration.GetSection("MsGraph"));

TokenCredential verifiedIdCredentials = new ManagedIdentityCredential();

if (builder.Environment.IsDevelopment())
{
    verifiedIdCredentials = new ClientSecretCredential(builder.Configuration["LocalDevSettings:VerifiedIdTenantId"], builder.Configuration["LocalDevSettings:VerifiedIdClientId"], builder.Configuration["LocalDevSettings:VerifiedIdSecret"]);
}
builder.Services.AddVerifiedIdHttpClient<VerifiedIdService>(verifiedIdCredentials);

builder.Services.AddScoped<IAuthContextService, AuthContextService>();
builder.Services.AddSingleton<VerifiedIdSignalRRepository>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHub<VerifiedIdHub>("/hubs/verifiedId");
app.MapFallbackToFile("/index.html");

app.Run();
