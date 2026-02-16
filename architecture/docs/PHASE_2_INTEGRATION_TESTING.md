# Phase 2: Integration Testing (Mature Phase)

## Purpose
This document defines Phase 2 of the automation testing strategy: **Integration Testing**. Phase 2 builds upon the unit testing foundation established in Phase 1 and adds testing of component interactions, Azure services, contracts, and performance.

## Reference
This is extracted from [AUTOMATION_TESTING_STANDARD.md](AUTOMATION_TESTING_STANDARD.md) covering Phase 2 integration testing requirements, examples, and best practices.

## Overview

### What is Phase 2?
Phase 2 focuses on **integration testing** - testing interactions between multiple components, validating Azure service integrations, ensuring contract compliance, and establishing performance baselines.

### Phase Duration
**3-6 months** for full adoption

### Phase Prerequisites
- ✅ Phase 1 exit criteria met
- ✅ Unit test coverage ≥ 80%
- ✅ Test environment infrastructure available
- ✅ Team capacity for integration testing

### Phase Objectives
- Test all integration points between Azure components
- Validate contracts (APIs, messages, schemas)
- Establish performance baselines
- Automate test data management
- Achieve < 15 minute integration test execution time
- Maintain < 5% flaky test rate

## Testing Scope - Step 2 (Integration Testing)

### Required

- ✅ **Integration tests between Azure components:**
  - Function → Service Bus
  - Function → CosmosDB
  - Logic App → API
  - Function → Blob Storage
  - API → Database

- ✅ **Component interaction validation:**
  - Triggers (HTTP, Timer, Queue, Event Grid, Blob)
  - Bindings (Input/Output bindings)
  - Connectors (Logic Apps)
  - Service-to-service communication

- ✅ **Contract tests:**
  - API schema validation (OpenAPI/Swagger)
  - Message format validation (JSON Schema)
  - Event schema validation
  - Backward compatibility checks

- ✅ **Regression tests:**
  - Critical integration paths
  - Previously identified bugs
  - Breaking change prevention

- ✅ **Smoke tests:**
  - Post-deployment validation
  - Critical path verification
  - Environment health checks

- ✅ **Performance tests:**
  - Baseline metrics establishment
  - Response time validation
  - Throughput testing
  - Resource utilization monitoring

- ✅ **Test data management:**
  - Automated setup/teardown
  - Data isolation strategies
  - Test data cleanup

### Intentionally Deferred (Not in Phase 2)

- ❌ **End-to-end tests** (performed manually in release/test environments)
- ❌ **Full user journey testing** (manual verification in Azure portal)
- ❌ **Production testing**

### Optional (Advanced)

- ⚠️ Load tests (stress, spike, soak)
- ⚠️ Resiliency tests (circuit breaker, retry, timeout)
- ⚠️ Chaos engineering tests
- ⚠️ Security/penetration tests

## Test Pyramid Architecture - Phase 2 Focus

### 30% Integration Tests (Middle Layer)

In Phase 2, you'll build the middle layer of the test pyramid while maintaining the 60% unit test foundation:

```
                     /\
                    /  \
                   /    \
                  / E2E  \ ← Still manual (10%)
                 /_______ \
                /          \
               /Integration \ ← ADD THIS (30%)
              /_____________ \
             /                \
            /   Unit Tests     \ ← MAINTAIN THIS (60%)
           /___________________ \
```

**Why 30% Integration Tests?**
- **Realistic:** Test actual component interactions
- **Confidence:** Validate integration points, contracts, data flow
- **Azure-Specific:** Test real Azure services
- **Balance:** More valuable than E2E, faster than E2E
- **Cost:** Higher cost than unit tests (requires infrastructure)
- **Scope:** Focus on critical integration points

## Azure-Specific Testing Strategies (Phase 2)

### Azure Functions (Integration Testing)

**What to Test:**
- Real triggers (HTTP, Timer, Queue, Event Grid)
- Real bindings (CosmosDB, Blob, Table, Service Bus)
- Dependency injection container
- Service Bus message processing
- End-to-end message flow

**How to Test:**
- Use real Azure services in test environment
- Use local emulators where possible (Azurite, Service Bus)
- Test actual trigger execution
- Validate binding behavior
- Test failure scenarios (dead-letter, retries)

