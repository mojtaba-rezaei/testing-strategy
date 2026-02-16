# Architecture Documentation

This folder contains all architecture-related documentation for the Azure Integration Platform testing strategy.

## Purpose

The `architecture/` folder centralizes core architecture documents, standards, and implementation guides that define the testing strategy framework.

## Structure

```
architecture/
└── docs/                                   # Architecture documentation
    ├── AUTOMATION_TESTING_STANDARD.md      # Core principles and architecture ⭐
    ├── NAMING_CONVENTIONS.md               # Naming standards and patterns
    ├── PHASE_1_UNIT_TESTING.md             # Phase 1 complete guide
    ├── PHASE_2_INTEGRATION_TESTING.md      # Phase 2 complete guide
    ├── PLAN.md                             # Implementation plan
    ├── MATURITY_ASSESSMENT.md              # Self-assessment checklist
    └── SUMMARY.md                          # Complete implementation summary
```

## Documents Overview

### Core Standard
- **[AUTOMATION_TESTING_STANDARD.md](docs/AUTOMATION_TESTING_STANDARD.md)** ⭐  
  The single source of truth for testing standards. Defines core principles, architecture patterns, and quality gates.

### Implementation Guides
- **[PHASE_1_UNIT_TESTING.md](docs/PHASE_1_UNIT_TESTING.md)**  
  Complete guide for Phase 1: Unit Testing implementation (60% of test pyramid).

- **[PHASE_2_INTEGRATION_TESTING.md](docs/PHASE_2_INTEGRATION_TESTING.md)**  
  Complete guide for Phase 2: Integration Testing implementation (30% of test pyramid).

### Standards & Conventions
- **[NAMING_CONVENTIONS.md](docs/NAMING_CONVENTIONS.md)**  
  All naming standards for test projects, classes, methods, and builders.

### Planning & Assessment
- **[PLAN.md](docs/PLAN.md)**  
  Rollout strategy and implementation timeline.

- **[MATURITY_ASSESSMENT.md](docs/MATURITY_ASSESSMENT.md)**  
  Self-assessment checklist to evaluate testing maturity.

- **[SUMMARY.md](docs/SUMMARY.md)**  
  Complete implementation summary of the testing strategy.

## How to Use

### For New Teams
1. Start with [AUTOMATION_TESTING_STANDARD.md](docs/AUTOMATION_TESTING_STANDARD.md) to understand the architecture
2. Use [MATURITY_ASSESSMENT.md](docs/MATURITY_ASSESSMENT.md) to baseline current state
3. Follow [PLAN.md](docs/PLAN.md) for phased adoption
4. Implement [PHASE_1_UNIT_TESTING.md](docs/PHASE_1_UNIT_TESTING.md) first

### For Existing Teams
1. Review [MATURITY_ASSESSMENT.md](docs/MATURITY_ASSESSMENT.md) to identify gaps
2. Reference [NAMING_CONVENTIONS.md](docs/NAMING_CONVENTIONS.md) for consistency
3. Progress through phases per the [PLAN.md](docs/PLAN.md)

### For Architects
- Use these documents as templates for other domains
- Reference [AUTOMATION_TESTING_STANDARD.md](docs/AUTOMATION_TESTING_STANDARD.md) for architecture patterns
- Adapt [NAMING_CONVENTIONS.md](docs/NAMING_CONVENTIONS.md) to your context

## Key Principles

1. **60-30-10 Test Pyramid**  
   - 60% Unit Tests (Phase 1)
   - 30% Integration Tests (Phase 2)
   - 10% E2E Tests (Manual, Phase 3)

2. **Phased Adoption**  
   Start simple with unit tests, mature to integration tests over time.

3. **CI/CD Driven**  
   All automated tests must run in CI/CD pipelines with quality gates.

4. **Azure-Centric**  
   Tailored for Azure Integration Platform (Functions, Logic Apps, Service Bus, etc.)

## Related Resources

- **[Root README](../README.md)** - Quick start and navigation
- **[AI Tools](../.ai/)** - AI-powered test generation
- **[Sample Projects](../samples/)** - Working code examples
- **[CI/CD Pipelines](../samples/pipelines/)** - Azure DevOps examples
- **[GitHub Workflows](../samples/.github/workflows/)** - GitHub Actions examples

## Maintenance

- **Last Updated:** February 16, 2026
- **Maintained By:** Azure Integration Platform Team
- **Review Frequency:** Quarterly or when standards evolve

## Support

For questions about these architecture documents:
- Contact the Azure Integration Platform team
- Open an issue in the repository
- Review the [SUMMARY.md](docs/SUMMARY.md) for comprehensive overview

---

**Note:** These documents define the **what** and **why** of the testing strategy. For **how-to** guides, see the [Quick Start Guide](../QUICK_START.md).
