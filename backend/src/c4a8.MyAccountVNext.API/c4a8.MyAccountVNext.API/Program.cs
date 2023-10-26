using c4a8.MyAccountVNext.API;
using c4a8.MyAccountVNext.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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
    options.AddPolicy("AllowAnyMethodAndHeader",
                          policy =>
                          {
                              policy.AllowAnyHeader().AllowAnyMethod();
                          });
});


builder.Services.AddConfig(builder.Configuration);
builder.Services.AddGraphClient(builder.Configuration.GetSection("MsGraph"));

builder.Services.AddScoped<IAuthContextService, AuthContextService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("AllowAll");
} 
else
{
    app.UseCors("AllowAnyMethodAndHeader");
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
