using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Json;
using MyWorkID.Server;
using MyWorkID.Server.Common;
using MyWorkID.Server.Features.VerifiedId.SignalR;
using MyWorkID.Server.Kernel;

Assembly appAssembly = Assembly.GetExecutingAssembly();
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// see https://github.com/dotnet/aspnetcore/issues/37680#issuecomment-1331559463
builder.Configuration.AddTestConfiguration();

builder.Services.ValidateOptionsOnStartup(builder.Configuration);

builder.Services.AddSignalR();
builder.Services.AddCustomAuthentication(builder.Configuration);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

// Skip Application Insights registration when no connection string is configured.
string? applicationInsightsConnectionString =
    builder.Configuration["ApplicationInsights:ConnectionString"]
    ?? builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
if (!string.IsNullOrWhiteSpace(applicationInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry();
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.ConfigureModules(builder.Configuration, builder.Environment, appAssembly);

WebApplication app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseExceptionHandler(exceptionHandlerApp =>
{
    exceptionHandlerApp.Run(async context =>
    {
        ILogger<Program> logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        IExceptionHandlerPathFeature? exceptionHandlerPathFeature =
            context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
        if (exceptionHandlerPathFeature?.Error != null)
        {
            logger.LogError(
                exceptionHandlerPathFeature.Error,
                "An unhandled exception has occurred."
            );
        }
        await Results
            .Problem(
                extensions: new Dictionary<string, object?>
                {
                    ["correlationId"] = context.TraceIdentifier,
                }
            )
            .ExecuteAsync(context);
    });
});

app.UseStatusCodePages(async statusCodeContext =>
    await Results
        .Problem(
            statusCode: statusCodeContext.HttpContext.Response.StatusCode,
            extensions: new Dictionary<string, object?>
            {
                ["correlationId"] = statusCodeContext.HttpContext.TraceIdentifier,
            }
        )
        .ExecuteAsync(statusCodeContext.HttpContext)
);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(builder =>
        builder
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowAnyOrigin()
            .WithExposedHeaders("Content-Disposition")
    );
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.RegisterEndpoints(appAssembly);

app.MapHub<VerifiedIdHub>("/hubs/verifiedId");
app.MapFallbackToFile("/index.html");

await app.RunAsync();