**Example:**
```csharp
[Collection("IntegrationTests")]
public class OrderProcessingIntegrationTests : IAsyncLifetime
{
    private ServiceBusClient _serviceBusClient;
    private ServiceBusSender _sender;
    private readonly string _testRunId;

    public OrderProcessingIntegrationTests()
    {
        _testRunId = Guid.NewGuid().ToString();
    }

    public async Task InitializeAsync()
    {
        var connectionString = _configuration["ServiceBus:ConnectionString"];
        _serviceBusClient = new ServiceBusClient(connectionString);
        _sender = _serviceBusClient.CreateSender("orders-queue");
    }

    [Fact]
    public async Task ProcessOrder_EndToEnd_MessageProcessedSuccessfully()
    {
        // Arrange
        var order = new OrderBuilder()
            .WithId($"ORDER-{_testRunId}")
            .Build();

        var message = new ServiceBusMessage(JsonSerializer.Serialize(order));

        // Act - Send message to trigger Function
        await _sender.SendMessageAsync(message);

        // Assert - Wait for processed result
        await VerifyOrderProcessed(order.Id);
    }

    public async Task DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _serviceBusClient.DisposeAsync();
    }
}
```

### Logic Apps Standard (Integration Testing)

**What to Test:**
- Workflow execution
- Built-in connectors (HTTP, Service Bus, Storage)
- Managed connectors (Dynamics, SAP, SQL)
- Triggers (recurrence, HTTP, event-based)
- Error handling and retries

**How to Test:**
- Use Logic Apps local runtime
- Test with real connectors in test environment
- Validate workflow run history
- Test error paths and retries

### Data Factory (Integration Testing)

**What to Test:**
- Pipeline runs
- Data movement activities
- Data transformation
- Linked services connectivity

**How to Test:**
- Use test environment Data Factory
- Validate data flow end-to-end
- Test with sample datasets
- Verify data quality

### Async/Retries/Failures (Integration Testing)

**What to Test:**
- Transient failures with real services
- Poison message handling
- Dead-letter queue scenarios
- Circuit breaker behavior
- Timeout handling in real scenarios

**Example:**
```csharp
[Fact]
public async Task ProcessOrder_WithInvalidData_SendsToDeadLetter()
{
    // Arrange
    var invalidMessage = new ServiceBusMessage("Invalid JSON");
    invalidMessage.ApplicationProperties["TestRunId"] = _testRunId;

    // Act - Send invalid message
    await _sender.SendMessageAsync(invalidMessage);
    await Task.Delay(5000); // Allow processing time

    // Assert - Check dead-letter queue
    var deadLetterReceiver = _serviceBusClient.CreateReceiver(
        "orders-queue",
        new ServiceBusReceiverOptions { SubQueue = SubQueue.DeadLetter }
    );

    var deadLetterMessage = await deadLetterReceiver.ReceiveMessageAsync(
        maxWaitTime: TimeSpan.FromSeconds(10)
    );

    deadLetterMessage.Should().NotBeNull();
    deadLetterMessage.DeadLetterReason.Should().Contain("Deserialization");
}
```

## Tooling and Frameworks (Phase 2)

### Azure-Native Tools

**Azurite (Storage Emulator):**
```bash
# Install
npm install -g azurite

# Run
azurite --silent --location c:\azurite --debug c:\azurite\debug.log
```

**Logic Apps Standard Local Runtime:**
- Visual Studio Code extension
- Azure Functions Core Tools
- Local workflow testing

**Azure Functions Core Tools:**
```bash
# Install
npm install -g azure-functions-core-tools@4

# Run locally
func start
```

**Azure CLI:**
```bash
# Provision test resources
az group create --name rg-integration-tests --location eastus
az servicebus namespace create --name sb-test --resource-group rg-integration-tests

# Cleanup
az group delete --name rg-integration-tests --yes --no-wait
```

### Open-Source Tools

**Postman/Newman (API Contract Testing):**
```bash
# Install Newman
npm install -g newman

# Run collection
newman run api-contract-tests.json --environment test-env.json
```

**TestContainers (Infrastructure Dependencies):**
```csharp
// Example: SQL Server container for integration tests
public class DatabaseIntegrationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _sqlContainer = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();

    public async Task InitializeAsync()
    {
        await _sqlContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _sqlContainer.DisposeAsync();
    }
}
```

### Azure SDK for Testing

```xml
<ItemGroup>
  <PackageReference Include="Azure.Messaging.ServiceBus" Version="7.17.0" />
  <PackageReference Include="Azure.Storage.Blobs" Version="12.19.0" />
  <PackageReference Include="Azure.Data.Tables" Version="12.8.0" />
  <PackageReference Include="Microsoft.Azure.Cosmos" Version="3.37.0" />
  <PackageReference Include="Microsoft.Azure.WebJobs.Extensions.ServiceBus" Version="5.13.0" />
</ItemGroup>
```

### Contract Testing

