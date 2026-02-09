# Testing Strategy Visual Guides

This document provides visual representations of our testing strategy, architecture, and implementation patterns.

## Table of Contents
1. [Test Pyramid Architecture](#test-pyramid-architecture)
2. [Testing Strategy by Phase](#testing-strategy-by-phase)
3. [CI/CD Pipeline Flow](#cicd-pipeline-flow)
4. [Azure Service Testing Strategy](#azure-service-testing-strategy)
5. [Test Execution Time Distribution](#test-execution-time-distribution)
6. [Testing Maturity Journey](#testing-maturity-journey)
7. [Test Type Decision Tree](#test-type-decision-tree)

---

## Test Pyramid Architecture

The foundational principle: **60% Unit - 30% Integration - 10% E2E (Manual)**

```mermaid
graph TB
    subgraph Test Pyramid - 60/30/10 Distribution
    E2E[E2E Tests - Manual<br/>10% - ~100 tests<br/>Slowest, Most Expensive<br/>Business Flows Only]
    INT[Integration Tests<br/>30% - ~300 tests<br/>Medium Speed<br/>Azure Service Interactions]
    UNIT[Unit Tests<br/>60% - ~600 tests<br/>Fast, Isolated<br/>Business Logic]
    end
    
    E2E --> INT
    INT --> UNIT
    
    style E2E fill:#ff6b6b,stroke:#c92a2a,stroke-width:2px,color:#fff
    style INT fill:#4ecdc4,stroke:#0a9396,stroke-width:2px,color:#fff
    style UNIT fill:#95e1d3,stroke:#38a169,stroke-width:3px,color:#000
```

**Key Principles:**
- **Unit Tests (60%):** Foundation - fast, cheap, high coverage
- **Integration Tests (30%):** Confidence - realistic, Azure services
- **E2E Tests (10%):** Strategic - manual, critical flows only

---

## Testing Strategy by Phase

Our incremental adoption approach across three phases:

```mermaid
graph LR
    subgraph Phase 1: Foundation - 2-3 months
    P1[Unit Tests Only<br/>60% Coverage<br/>Fast Feedback<br/>Low Cost]
    end
    
    subgraph Phase 2: Maturity - 3-6 months
    P2[Unit + Integration<br/>90% Total Coverage<br/>Azure Services<br/>Contract Testing]
    end
    
    subgraph Phase 3: Continuous
    P3[Complete Strategy<br/>60/30/10 Distribution<br/>Manual E2E<br/>Full Automation]
    end
    
    P1 -->|Exit Criteria Met| P2
    P2 -->|Fully Adopted| P3
    
    style P1 fill:#e3f2fd,stroke:#1976d2,stroke-width:2px
    style P2 fill:#f3e5f5,stroke:#7b1fa2,stroke-width:2px
    style P3 fill:#e8f5e9,stroke:#388e3c,stroke-width:2px
```

---

## CI/CD Pipeline Flow

Complete pipeline with quality gates at each level:

```mermaid
flowchart TD
    START([Code Commit]) --> BUILD[Build Solution]
    BUILD --> UNIT{Unit Tests<br/>60%}
    
    UNIT -->|Pass| COV{Code Coverage<br/>≥ 80%?}
    UNIT -->|Fail| FAIL1[❌ Block PR/Deploy]
    
    COV -->|Yes| DEPLOY[Deploy to Test Env]
    COV -->|No| FAIL2[❌ Block PR/Deploy]
    
    DEPLOY --> INT{Integration Tests<br/>30%}
    
    INT -->|Pass| CONTRACT{Contract Tests<br/>Valid?}
    INT -->|Fail| ROLLBACK[Rollback Deployment]
    
    CONTRACT -->|Pass| PERF{Performance<br/>Baseline Met?}
    CONTRACT -->|Fail| ROLLBACK
    
    PERF -->|Pass| SMOKE[Smoke Tests]
    PERF -->|Fail| ALERT[Alert Team]
    
    SMOKE -->|Pass| APPROVE{Manual<br/>Approval}
    SMOKE -->|Fail| ROLLBACK
    
    APPROVE -->|Approved| PROD[Deploy to Production]
    APPROVE -->|Rejected| STOP([Stop Pipeline])
    
    PROD --> E2E[Manual E2E Testing<br/>10%<br/>In Production]
    E2E --> SUCCESS([✅ Complete])
    
    style UNIT fill:#95e1d3,stroke:#38a169,stroke-width:2px
    style INT fill:#4ecdc4,stroke:#0a9396,stroke-width:2px
    style E2E fill:#ff6b6b,stroke:#c92a2a,stroke-width:2px,color:#fff
    style FAIL1 fill:#ff6b6b,stroke:#c92a2a,stroke-width:2px,color:#fff
    style FAIL2 fill:#ff6b6b,stroke:#c92a2a,stroke-width:2px,color:#fff
    style SUCCESS fill:#51cf66,stroke:#2f9e44,stroke-width:2px
```

---

## Azure Service Testing Strategy

Testing approach for key Azure services:

```mermaid
graph TB
    subgraph Azure Functions
    AF_UNIT[Unit Tests 60%<br/>- Business Logic<br/>- Validators<br/>- Transformers<br/>- Mocked Triggers]
    AF_INT[Integration Tests 30%<br/>- Real Triggers<br/>- Bindings<br/>- DI Container<br/>- Service Bus]
    AF_E2E[Manual E2E 10%<br/>- Azure Portal<br/>- End-to-End Flow<br/>- Production-like]
    end
    
    subgraph Logic Apps
    LA_UNIT[Unit Tests 60%<br/>- Inline Code<br/>- Expressions<br/>- Custom Connectors]
    LA_INT[Integration Tests 30%<br/>- Workflow Execution<br/>- Built-in Connectors<br/>- Managed Connectors]
    LA_E2E[Manual E2E 10%<br/>- Designer Testing<br/>- Run History<br/>- Monitoring]
    end
    
    subgraph Service Bus
    SB_UNIT[Unit Tests 60%<br/>- Message Handlers<br/>- Serialization<br/>- Validation]
    SB_INT[Integration Tests 30%<br/>- Queue Operations<br/>- Topics/Subscriptions<br/>- Dead Letter<br/>- Sessions]
    SB_E2E[Manual E2E 10%<br/>- Message Flow<br/>- Ordering<br/>- Transactions]
    end
    
    AF_UNIT -.->|Foundation| AF_INT
    AF_INT -.->|Confidence| AF_E2E
    
    LA_UNIT -.->|Foundation| LA_INT
    LA_INT -.->|Confidence| LA_E2E
    
    SB_UNIT -.->|Foundation| SB_INT
    SB_INT -.->|Confidence| SB_E2E
    
    style AF_UNIT fill:#95e1d3,stroke:#38a169,stroke-width:2px
    style LA_UNIT fill:#95e1d3,stroke:#38a169,stroke-width:2px
    style SB_UNIT fill:#95e1d3,stroke:#38a169,stroke-width:2px
    
    style AF_INT fill:#4ecdc4,stroke:#0a9396,stroke-width:2px
    style LA_INT fill:#4ecdc4,stroke:#0a9396,stroke-width:2px
    style SB_INT fill:#4ecdc4,stroke:#0a9396,stroke-width:2px
    
    style AF_E2E fill:#ff6b6b,stroke:#c92a2a,stroke-width:2px,color:#fff
    style LA_E2E fill:#ff6b6b,stroke:#c92a2a,stroke-width:2px,color:#fff
    style SB_E2E fill:#ff6b6b,stroke:#c92a2a,stroke-width:2px,color:#fff
```

---

## Test Execution Time Distribution

Pipeline execution time allocation:

```mermaid
pie title Test Execution Time Budget (Minutes)
    "Unit Tests (5 min)" : 5
    "Integration Tests (15 min)" : 15
    "Manual E2E (Variable)" : 30
```

**Total Automated Pipeline Time:** ~20 minutes  
**Manual E2E Time:** Variable (performed outside pipeline)

---

## Testing Maturity Journey

Timeline for achieving full testing maturity:

```mermaid
timeline
    title Testing Maturity Evolution
    section Phase 1
        Month 1 : Setup Infrastructure
                : Configure CI/CD
                : First Unit Tests
        Month 2 : Expand Unit Coverage
                : Team Training
                : 50% Coverage
        Month 3 : 80% Coverage
                : Quality Gates
                : Ready for Phase 2
    section Phase 2
        Month 4 : Test Environment
                : First Integration Tests
                : Azure Services Setup
        Month 5 : Contract Testing
                : Performance Baselines
                : 20% Integration Coverage
        Month 6 : 30% Integration Coverage
                : CI/CD Integration
                : Flaky Test Management
    section Phase 3
        Month 7+ : Manual E2E Plans
                 : 60/30/10 Balance
                 : Continuous Improvement
                 : Metrics & Monitoring
```

---

## Test Type Decision Tree

Decision guide for choosing the right test type:

```mermaid
graph TD
    START{What are you<br/>testing?} -->|Single Function/Method| UNIT_Q{External<br/>Dependencies?}
    START -->|Multiple Components| INT_Q{Azure Services<br/>Involved?}
    START -->|Complete User Flow| E2E_Q{Can be<br/>Automated?}
    
    UNIT_Q -->|No| UNIT[✅ Unit Test<br/>Mock Dependencies<br/>Fast & Isolated]
    UNIT_Q -->|Yes| UNIT_MOCK[✅ Unit Test<br/>with Mocks<br/>Test Logic Only]
    
    INT_Q -->|Yes| INT_REAL[✅ Integration Test<br/>Real Azure Services<br/>Test Environment]
    INT_Q -->|No| INT_MOCK[⚠️ Consider Unit Test<br/>Or Integration with<br/>Local Emulators]
    
    E2E_Q -->|No| E2E_MANUAL[✅ Manual E2E<br/>Test Plan<br/>Azure Portal]
    E2E_Q -->|Yes| E2E_WARNING[⚠️ Reconsider<br/>E2E automation is<br/>expensive & fragile]
    
    E2E_WARNING --> E2E_MANUAL
    
    style UNIT fill:#95e1d3,stroke:#38a169,stroke-width:2px
    style UNIT_MOCK fill:#95e1d3,stroke:#38a169,stroke-width:2px
    style INT_REAL fill:#4ecdc4,stroke:#0a9396,stroke-width:2px
    style E2E_MANUAL fill:#ff6b6b,stroke:#c92a2a,stroke-width:2px,color:#fff
    style E2E_WARNING fill:#ffd43b,stroke:#f59f00,stroke-width:2px
```

---

## Quick Reference

### Color Legend
- 🟢 **Green (Unit Tests):** Fast, Isolated, Foundation
- 🔵 **Blue (Integration Tests):** Realistic, Azure Services, Confidence
- 🔴 **Red (E2E Tests):** Manual, Strategic, Critical Flows

### Key Metrics
| Metric | Target | Status Indicator |
|--------|--------|-----------------|
| Unit Test Coverage | ≥ 80% | 🟢 Good, 🟡 Warning < 70%, 🔴 Critical < 60% |
| Unit Test Distribution | 55-70% | 🟢 Compliant with 60% ± 10% |
| Integration Test Distribution | 25-35% | 🟢 Compliant with 30% ± 5% |
| E2E Test Distribution | 5-15% | 🟢 Compliant with 10% ± 5% |
| Unit Test Execution Time | < 5 min | 🟢 Fast, 🟡 Warning > 5 min |
| Integration Test Execution Time | < 15 min | 🟢 Acceptable, 🟡 Warning > 15 min |
| Flaky Test Rate | < 5% | 🟢 Stable, 🔴 Action Required > 10% |

---

## Usage

These diagrams are embedded in the [AUTOMATION_TESTING_STANDARD.md](AUTOMATION_TESTING_STANDARD.md) document. Use this file as a quick visual reference when:

- Explaining the testing strategy to new team members
- Making decisions about test types
- Planning test distribution for new features
- Setting up CI/CD pipelines
- Assessing project testing maturity

For complete details, refer to the main [AUTOMATION_TESTING_STANDARD.md](AUTOMATION_TESTING_STANDARD.md) document.

---

**Version:** 1.0.0  
**Last Updated:** February 9, 2026  
**Maintained by:** Architecture & Test Leads
