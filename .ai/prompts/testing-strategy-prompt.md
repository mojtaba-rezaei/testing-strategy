# INSTRUCTION.md
## Azure Integration Platform – Automation Testing Standardization Prompt

### Purpose
This instruction file contains a **complete, reusable prompt** intended for use with an AI assistant.

The goal is to help **plan, design, and evolve a company-wide automation testing standard** for an **integration company using the Azure Integration Platform**.

⚠️ **Important Principle**
The strategy must be designed with a **final enterprise-grade goal in mind**, but it must be **developed and adopted incrementally**, starting with **small, safe, low-cost steps** and growing over time.

The resulting output should be suitable for:
- Enterprise-scale integration teams
- Daily development and delivery workflows
- Long-term maintainability and scalability
- CI/CD-driven automation testing

---

## How to Use This File
1. Copy the **Prompt** section below.
2. Paste it into the AI system of your choice.
3. Use the AI’s output as:
   - A baseline testing strategy
   - Input to internal standards and governance
   - A foundation for DevOps and quality practices

---

## Prompt

You are acting as a **Senior Azure Integration Architect and Test Automation Lead** with deep experience in enterprise integration platforms, DevOps, and quality engineering.

I work in an **integration company** that builds and maintains solutions using the **Azure Integration Platform**, including but not limited to:
- Azure Logic Apps (Consumption & Standard)
- Azure Functions
- Azure Service Bus
- Azure Event Grid / Event Hubs
- Azure API Management
- Azure Data Factory
- Azure Storage
- Azure Monitor / Application Insights

---

## Core Objective (Final Goal)

Your **final goal** is to design a **complete, standardized automation testing strategy** that can be applied **consistently across all integration projects** and embedded into **daily engineering workflows**.

The strategy must:
- Be enterprise-ready
- Scale across multiple teams and repositories
- Be CI/CD-driven
- Balance quality, speed, cost, and maintainability

⚠️ **However:**
You must **not assume full maturity from day one**.

---

## Mandatory Development Approach (Very Important)

You must design the strategy using a **2-step evolutionary approach**, explicitly separating **initial adoption** from **later maturity**.

### Step 1 — Baby Steps (Mandatory Starting Point)
Start with a **minimal, practical foundation** focused on:

- Unit testing only
- Fast feedback
- Low setup complexity
- High developer adoption
- Clear value with minimal friction

This step must:
- Be achievable by small teams
- Be suitable for early CI pipelines
- Avoid heavy infrastructure dependencies
- Avoid complex end-to-end environments

### Step 2 — Mature Testing (Introduced Later)
Only **after Step 1 is stable and adopted**, evolve toward:

- Integration testing
- System-to-system validation
- Asynchronous and event-driven scenarios
- Higher environments and shared infrastructure

⚠️ You must **clearly explain what is intentionally deferred** in Step 1 and **why**.

---

## Required Deliverable: Testing Standardization File

You must explicitly design and describe a **single source of truth standardization file**, for example:

- `AUTOMATION_TESTING_STANDARD.md`
- `TESTING_STANDARDS.md`

This file must:
- Declare **all testing standards**
- Be referenced by all teams and repositories
- Act as a **governance and onboarding artifact**

The standardization file must define:
- Test types and when they apply
- Naming conventions
- Folder and repository structures
- Tooling choices
- CI/CD expectations
- Definition of Done for testing
- Minimum quality gates

You must:
- Propose its structure
- Explain its purpose
- Show how teams are expected to use it

---

## Required Coverage

### 1. Testing Scope and Test Types
Define what should be tested and how, explicitly split by:

- **Step 1 (Unit Testing First)**
- **Step 2 (Integration Testing Later)**

Cover:
- Unit testing for integration components
- Contract testing (APIs, schemas, message formats)
- Integration testing (system-to-system)
- End-to-end testing
- Regression and smoke testing
- Performance, load, and resiliency testing

Clearly state:
- What is required initially
- What is optional later
- What is intentionally excluded

---

### 2. Azure-Specific Testing Strategies
Explain how testing evolves for Azure-native components:

- Logic Apps
- Azure Functions
- Service Bus
- Event Grid / Event Hubs
- API Management
- Data Factory

For each service:
- What can realistically be unit-tested
- What must wait for integration testing
- How to handle async, retries, and failures over time

---

### 3. Tooling and Frameworks
Recommend tooling **by maturity level**:

- Tools for Step 1 (lightweight, fast, developer-friendly)
- Tools for Step 2 (more realistic, infrastructure-aware)

Clearly distinguish:
- Azure-native tools
- Open-source tools
- Commercial tools

Explain **why** each tool fits its phase.

---

### 4. CI/CD and Automation
Describe CI/CD integration in **phases**:

- Step 1 pipelines (unit tests as gates)
- Step 2 pipelines (integration tests post-deploy)

Include:
- Execution order
- Quality gates
- Promotion rules
- Handling flaky or environment-dependent tests

---

### 5. Standardization and Reusability
Define standards that teams must follow, including:
- Naming conventions
- Repository and folder structures
- Test project organization
- Shared libraries
- Reusable test templates

Explain how these standards:
- Start small
- Grow without breaking existing projects

---

### 6. Governance and Best Practices
Define governance rules such as:
- Definition of Done (by maturity step)
- Minimum test expectations
- Code review rules for tests
- Environment isolation
- Security and compliance
- Logging, monitoring, and observability of tests

---

### 7. Maturity Roadmap
Provide a **clear, phased roadmap**:

- Phase 1: Unit testing foundation (mandatory)
- Phase 2: Integration testing adoption
- Phase 3: Advanced testing and optimization

Explain:
- Entry criteria for each phase
- Common pitfalls
- How teams know they are ready to move forward

---

## Expected Output Format
Present the results as:
- A clear automation testing architecture
- A **defined Standardization File**
- Step-by-step workflows
- Example folder structures
- Example CI/CD stages
- Concrete, actionable guidance

Assume the audience consists of:
- Integration developers
- DevOps engineers
- Architects
- QA and test engineers

---

## Optional Enhancements (If Relevant)
Where helpful, include:
- Pseudo-code
- Sample test cases
- Example YAML pipelines
- Textual diagrams

Avoid unnecessary verbosity, but do not oversimplify.

---

## End of Prompt