**Pact (Consumer-Driven Contracts):**
```csharp
// Consumer test
[Fact]
public async Task GetOrder_ReturnsOrderMatchingContract()
{
    _mockProviderService
        .Given("Order with ID 123 exists")
        .UponReceiving("A GET request for order 123")
        .With(new ProviderServiceRequest
        {
            Method = HttpVerb.Get,
            Path = "/api/orders/123"
        })
        .WillRespondWith(new ProviderServiceResponse
        {
            Status = 200,
            Headers = new Dictionary<string, object>
            {
                { "Content-Type", "application/json" }
            },
            Body = new
            {
                id = "123",
                customerId = "C-456",
                amount = 99.99m
            }
        });

    var client = new OrderApiClient(_mockProviderServiceBaseUri);
    var order = await client.GetOrder("123");

    order.Id.Should().Be("123");
    _mockProviderService.VerifyInteractions();
}
```

**JSON Schema Validators:**
```csharp
using Newtonsoft.Json.Schema;

[Fact]
public async Task GetOrder_ReturnsResponseMatchingSchema()
{
    // Arrange
    var schema = JSchema.Parse(File.ReadAllText("Schemas/order-schema.json"));
    
    // Act
    var response = await _httpClient.GetAsync("/api/orders/123");
    var json = JObject.Parse(await response.Content.ReadAsStringAsync());

    // Assert
    json.IsValid(schema).Should().BeTrue();
}
```

**OpenAPI/Swagger Validators:**
```csharp
[Fact]
public async Task ApiEndpoints_MatchOpenApiSpecification()
{
    var openApiDoc = await OpenApiDocument.LoadAsync("swagger.json");
    
    // Validate API against OpenAPI spec
    var validator = new OpenApiValidator(openApiDoc);
    var result = await validator.ValidateAsync(_httpClient);
    
    result.IsValid.Should().BeTrue();
}
```

## Folder Structure (Phase 2)

```
/project-root
  /src
    /OrderProcessor.FunctionApp
      # Application code
  
  /tests
    /unit
      /OrderProcessor.UnitTests
        # Unit tests from Phase 1
    
    /integration                           ← NEW IN PHASE 2
      /OrderProcessor.IntegrationTests
        /Scenarios                          ← End-to-end integration scenarios
          OrderProcessingTests.cs
          PaymentProcessingTests.cs
        /Contracts                          ← Contract tests
          OrderApiContractTests.cs
          PaymentApiContractTests.cs
        /Performance                        ← Performance baseline tests
          OrderProcessingPerformanceTests.cs
        /Fixtures                           ← Test fixtures and helpers
          ServiceBusFixture.cs
          CosmosDbFixture.cs
          StorageFixture.cs
        appsettings.test.json               ← Test configuration
        OrderProcessor.IntegrationTests.csproj
    
    /shared
      /TestUtilities
        /Builders
        /Helpers
        /TestData                           ← Test data files
          sample-order.json
          sample-payment.json
  
  /pipelines
    ci-unit-tests.yml
    ci-integration-tests.yml                ← NEW IN PHASE 2
```

## Contract Testing (Phase 2 Focus)

### What are Contract Tests?

Contract tests validate that:
- APIs conform to their specification (OpenAPI/Swagger)
- Messages conform to their schema (JSON Schema)
- Events conform to their schema
- Backward compatibility is maintained

### API Contract Testing Example

**Schema Definition (order-schema.json):**
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["id", "customerId", "amount", "status"],
  "properties": {
    "id": {
      "type": "string",
      "pattern": "^ORDER-[0-9]+$"
    },
    "customerId": {
      "type": "string",
      "pattern": "^C-[0-9]+$"
    },
    "amount": {
      "type": "number",
      "minimum": 0
    },
    "status": {
      "type": "string",
      "enum": ["Pending", "Processed", "Failed"]
    }
  }
}
```

**Contract Test:**
```csharp
public class OrderApiContractTests
{
    private readonly HttpClient _httpClient;
    private readonly JSchema _orderSchema;

    public OrderApiContractTests()
    {
        _httpClient = new HttpClient 
        { 
            BaseAddress = new Uri("https://api-test.contoso.com") 
        };
        
        _orderSchema = JSchema.Parse(File.ReadAllText("Schemas/order-schema.json"));
    }

    [Fact]
    public async Task GetOrder_ReturnsResponseMatchingContract()
    {
        // Act
        var response = await _httpClient.GetAsync("/api/orders/123");
        var content = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(content);

        // Assert - Validate against schema
        json.IsValid(_orderSchema).Should().BeTrue();
        
        // Additional contract assertions
        json["id"].Should().NotBeNull();
        json["status"].Value<string>().Should().BeOneOf("Pending", "Processed", "Failed");
    }

