using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderProcessor.FunctionApp.Models;
using OrderProcessor.FunctionApp.Services;

namespace OrderProcessor.FunctionApp.Functions;

public class ProcessOrderFunction
{
    private readonly IOrderService _orderService;
    private readonly ILogger<ProcessOrderFunction> _logger;

    public ProcessOrderFunction(IOrderService orderService, ILogger<ProcessOrderFunction> logger)
    {
        _orderService = orderService ?? throw new ArgumentNullException(nameof(orderService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("ProcessOrder")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "orders/process")] HttpRequestData req)
    {
        _logger.LogInformation("Processing order request");

        try
        {
            // Deserialize order from request body
            var order = await JsonSerializer.DeserializeAsync<Order>(req.Body);

            if (order == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid order data");
            }

            // Process the order
            var result = await _orderService.ProcessOrderAsync(order);

            if (result.Success)
            {
                return await CreateSuccessResponse(req, result);
            }
            else
            {
                var statusCode = result.ErrorCode == "VALIDATION_ERROR" 
                    ? HttpStatusCode.BadRequest 
                    : HttpStatusCode.InternalServerError;
                
                return await CreateErrorResponse(req, statusCode, result.Message, result.Metadata);
            }
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in request body");
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid JSON format");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing order");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    [Function("GetOrder")]
    public async Task<HttpResponseData> GetOrder(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "orders/{orderId}")] HttpRequestData req,
        string orderId)
    {
        _logger.LogInformation("Getting order {OrderId}", orderId);

        try
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);

            if (order == null)
            {
                return await CreateErrorResponse(req, HttpStatusCode.NotFound, "Order not found");
            }

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(order);
            return response;
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid order ID: {OrderId}", orderId);
            return await CreateErrorResponse(req, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving order {OrderId}", orderId);
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError, "An error occurred");
        }
    }

    private async Task<HttpResponseData> CreateSuccessResponse(HttpRequestData req, ProcessResult result)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(new
        {
            success = true,
            message = result.Message,
            data = result.Metadata
        });
        return response;
    }

    private async Task<HttpResponseData> CreateErrorResponse(
        HttpRequestData req, 
        HttpStatusCode statusCode, 
        string message,
        Dictionary<string, object>? metadata = null)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteAsJsonAsync(new
        {
            success = false,
            message,
            errors = metadata
        });
        return response;
    }
}
