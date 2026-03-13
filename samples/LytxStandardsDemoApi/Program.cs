using LytxStandardsDemoApi.Data;
using LytxStandardsDemoApi.Infrastructure.FeatureToggles;
using LytxStandardsDemoApi.Infrastructure.Logging;
using LytxStandardsDemoApi.Infrastructure.Security;
using LytxStandardsDemoApi.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5085");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddAuthentication("DemoHeader")
    .AddScheme<AuthenticationSchemeOptions, DemoHeaderAuthenticationHandler>("DemoHeader", _ => { });

builder.Services.AddAuthorization();

builder.Services.AddSingleton<IFeatureToggleCollection, InMemoryFeatureToggleCollection>();
builder.Services.AddScoped<ITransactionLogger, TransactionLogger>();
builder.Services.AddSingleton<IPostgresDbContext, InMemoryPostgresDbContext>();
builder.Services.AddScoped<IEntityDataAccess, PostgreSqlEntityDataAccess>();
builder.Services.AddScoped<IEntitySummaryService, EntitySummaryService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