    [Fact]
    public async Task CreateOrder_RequestMatchesContract()
    {
        // Arrange
        var orderRequest = new
        {
            customerId = "C-123",
            amount = 99.99m,
            items = new[]
            {
                new { productId = "P-1", quantity = 2 }
            }
        };

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/orders", orderRequest);

        // Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseBody = await response.Content.ReadAsStringAsync();
        var json = JObject.Parse(responseBody);
        json.IsValid(_orderSchema).Should().BeTrue();
    }
}
```

### Message Contract Testing Example

```csharp
public class ServiceBusContractTests
{
    private readonly JSchema _orderMessageSchema;

    [Fact]
    public async Task OrderMessage_ConformsToContract()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        var messageBody = JsonSerializer.Serialize(order);
        var json = JObject.Parse(messageBody);

        // Assert
        json.IsValid(_orderMessageSchema).Should().BeTrue();
    }
}
```

## Performance Testing (Phase 2)

### Performance Baseline Tests

**Purpose:** Establish baseline metrics to detect performance regression

**Example:**
```csharp
public class OrderProcessingPerformanceTests
{
    [Fact]
    public async Task ProcessOrder_CompletesWithinAcceptableTime()
    {
        // Arrange
        var order = new OrderBuilder().Build();
        var stopwatch = Stopwatch.StartNew();

        // Act
        var result = await _orderService.ProcessOrder(order);

        // Assert
        stopwatch.Stop();
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(2000); // 2 second SLA
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task ProcessOrders_HandlesConcurrentRequests()
    {
        // Arrange
        var orders = Enumerable.Range(1, 10)
            .Select(_ => new OrderBuilder().Build())
            .ToList();

        var stopwatch = Stopwatch.StartNew();

        // Act
        var tasks = orders.Select(o => _orderService.ProcessOrder(o));
        var results = await Task.WhenAll(tasks);

        // Assert
        stopwatch.Stop();
        results.Should().AllSatisfy(r => r.Success.Should().BeTrue());
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // All complete within 5 seconds
    }
}
```

### Load Testing (Optional)

**Azure Load Testing:**
```yaml
# load-test-config.yaml
testName: OrderProcessingLoadTest
description: Load test for order processing endpoint
engineInstances: 1
testPlan: order-processing-test.jmx
properties:
  duration: 300
  threads: 50
  rampup: 60
```

**k6 (Open Source Alternative):**
```javascript
// load-test.js
import http from 'k6/http';
import { check, sleep } from 'k6';

export let options = {
  stages: [
    { duration: '2m', target: 50 },  // Ramp up
    { duration: '5m', target: 50 },  // Stay at 50 users
    { duration: '2m', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<2000'], // 95% of requests under 2s
    http_req_failed: ['rate<0.01'],    // Error rate < 1%
  },
};

export default function () {
  const order = {
    customerId: 'C-123',
    amount: 99.99,
  };

  const response = http.post('https://api-test.contoso.com/api/orders', JSON.stringify(order));
  
  check(response, {
    'status is 200': (r) => r.status === 200,
    'response time < 2s': (r) => r.timings.duration < 2000,
  });

  sleep(1);
}
```

## Test Data Management (Phase 2)

### Automated Setup/Teardown

```csharp
public class OrderProcessingTests : IAsyncLifetime
{
    private readonly string _testRunId;
    private readonly List<string> _createdOrders = new();

    public OrderProcessingTests()
    {
        _testRunId = Guid.NewGuid().ToString();
    }

    public async Task InitializeAsync()
    {
        // Setup test data
        await CreateTestStorageContainer();
        await CreateTestServiceBusQueue();
    }

    [Fact]
    public async Task ProcessOrder_EndToEnd_Success()
    {
        // Arrange
        var orderId = $"ORDER-{_testRunId}-001";
        _createdOrders.Add(orderId);

        // Act & Assert
        // ...
    }

    public async Task DisposeAsync()
    {
        // Cleanup - Delete all test data
        foreach (var orderId in _createdOrders)
        {
            await DeleteOrder(orderId);
        }

        await DeleteTestStorageContainer();
        await DeleteTestServiceBusQueue();
    }
}
```

### Data Isolation Strategies

**Strategy 1: Unique Test Run ID**
```csharp
private readonly string _testRunId = Guid.NewGuid().ToString();

var order = new OrderBuilder()
    .WithId($"ORDER-{_testRunId}-001")
    .Build();
```

**Strategy 2: Test-Specific Resources**
```csharp
// Use test-specific queue names
var queueName = $"orders-{_testRunId}";
await CreateQueue(queueName);
```

**Strategy 3: Timestamp-Based Data**
```csharp
var order = new OrderBuilder()
    .WithId($"ORDER-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}")
    .Build();
```

### Test Configuration

**appsettings.test.json:**
```json
{
  "ServiceBus": {
    "ConnectionString": "Endpoint=sb://test-namespace.servicebus.windows.net/;...",
    "QueueName": "orders-queue"
  },
  "Storage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;AccountName=teststorage;...",
    "ContainerName": "test-container"
  },
  "CosmosDb": {
    "Endpoint": "https://test-cosmos.documents.azure.com:443/",
    "DatabaseName": "TestDatabase",
    "ContainerName": "Orders"
  },
  "TestSettings": {
    "TimeoutSeconds": 30,
    "RetryAttempts": 3,
    "CleanupEnabled": true
  }
}
```

## Integration Test Examples (Phase 2)

### Example 1: Service Bus End-to-End Test

```csharp
[Collection("IntegrationTests")]
public class OrderProcessingIntegrationTests : IAsyncLifetime
{
    private ServiceBusClient _serviceBusClient;
    private ServiceBusSender _sender;
    private ServiceBusReceiver _receiver;
    private readonly IConfiguration _configuration;
    private readonly string _testRunId;

    public OrderProcessingIntegrationTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();
        
        _testRunId = Guid.NewGuid().ToString();
    }

    public async Task InitializeAsync()
    {
        var connectionString = _configuration["ServiceBus:ConnectionString"];
        _serviceBusClient = new ServiceBusClient(connectionString);
        _sender = _serviceBusClient.CreateSender("orders-queue");
        _receiver = _serviceBusClient.CreateReceiver("orders-processed-queue");
    }

    [Fact]
    public async Task ProcessOrder_EndToEnd_MessageProcessedSuccessfully()
    {
        // Arrange
        var orderId = $"ORDER-{_testRunId}-001";
        var orderMessage = new ServiceBusMessage(
            JsonSerializer.Serialize(new 
            { 
                Id = orderId,
                CustomerId = "C-123",
                Amount = 250.00,
                TestRunId = _testRunId
            })
        );
        orderMessage.ApplicationProperties["TestRunId"] = _testRunId;

        // Act - Send message to trigger Function
        await _sender.SendMessageAsync(orderMessage);

        // Assert - Wait for processed message
        var processedMessage = await _receiver.ReceiveMessageAsync(
            maxWaitTime: TimeSpan.FromSeconds(30)
        );

        processedMessage.Should().NotBeNull();
        
        var result = JsonSerializer.Deserialize<ProcessedOrder>(
            processedMessage.Body.ToString()
        );
        
        result.OrderId.Should().Be(orderId);
        result.Status.Should().Be("Processed");
        
        // Cleanup
        await _receiver.CompleteMessageAsync(processedMessage);
    }

    [Fact]
    public async Task ProcessOrder_WithInvalidData_SendsToDeadLetter()
    {
        // Arrange
        var invalidMessage = new ServiceBusMessage("Invalid JSON");
        invalidMessage.ApplicationProperties["TestRunId"] = _testRunId;

        // Act
        await _sender.SendMessageAsync(invalidMessage);
        await Task.Delay(5000); // Wait for processing

        // Assert - Check dead-letter queue
        var deadLetterReceiver = _serviceBusClient.CreateReceiver(
            "orders-queue",
            new ServiceBusReceiverOptions 
            { 
                SubQueue = SubQueue.DeadLetter 
            }
        );

        var deadLetterMessage = await deadLetterReceiver.ReceiveMessageAsync(
            maxWaitTime: TimeSpan.FromSeconds(10)
        );

        deadLetterMessage.Should().NotBeNull();
        deadLetterMessage.DeadLetterReason.Should().Contain("Deserialization");
        
        await deadLetterReceiver.CompleteMessageAsync(deadLetterMessage);
    }

    public async Task DisposeAsync()
    {
        await _sender.DisposeAsync();
        await _receiver.DisposeAsync();
        await _serviceBusClient.DisposeAsync();
    }
}
```

### Example 2: HTTP API Integration Test

```csharp
public class OrderApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public OrderApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetOrder_WithValidId_ReturnsOrder()
    {
        // Arrange - Create test order first
        var createResponse = await _client.PostAsJsonAsync("/api/orders", new
        {
            CustomerId = "C-123",
            Amount = 99.99m
        });

        var createdOrder = await createResponse.Content.ReadFromJsonAsync<Order>();

        // Act
        var getResponse = await _client.GetAsync($"/api/orders/{createdOrder.Id}");

        // Assert
        getResponse.IsSuccessStatusCode.Should().BeTrue();
        var order = await getResponse.Content.ReadFromJsonAsync<Order>();
        order.Id.Should().Be(createdOrder.Id);
        order.CustomerId.Should().Be("C-123");
    }

    [Fact]
    public async Task CreateOrder_WithValidData_Returns201Created()
    {
        // Arrange
        var orderRequest = new
        {
            CustomerId = "C-456",
            Amount = 149.99m,
            Items = new[]
            {
                new { ProductId = "P-1", Quantity = 2, Price = 49.99m },
                new { ProductId = "P-2", Quantity = 1, Price = 50.01m }
            }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", orderRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
        
        var order = await response.Content.ReadFromJsonAsync<Order>();
        order.CustomerId.Should().Be("C-456");
        order.Amount.Should().Be(149.99m);
    }
}
```

### Example 3: Blob Storage Integration Test

```csharp
public class BlobStorageIntegrationTests : IAsyncLifetime
{
    private BlobServiceClient _blobServiceClient;
    private BlobContainerClient _containerClient;
    private readonly string _testContainerName;

