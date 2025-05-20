using fiap.Application;
using fiap.Repositories;
using fiap.Services;
using Serilog;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using fiap.Domain.Interfaces;
using fiap.Domain.Entities;
using System.Diagnostics.CodeAnalysis;

[assembly: ExcludeFromCodeCoverage]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "FIAP - Tech Challenge - API Produtos", Version = "v1" });
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath);
    });
builder.Services.AddHealthChecks();

// Adiciona inje��o de depend�ncia no Application
builder.Services.AddHttpClient();
builder.Services.AddApplicationModule();
builder.Services.AddServicesModule();

builder.Services.AddSingleton<Func<IDbConnection>>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("fiap.sqlServer");
    var secretService = sp.GetRequiredService<ISecretManagerService>();

    var secret = secretService.ObterSecret<SecretDbConnect>("dev/fiap/sql-rds").Result;

    if (secret.Host is null)
    {
        Console.WriteLine("N�o foi poss�vel recuperar a secret - Console.WriteLine"); ;
        Log.Information("N�o foi poss�vel recuperar a secret Serilog");
        throw new Exception("N�o foi poss�vel recuperar a secret - Lan�ada excecao");
    }

    connectionString = connectionString
    .Replace("__server__", secret.Host)
    .Replace("__port__", secret.Port)
    .Replace("__db__", secret.DbInstanceIdentifier)
    .Replace("__userdb__", secret.UserName)
    .Replace("__senha__", secret.Password);

    return () => new SqlConnection(connectionString);
});


builder.Services.AddRepositoriesModule();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(opt =>
{
    opt.SwaggerEndpoint("/swagger/v1/swagger.json", "FIAP - Tech Challenge V1");
});


app.UseHttpsRedirection();
app.UseSerilogRequestLogging();
app.UseAuthorization();
app.UseRouting();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("api/health");
    endpoints.MapHealthChecks("api/metrics");
    endpoints.MapControllers();
});

app.UsePathBase("api-produtos");
app.MapControllers();
app.Map("/", app1 => app1
.Run(ctx => ctx.Response
.WriteAsync($"path-Base: {ctx.Request.PathBase} Path: {ctx.Request.Path}")));

Log.Information("Iniciando aplica��o");
app.Run();


