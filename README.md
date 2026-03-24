# Azure Integration Platform - Testing Strategy

This repository contains the standardized automation testing strategy for our Azure Integration Platform projects.

## Overview

This testing strategy defines a **phased, incremental approach** to automation testing across all integration projects. The strategy emphasizes starting simple with unit tests and maturing over time to include comprehensive integration testing.

### Key Principles

- **Incremental Adoption:** Start with unit tests, mature to integration tests
- **CI/CD Driven:** All tests run in automated pipelines
- **Azure-Centric:** Tailored for Azure Integration Platform (Logic Apps, Functions, Service Bus, etc.)
- **Quality Gates:** Enforced standards prevent quality degradation
- **Developer-Friendly:** Focus on fast feedback and easy adoption
- **Always Ask, Never Guess:** AI agents must ask for clarification when behavior is unclear
- **Comprehensive Coverage:** All testable components in the Function App must have unit tests
- **CSV-Driven Precision:** Mapper/converter tests use CSV mapping specifications for field-level assertions

## Documentation Structure

```
.
├── .ai/                                      # AI-generated content and prompts
│   ├── README.md                             # AI folder overview
│   ├── generation-log.md                     # AI generation history
│   └── prompts/
│       ├── testing-strategy-prompt.md        # Original strategy prompt
│       └── unit-test-generator.md            # ⭐ AI agent test generation guide
│
├── samples/
│   ├── OrderProcessingFunction/              # Azure Function sample (.NET 8)
│   ├── pipelines/                            # CI/CD pipeline examples
│   │   ├── README.md                         # Pipelines guide
│   │   ├── azure-pipelines-unit-tests.yml    # Azure Pipelines unit tests
│   │   └── azure-pipelines-integration-tests.yml  # Azure Pipelines integration
│   └── .github/
│       └── workflows/                            # GitHub Actions workflows
│           ├── README.md                         # Workflows guide
│           ├── unit-tests.yml                    # Unit test workflow
│           └── integration-tests.yml             # Integration test workflow
│
├── architecture/
│   └── docs/                                 # Architecture documentation
│       ├── AUTOMATION_TESTING_STANDARD.md    # Core principles and architecture
│       ├── NAMING_CONVENTIONS.md             # Naming standards and patterns
│       ├── PHASE_1_UNIT_TESTING.md           # Phase 1 complete guide
│       ├── PHASE_2_INTEGRATION_TESTING.md    # Phase 2 complete guide
│       ├── PLAN.md                           # Implementation plan
│       ├── MATURITY_ASSESSMENT.md            # Self-assessment checklist
│       └── SUMMARY.md                        # Complete implementation summary
│
├── README.md                                 # This file
└── QUICK_START.md                            # Getting started guide
```

## Quick Links

### Core Documentation
- **📘 [Testing Standard](architecture/docs/AUTOMATION_TESTING_STANDARD.md)** - Core principles and architecture
- **🚀 [Quick Start Guide](QUICK_START.md)** - Get started in 5 minutes
- **📋 [Implementation Plan](architecture/docs/PLAN.md)** - Rollout strategy
- **📈 [Maturity Assessment](architecture/docs/MATURITY_ASSESSMENT.md)** - Self-assessment tool

### Phase-Specific Guides
- **🔷 [Phase 1: Unit Testing](architecture/docs/PHASE_1_UNIT_TESTING.md)** - Complete unit testing guide
- **🔶 [Phase 2: Integration Testing](architecture/docs/PHASE_2_INTEGRATION_TESTING.md)** - Complete integration testing guide
- **📝 [Naming Conventions](architecture/docs/NAMING_CONVENTIONS.md)** - All naming standards

### AI & Automation
- **🤖 [AI Prompts & Tools](.ai/prompts/)** - AI agent instructions
- **⚡ [Unit Test Generator](.ai/prompts/unit-test-generator.md)** - AI-powered test generation guide
- **📜 [Generation Log](.ai/generation-log.md)** - AI content history

### CI/CD Examples
- **🔧 [GitHub Actions Workflows](samples/.github/workflows/)** - Unit + Integration workflows
- **🔧 [Azure Pipelines](samples/pipelines/)** - Unit + Integration pipelines
- **📂 [Sample Projects](samples/)** - Working code examples (.NET 8)

## Testing Maturity Phases

### Phase 1: Unit Testing (Start Here) ✅
- Fast, isolated unit tests
- 80% code coverage minimum
- CI pipeline integration
- ~2-3 months to establish

### Phase 2: Integration Testing (Mature)
- Component-to-component testing
- Contract validation
- Performance baselines
- ~3-6 months to establish

### Phase 3: Advanced (Optional)
- Load and performance testing
- Chaos engineering
- Security testing

## Getting Started

### For New Projects

