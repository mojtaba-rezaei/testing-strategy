using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrderProcessor.FunctionApp.Services;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddApplicationInsightsTelemetryWorkerService();

// Register application services
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Build().Run();
