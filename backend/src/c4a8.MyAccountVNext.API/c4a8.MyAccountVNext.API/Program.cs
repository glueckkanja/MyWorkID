using Azure.Identity;
using c4a8.MyAccountVNext.API.Options;
using c4a8.MyAccountVNext.API.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Graph;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
                          policy =>
                          {
                              policy.AllowAnyOrigin()
                                                  .AllowAnyHeader()
                                                  .AllowAnyMethod();
                          });
});

builder.Services.Configure<MsGraphOptions>(
    builder.Configuration.GetSection("MsGraph"));
builder.Services.Configure<AppFunctionsOptions>(
    builder.Configuration.GetSection("AppFunctions"));

MsGraphOptions msGraphOptions = new();
builder.Configuration.GetSection("MsGraph").Bind(msGraphOptions);

var graphSettings = new ClientSecretCredential(msGraphOptions.TenantId, msGraphOptions.ClientId, msGraphOptions.ClientSecret);

builder.Services.AddSingleton(new GraphServiceClient(graphSettings));
builder.Services.AddScoped<IAuthContextService, AuthContextService>();

var app = builder.Build();

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

app.Run();
