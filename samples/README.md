# Sample Projects

This directory contains complete, working sample projects demonstrating the automation testing standards for Azure Integration Platform projects.

## Available Samples

### 1. OrderProcessingFunction

**Technology:** Azure Functions (.NET 8 isolated worker)  
**Focus:** Phase 1 - Unit Testing  
**Location:** `OrderProcessingFunction/`

**Features:**
- HTTP-triggered Azure Functions
- Business services with dependency injection
- Comprehensive unit tests (80%+ coverage)
- Test builders and fixtures
- Mocking with Moq
- FluentAssertions

**What You'll Learn:**
- How to structure an Azure Function project for testability
- Unit testing Azure Functions business logic
- Using test builders for complex objects
- Mocking dependencies effectively
- Achieving high code coverage

[View Sample →](OrderProcessingFunction/)

---

## Using These Samples

### Build and Run All Samples

```bash
# From the samples directory
cd samples

# Restore all dependencies
dotnet restore

# Build all projects
dotnet build

# Run all unit tests
dotnet test --filter "FullyQualifiedName~UnitTests"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Learn from the Samples

1. **Start Here:** Read the `README.md` in each sample project
2. **Explore Code:** Review the project structure and code organization
3. **Study Tests:** Examine the test files to understand patterns
4. **Run Tests:** Execute tests locally to see them in action
5. **Modify:** Try changing code and tests to experiment

## Sample Project Structure

Each sample follows the standard structure:

```
SampleProject/
├── src/
│   └── [ProjectName]/
│       ├── Functions/
│       ├── Services/
│       ├── Models/
│       └── [ProjectName].csproj
├── tests/
│   └── unit/
│       └── [ProjectName].UnitTests/
│           ├── Services/
│           ├── Models/
│           ├── Builders/
│           └── [ProjectName].UnitTests.csproj
└── README.md
```

## Testing Standards Demonstrated

All samples demonstrate:
- ✅ Proper folder structure
- ✅ Naming conventions
- ✅ Test organization
- ✅ Dependency injection
- ✅ Unit testing best practices
- ✅ 80%+ code coverage
- ✅ Fast test execution
- ✅ Isolated tests (no external dependencies)

## Future Samples (Coming Soon)

### LogicAppWorkflow
- Logic Apps Standard with inline code
- Custom connectors
- Integration with Service Bus

### ApiManagementPolicies
- API Management custom policies
- Request/response transformations
- Policy testing strategies

### ServiceBusProcessor
- Service Bus message processing
- Retry and error handling
- Dead-letter queue handling

### DataFactoryPipeline
- Custom Data Factory activities
- Data transformation logic
- Pipeline orchestration

## Contributing New Samples

Want to add a sample project? Follow these guidelines:

1. **Follow the Standard Structure**
   - Use the same folder layout as existing samples
   - Follow naming conventions

2. **Include Complete Documentation**
   - Comprehensive README.md
   - Inline code comments
   - Testing strategy explanation

3. **Demonstrate Best Practices**
   - High test coverage (≥80%)
   - Multiple testing patterns
   - Real-world scenarios

4. **Keep It Simple**
   - Focus on one concept per sample
   - Easy to understand and run
   - Well-commented code

5. **Ensure It Works**
   - All tests pass
   - Builds without errors
   - Can run locally

## Getting Help

- 📚 [Automation Testing Standard](../AUTOMATION_TESTING_STANDARD.md)
- 🚀 [Quick Start Guide](../QUICK_START.md)
- 💬 Slack: #testing-standards
- 📧 Email: testing-standards@company.com

## Quick Reference

### Build a Specific Sample
```bash
cd samples/OrderProcessingFunction
dotnet build
```

### Test a Specific Sample
```bash
cd samples/OrderProcessingFunction
dotnet test tests/unit/OrderProcessor.UnitTests
```

### Add Sample to Your Project
```bash
# Copy the structure
cp -r samples/OrderProcessingFunction/tests your-project/

# Adapt to your needs
# Update namespaces, references, etc.
```

---

**Pro Tip:** Clone these samples as starting templates for your own projects!