    public BlobStorageIntegrationTests()
    {
        _testContainerName = $"test-{Guid.NewGuid()}";
    }

    public async Task InitializeAsync()
    {
        var connectionString = _configuration["Storage:ConnectionString"];
        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = await _blobServiceClient.CreateBlobContainerAsync(_testContainerName);
    }

    [Fact]
    public async Task UploadBlob_ThenDownload_ContentsMatch()
    {
        // Arrange
        var blobName = $"test-{Guid.NewGuid()}.txt";
        var testContent = "This is test content";
        var blobClient = _containerClient.GetBlobClient(blobName);

        // Act - Upload
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(testContent)))
        {
            await blobClient.UploadAsync(stream, overwrite: true);
        }

        // Act - Download
        var downloadResponse = await blobClient.DownloadContentAsync();
        var downloadedContent = downloadResponse.Value.Content.ToString();

        // Assert
        downloadedContent.Should().Be(testContent);
    }

    public async Task DisposeAsync()
    {
        await _containerClient.DeleteAsync();
    }
}
```

## CI/CD Integration (Phase 2)

### Integration Test Pipeline Requirements

**Trigger:** After deployment to test environment

**Execution Time:** < 15 minutes total

**Prerequisites:**
- Unit tests passed
- Deployment successful
- Test environment ready

**Quality Gates:**
- ✅ All integration tests must pass
- ✅ Performance baseline not degraded (< 10% regression)
- ✅ Contract tests validate API/message schemas
- ✅ Smoke tests verify critical paths

**Environment:** Dedicated test environment (separate from dev/prod)

### Azure Pipelines Example

```yaml
# pipelines/ci-integration-tests.yml
trigger: none # Triggered after deployment

pool:
  vmImage: 'ubuntu-latest'

parameters:
  - name: environment
    displayName: 'Target Environment'
    type: string
    default: 'test'
    values:
      - dev
      - test
      - staging

variables:
  - group: 'integration-test-secrets-${{ parameters.environment }}'
  - name: testTimeout
    value: 900 # 15 minutes

stages:
  - stage: IntegrationTest
    displayName: 'Integration Tests - ${{ parameters.environment }}'
    jobs:
      - job: RunIntegrationTests
        displayName: 'Run Integration Tests'
        timeoutInMinutes: 20
        steps:
          - task: UseDotNet@2
            displayName: 'Install .NET SDK'
            inputs:
              version: '8.x'

          - task: AzureCLI@2
            displayName: 'Setup Test Environment'
            inputs:
              azureSubscription: 'Azure-ServiceConnection-${{ parameters.environment }}'
              scriptType: 'bash'
              scriptLocation: 'inlineScript'
              inlineScript: |
                echo "Verifying test resources..."
                az servicebus namespace show --name sb-test-${{ parameters.environment }} --resource-group rg-test

          - task: DotNetCoreCLI@2
            displayName: 'Run Integration Tests'
            inputs:
              command: 'test'
              projects: '**/tests/integration/**/*.csproj'
              arguments: >
                --configuration Release
                --logger trx
                --results-directory $(Agent.TempDirectory)/IntegrationTestResults
            env:
              ServiceBus__ConnectionString: $(ServiceBusConnectionString)
              Storage__ConnectionString: $(StorageConnectionString)
              Environment: ${{ parameters.environment }}

          - task: PublishTestResults@2
            displayName: 'Publish Integration Test Results'
            condition: succeededOrFailed()
            inputs:
              testResultsFormat: 'VSTest'
              testResultsFiles: '$(Agent.TempDirectory)/IntegrationTestResults/**/*.trx'
              mergeTestResults: true
              failTaskOnFailedTests: true
              testRunTitle: 'Integration Tests - ${{ parameters.environment }}'

          - task: AzureCLI@2
            displayName: 'Cleanup Test Data'
            condition: always()
            inputs:
              azureSubscription: 'Azure-ServiceConnection-${{ parameters.environment }}'
              scriptType: 'bash'
              scriptLocation: 'inlineScript'
              inlineScript: |
                echo "Cleaning up test data..."
                # Add cleanup scripts here