1. **Read the documentation:**
   - Start with [QUICK_START.md](QUICK_START.md)
   - Review [AUTOMATION_TESTING_STANDARD.md](architecture/docs/AUTOMATION_TESTING_STANDARD.md)
   - Deep dive into [PHASE_1_UNIT_TESTING.md](architecture/docs/PHASE_1_UNIT_TESTING.md)

2. **Set up your project:**
   - Copy the [sample project structure](samples/OrderProcessingFunction/)
   - Apply [NAMING_CONVENTIONS.md](architecture/docs/NAMING_CONVENTIONS.md)
   - Follow the Test Builder pattern

3. **Set up CI/CD:**
   - **For GitHub:** Copy [samples/.github/workflows/unit-tests.yml](samples/.github/workflows/unit-tests.yml)
   - **For Azure DevOps:** Copy [samples/pipelines/azure-pipelines-unit-tests.yml](samples/pipelines/azure-pipelines-unit-tests.yml)
   - Configure secrets and variables as per README files

4. **Gather CSV mapping specifications (if applicable):**
   - If your project contains mappers/converters, locate CSV mapping spec files
   - These define source-to-target field mappings for precise test assertions
   - Place them in a `mapping-spec/` folder or provide paths to the AI agent

5. **Generate tests with AI:**
   - Use [.ai/prompts/unit-test-generator.md](.ai/prompts/unit-test-generator.md) with your AI assistant
   - Provide the prompt to ChatGPT, GitHub Copilot, or Claude
   - The AI agent will scan ALL testable components (not just what you point to)
   - It will ask for CSV mapping specs and clarifications — don't skip these prompts
   - Get deterministic, standards-compliant unit tests with verified passing status

6. **Achieve Phase 1 compliance:**
   - ≥80% code coverage (mandatory minimum, ≥90% for business logic)
   - All tests passing in CI
   - Follow naming conventions
   - All testable components covered

### For Existing Projects

1. **Assess current state:**
   - Complete [MATURITY_ASSESSMENT.md](architecture/docs/MATURITY_ASSESSMENT.md)
   - Identify gaps

2. **Plan adoption:**
   - Review [PLAN.md](architecture/docs/PLAN.md)
   - Prioritize high-risk components
   - Create sprint-level tasks

3. **Implement incrementally:**
   - Start with [PHASE_1_UNIT_TESTING.md](architecture/docs/PHASE_1_UNIT_TESTING.md)
   - Use [.ai/prompts/unit-test-generator.md](.ai/prompts/unit-test-generator.md) to speed up test creation
   - Set up CI/CD early (use the provided examples)

4. **Track progress:**
   - Monitor coverage in CI/CD dashboards
   - Review against Phase 1 exit criteria
   - Plan Phase 2 adoption

### Using AI to Generate Tests

**🤖 AI-Powered Test Generation**

This repository includes comprehensive AI agent instructions for generating standards-compliant unit tests:

1. **Open your AI assistant** (ChatGPT, GitHub Copilot Chat, Claude)
2. **Load the prompt:** Copy content from [.ai/prompts/unit-test-generator.md](.ai/prompts/unit-test-generator.md)
3. **Point to your project root:** The AI will automatically scan ALL testable components
4. **Provide CSV mapping specs** (if applicable): The AI will ask for these — they enable precise field-level mapper tests
5. **Answer clarification questions:** The AI will ask about unclear behavior instead of guessing
6. **Get production-ready tests:** The AI generates, builds, and verifies all tests pass

**What you get:**
- ✅ Correct naming (MethodName_Scenario_ExpectedBehavior)
- ✅ AAA pattern (Arrange-Act-Assert)
- ✅ Proper mocking (Moq) and assertions (FluentAssertions)
- ✅ Test Builder pattern
- ✅ ≥80% coverage (≥90% for business logic) — mandatory minimums, enforced
- ✅ CI/CD compatible tests
- ✅ CSV-driven field-level assertions for mapper/converter classes
- ✅ ALL testable components covered (comprehensive scan)
- ✅ Existing stubbed/TODO tests filled with real logic
- ✅ Build and test verified before completion

See [.ai/README.md](.ai/README.md) for more details.

## Scope

This standard applies to all projects using:

- ✅ Azure Functions
- ✅ Azure Logic Apps Standard
- ✅ Azure Data Factory

## Key Standards at a Glance

| Aspect | Standard |
|--------|----------|
| **Unit Test Coverage** | ≥ 80% per class (mandatory), ≥ 90% for business logic (mandatory) |
| **Test Execution Time** | Unit: < 5 min, Integration: < 15 min |
| **Folder Structure** | `/tests/unit`, `/tests/integration`, `/tests/shared` |
| **Naming Convention** | `<Component>.UnitTests`, `<Component>.IntegrationTests` |
| **Pipeline Gates** | All tests pass, coverage threshold met |
| **Definition of Done** | Tests written, reviewed, and passing in CI |

## Compliance

- ✅ All new projects must follow this standard
- ✅ Existing projects: adoption overtime
- ✅ Pipeline gates enforce standards automatically
