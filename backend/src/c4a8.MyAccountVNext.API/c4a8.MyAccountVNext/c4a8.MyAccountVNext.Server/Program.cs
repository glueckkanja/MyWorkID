using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text.Json.Serialization;
using System.Text.Json;
using Microsoft.Identity.Web;
using c4a8.MyAccountVNext.API;
using c4a8.MyAccountVNext.API.Services;
using c4a8.MyAccountVNext.Server.Services;
using Azure.Identity;
using Azure.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers().AddJsonOptions((options) =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

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

app.MapFallbackToFile("/index.html");

app.Run();