```

### GitHub Actions Example

```yaml
# .github/workflows/integration-tests.yml
name: Integration Tests

on:
  workflow_dispatch:
    inputs:
      environment:
        description: 'Target environment'
        required: true
        default: 'test'
        type: choice
        options:
          - dev
          - test

jobs:
  integration-tests:
    runs-on: ubuntu-latest
    environment: ${{ github.event.inputs.environment }}
    
    steps:
      - uses: actions/checkout@v4

      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      - name: Azure Login
        uses: azure/login@v1
        with:
          creds: ${{ secrets.AZURE_CREDENTIALS }}

      - name: Run Integration Tests
        run: |
          dotnet test tests/integration/**/*.csproj \
            --configuration Release \
            --logger "trx" \
            --results-directory ./test-results
        env:
          ServiceBus__ConnectionString: ${{ secrets.SERVICEBUS_CONNECTION_STRING }}
          Storage__ConnectionString: ${{ secrets.STORAGE_CONNECTION_STRING }}

      - name: Publish Test Results
        uses: EnricoMi/publish-unit-test-result-action@v2
        if: always()
        with:
          files: './test-results/**/*.trx'

      - name: Cleanup Test Data
        if: always()
        run: |
          # Add cleanup commands
          echo "Cleaning up test data..."
```

## Flaky Test Handling (Phase 2)

### What are Flaky Tests?

Tests that sometimes pass and sometimes fail without code changes.

### Common Causes

- Race conditions
- Timing issues
- External dependency failures
- Test data conflicts
- Environment instability

### Detection

```csharp
// Mark potentially flaky tests
[Trait("Flaky", "Investigation")]
[Fact]
public async Task PotentiallyFlakyTest()
{
    // Test implementation
}
```

### Mitigation Strategies

**1. Retry Logic (Use Sparingly):**
```csharp
public static class RetryHelper
{
    public static async Task<T> RetryAsync<T>(
        Func<Task<T>> action, 
        int maxAttempts = 3,
        TimeSpan delay = default)
    {
        var attempts = 0;
        while (true)
        {
            try
            {
                return await action();
            }
            catch when (attempts < maxAttempts - 1)
            {
                attempts++;
                await Task.Delay(delay == default ? TimeSpan.FromSeconds(1) : delay);
            }
        }
    }
}

// Usage
[Fact]
public async Task ProcessMessage_WithRetry()
{
    var result = await RetryHelper.RetryAsync(
        async () => await _service.ProcessMessage(message),
        maxAttempts: 3
    );
}
```

**2. Increase Timeouts:**
```csharp
// Instead of
await ReceiveMessageAsync(maxWaitTime: TimeSpan.FromSeconds(5));

// Use
await ReceiveMessageAsync(maxWaitTime: TimeSpan.FromSeconds(30));
```

**3. Add Explicit Waits:**
```csharp
// Wait for eventual consistency
await Task.Delay(TimeSpan.FromSeconds(2));
```

**4. Improve Test Isolation:**
```csharp
// Always use unique IDs
var testId = Guid.NewGuid().ToString();
```

### Tracking Flaky Tests

Keep a registry of flaky tests and their status:

```markdown
## Flaky Test Registry

| Test Name | Flaky Rate | Status | Owner | Notes |
|-----------|-----------|--------|-------|-------|
| ProcessOrder_EndToEnd | 3% | Under Investigation | @dev1 | Timing issue |
| SendMessage_ToQueue | 1% | Quarantined | @dev2 | Environment issue |
```

**Target:** < 5% flaky test rate

## Environment Isolation (Phase 2)

### Test Environment Requirements

- **Dedicated Test Environment:**
  - Separate from dev and production
  - Mirrors production architecture
  - Isolated resources (Service Bus, Storage, CosmosDB)

- **Resource Isolation:**
  - Test-specific resource groups
  - Test-specific service instances
  - Separate from production data

- **Configuration:**
  - Environment-specific connection strings
  - Test-specific settings
  - Secrets in Key Vault

### Example Environment Setup

```bash
# Create test resource group
az group create \
  --name rg-integration-tests \
  --location eastus

# Create test Service Bus namespace
az servicebus namespace create \
  --name sb-test-integration \
  --resource-group rg-integration-tests \
  --sku Standard

# Create test queues
az servicebus queue create \
  --name orders-queue \
  --namespace-name sb-test-integration \
  --resource-group rg-integration-tests
