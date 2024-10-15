using c4a8.MyAccountVNext.API;
using c4a8.MyAccountVNext.Server.Features.VerifiedId.SignalR;
using c4a8.MyAccountVNext.Server.Kernel;
using Microsoft.AspNetCore.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

var appAssembly = Assembly.GetExecutingAssembly();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();
builder.Services.AddCustomAuthentication(builder.Configuration);

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.AddApplicationInsightsTelemetry();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();
builder.Services.ConfigureModules(builder.Configuration, builder.Environment, appAssembly);
//builder.Services.AddProblemDetails();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseExceptionHandler(exceptionHandlerApp
    => exceptionHandlerApp.Run(async context
        => await Results.Problem()
                     .ExecuteAsync(context)));

app.UseStatusCodePages(async statusCodeContext
    => await Results.Problem(statusCode: statusCodeContext.HttpContext.Response.StatusCode)
                 .ExecuteAsync(statusCodeContext.HttpContext));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors(builder => builder
       .AllowAnyHeader()
       .AllowAnyMethod()
       .AllowAnyOrigin()
       .WithExposedHeaders("Content-Disposition"));
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.RegisterEndpoints(appAssembly);

//app.MapControllers();
app.MapHub<VerifiedIdHub>("/hubs/verifiedId");
app.MapFallbackToFile("/index.html");

app.Run();
