using LuShop.Api;
using LuShop.Api.Common.Api;
using LuShop.Api.Endpoints;
using LuShop.Api.Models;
var builder = WebApplication.CreateBuilder(args);

builder.AddConfiguration();
builder.AddSecurity();
builder.AddDataContexts();
builder.AddCrossOrigin();
builder.AddDocumentation();
builder.AddServices();
builder.AddJsonSerialization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.ConfigureDevEnvironment();

app.UseCors(ApiConfiguration.CorsPolicyName);
app.UseStaticFiles();
app.UseSecurity();
app.MapEndpoints();

app.Run();