```

## Phase 2 Success Criteria

### Entry Criteria
- [ ] Phase 1 exit criteria met
- [ ] Unit test coverage ≥ 80%
- [ ] Test environment infrastructure available
- [ ] Team has capacity to write integration tests

### Success Criteria (During Phase)
- ✅ All integration points have integration tests
- ✅ Contract tests cover all API/message interfaces
- ✅ Integration tests run post-deployment
- ✅ Test data management automated
- ✅ Performance baselines established
- ✅ < 5% flaky test rate

### Exit Criteria (Ready for Phase 3)
- [ ] Integration tests cover all critical paths
- [ ] Contract tests prevent breaking changes
- [ ] Integration test execution time < 15 minutes
- [ ] Automated test data setup/teardown
- [ ] Performance baseline metrics tracked
- [ ] < 5% flaky tests for 2 months
- [ ] Test environment stable and reliable

## Definition of Done (Phase 2)

Every PR that affects integration points must meet:

- [ ] All integration points have integration tests
- [ ] Contract tests validate API/message schemas
- [ ] Integration tests pass in test environment
- [ ] Performance baselines established and met
- [ ] Test data cleanup automated (if needed)
- [ ] No manual intervention required
- [ ] Tests follow naming conventions (see [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md))
- [ ] Test configuration externalized
- [ ] Secrets managed securely

## Best Practices (Phase 2)

### 1. Use Real Azure Services (When Possible)

```csharp
// ✅ Good - Use real Service Bus in test environment
var connectionString = _configuration["ServiceBus:ConnectionString"];
var client = new ServiceBusClient(connectionString);

// ⚠️ Acceptable - Use Azurite for Storage tests
var connectionString = "UseDevelopmentStorage=true";
```

### 2. Isolate Test Data

```csharp
// ✅ Good - Unique test run ID
var testRunId = Guid.NewGuid().ToString();
var orderId = $"ORDER-{testRunId}-001";

// ❌ Bad - Hardcoded IDs
var orderId = "ORDER-123"; // Will conflict with other tests
```

### 3. Clean Up Resources

```csharp
// ✅ Good - Implement IAsyncLifetime for cleanup
public async Task DisposeAsync()
{
    foreach (var resource in _createdResources)
    {
        await DeleteResource(resource);
    }
}
```

### 4. Handle Timeouts Appropriately

```csharp
// ✅ Good - Reasonable timeout for async operations
var message = await receiver.ReceiveMessageAsync(
    maxWaitTime: TimeSpan.FromSeconds(30)
);

// ❌ Bad - Too short, will cause flaky tests
var message = await receiver.ReceiveMessageAsync(
    maxWaitTime: TimeSpan.FromSeconds(1)
);
```

### 5. Test Error Scenarios

```csharp
// ✅ Test both success and failure paths
[Fact]
public async Task ProcessMessage_WithValidData_Succeeds() { }

[Fact]
public async Task ProcessMessage_WithInvalidData_SendsToDeadLetter() { }

[Fact]
public async Task ProcessMessage_WhenServiceDown_Retries() { }
```

## Common Pitfalls (Phase 2)

| Pitfall | Impact | Solution |
|---------|--------|----------|
| **Flaky integration tests** | Pipeline unreliability | Isolate test data, increase timeouts, retry logic |
| **Not cleaning up test data** | Resource bloat, cost | Automate cleanup in DisposeAsync |
| **Slow test suites** | Reduced productivity | Parallelize, optimize, focus on critical paths |
| **Hardcoded connection strings** | Security risk | Use configuration, Key Vault |
| **Testing in production** | Risk to prod data | Use dedicated test environment |
| **Ignoring performance** | Regression undetected | Establish and track baselines |
| **Missing contract tests** | Breaking changes deployed | Add contract validation for all APIs |

## Next Steps

After completing Phase 2:

1. **Validate Exit Criteria:** Ensure all exit criteria are met
2. **Stabilize Flaky Tests:** Address any remaining flaky tests
3. **Optimize Performance:** Ensure integration tests run < 15 min
4. **Document Patterns:** Capture team-specific patterns
5. **Plan Phase 3 (Optional):** Consider advanced testing (E2E automation, chaos, security)
6. **Continuous Improvement:** Regular review and refinement

## Related Documentation

- [AUTOMATION_TESTING_STANDARD.md](AUTOMATION_TESTING_STANDARD.md) - Complete testing standard
- [NAMING_CONVENTIONS.md](NAMING_CONVENTIONS.md) - Test naming patterns
- [PHASE_1_UNIT_TESTING.md](PHASE_1_UNIT_TESTING.md) - Previous phase requirements
- [QUICK_START.md](QUICK_START.md) - Getting started guide
