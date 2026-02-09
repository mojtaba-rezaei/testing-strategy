# Testing Maturity Self-Assessment

Use this checklist to assess your project's current testing maturity and track progress toward compliance with the automation testing standard.

## Instructions

1. Review each section honestly
2. Check items that are currently in place
3. Calculate your maturity score
4. Identify gaps and create action plan
5. Re-assess monthly

---

## Phase 1: Unit Testing Foundation

### Test Infrastructure (Weight: 20%)

- [ ] Unit test project created following naming convention (`<Component>.UnitTests`)
- [ ] Folder structure follows standard (`/tests/unit`, `/tests/integration`, `/tests/shared`)
- [ ] Test framework configured (xUnit, MSTest, or NUnit)
- [ ] Mocking library available (Moq, NSubstitute)
- [ ] Assertion library available (FluentAssertions recommended)
- [ ] Test data generation tools (AutoFixture, Bogus)

**Score:** ___ / 6

### Test Coverage (Weight: 30%)

- [ ] All new code has unit tests
- [ ] Code coverage ≥ 80% for business logic
- [ ] All public methods tested
- [ ] Critical paths and edge cases covered
- [ ] Error handling and validation tested
- [ ] No skipped/ignored tests without justification
- [ ] Coverage trending upward or stable

**Score:** ___ / 7

### CI/CD Integration (Weight: 25%)

- [ ] Unit tests run on every PR
- [ ] Unit tests run on every commit to main/develop
- [ ] Pipeline fails if tests fail
- [ ] Test results published to pipeline
- [ ] Code coverage reports generated
- [ ] Coverage threshold enforced (≥ 80%)
- [ ] Test execution time < 5 minutes
- [ ] No manual steps required

**Score:** ___ / 8

### Code Quality (Weight: 15%)

- [ ] Tests follow naming conventions
- [ ] Tests are isolated (no dependencies between tests)
- [ ] Tests are deterministic (always same result)
- [ ] Tests are fast (< 100ms per unit test)
- [ ] Appropriate use of mocks (not over-mocked)
- [ ] Test code is reviewed in PRs
- [ ] Test code is maintainable and readable

**Score:** ___ / 7

### Team Adoption (Weight: 10%)

- [ ] Team trained on unit testing practices
- [ ] Testing is part of Definition of Done
- [ ] Team writes tests before or with code
- [ ] Team comfortable with testing tools
- [ ] Testing standards referenced in README
- [ ] New team members onboarded on testing

**Score:** ___ / 6

---

### Phase 1 Total Score

**Total:** ___ / 34 = ___% 

**Maturity Level:**
- 🔴 0-49%: Not Compliant - Action Required
- 🟡 50-74%: Partially Compliant - Improvement Needed
- 🟢 75-89%: Mostly Compliant - Minor Gaps
- ✅ 90-100%: Fully Compliant - Ready for Phase 2

**Ready for Phase 2?** Score ≥ 75% + No critical gaps

---

## Phase 2: Integration Testing

### Test Infrastructure (Weight: 20%)

- [ ] Integration test project created (`<Component>.IntegrationTests`)
- [ ] Test environment available (dedicated Azure subscription/resource group)
- [ ] Integration test tools configured (Azure SDK, TestContainers, etc.)
- [ ] Test data management strategy defined
- [ ] Automated setup/teardown for test resources
- [ ] Environment isolation from production

**Score:** ___ / 6

### Integration Test Coverage (Weight: 30%)

- [ ] All integration points have tests
- [ ] Contract tests for all APIs/messages
- [ ] Real Azure resources used (Service Bus, Storage, etc.)
- [ ] Async scenarios tested
- [ ] Retry and error handling tested
- [ ] Performance baselines established
- [ ] Regression tests for critical paths
- [ ] Smoke tests for deployment validation

**Score:** ___ / 8

### CI/CD Integration (Weight: 25%)

- [ ] Integration tests run post-deployment
- [ ] Tests run in dedicated test environment
- [ ] Separate pipeline stages (unit → integration)
- [ ] Promotion gates based on test results
- [ ] Flaky tests identified and tracked
- [ ] Test execution time < 15 minutes
- [ ] Automated test data cleanup
- [ ] No manual intervention required

**Score:** ___ / 8

### Test Quality (Weight: 15%)

- [ ] Tests are isolated (unique test data per run)
- [ ] Tests clean up resources after execution
- [ ] Tests use realistic scenarios
- [ ] Contract tests validate schemas
- [ ] Performance tests track trends
- [ ] Tests are maintainable
- [ ] Minimal flaky tests (< 5%)

**Score:** ___ / 7

### Team Maturity (Weight: 10%)

- [ ] Team trained on integration testing
- [ ] Team understands when to use integration vs unit tests
- [ ] Integration tests part of Definition of Done
- [ ] Team can troubleshoot test failures
- [ ] Documentation updated

**Score:** ___ / 5

---

### Phase 2 Total Score

**Total:** ___ / 34 = ___% 

**Maturity Level:**
- 🔴 0-49%: Not Ready - Complete Phase 1 First
- 🟡 50-74%: In Progress - Continue Building
- 🟢 75-89%: Mature - Minor Improvements
- ✅ 90-100%: Advanced - Ready for Phase 3

---

## Governance Compliance

### Standards Adherence (Required)

- [ ] Testing standard referenced in README
- [ ] Naming conventions followed
- [ ] Folder structure matches standard
- [ ] PR template includes test checklist
- [ ] Definition of Done includes testing
- [ ] No PRs merged without passing tests
- [ ] Code reviews include test review

**Score:** ___ / 7

### Security & Compliance (Required)

- [ ] No secrets in test code
- [ ] Secrets managed via Key Vault/pipeline variables
- [ ] Test data is non-sensitive
- [ ] Test data complies with data protection regulations
- [ ] Test environments isolated from production
- [ ] Access controls on test environments

**Score:** ___ / 6

### Metrics & Monitoring (Recommended)

- [ ] Test metrics tracked (pass rate, execution time)
- [ ] Code coverage tracked over time
- [ ] Flaky tests identified and reported
- [ ] Test failures investigated promptly
- [ ] Regular test suite health reviews

**Score:** ___ / 5

---

### Governance Total Score

**Total:** ___ / 18 = ___% 

**Minimum Required:** 85% for full compliance

---

## Overall Assessment

| Category | Score | Weight | Weighted Score |
|----------|-------|--------|----------------|
| Phase 1: Unit Testing | ___% | 60% | ___% |
| Phase 2: Integration Testing | ___% | 30% | ___% |
| Governance & Compliance | ___% | 10% | ___% |
| **TOTAL** | | **100%** | **___%** |

---

## Action Plan Template

Based on your assessment, identify top 3-5 priority gaps:

### Priority 1: [Gap Description]
- **Current State:** 
- **Target State:** 
- **Actions Required:**
  1. 
  2. 
  3. 
- **Owner:** 
- **Target Date:** 
- **Status:** Not Started / In Progress / Complete

### Priority 2: [Gap Description]
- **Current State:** 
- **Target State:** 
- **Actions Required:**
  1. 
  2. 
- **Owner:** 
- **Target Date:** 
- **Status:** 

### Priority 3: [Gap Description]
- **Current State:** 
- **Target State:** 
- **Actions Required:**
  1. 
  2. 
- **Owner:** 
- **Target Date:** 
- **Status:** 

---

## Recommendations by Score

### If Overall Score < 50%
- Focus exclusively on Phase 1
- Start with basic unit test setup
- Get CI pipeline running
- Target one component/service at a time
- Schedule weekly check-ins with test leads

### If Overall Score 50-74%
- Continue Phase 1 improvements
- Focus on coverage gaps
- Improve test quality (speed, reliability)
- Address governance basics
- Monthly progress reviews

### If Overall Score 75-89%
- Complete Phase 1 exit criteria
- Begin Phase 2 planning
- Address specific gaps
- Share learnings with other teams
- Consider Phase 2 pilot

### If Overall Score ≥ 90%
- Maintain current quality
- Begin/continue Phase 2
- Mentor other teams
- Contribute to standards improvements
- Consider Phase 3 (advanced testing)

---

## Next Steps

1. **Schedule Review Meeting**
   - Review results with team
   - Discuss gaps and priorities
   - Create action plan

2. **Get Help**
   - Slack: #testing-standards
   - Office Hours: Thursdays 3-4 PM
   - Email: testing-standards@company.com

3. **Track Progress**
   - Re-assess monthly
   - Update action plan
   - Celebrate improvements

4. **Share Success**
   - Demo in Community of Practice
   - Write case study
   - Help other teams

---

## Assessment History

Track your progress over time:

| Date | Phase 1 | Phase 2 | Governance | Overall | Notes |
|------|---------|---------|------------|---------|-------|
| 2026-02 | ___% | ___% | ___% | ___% | Initial assessment |
| | | | | | |
| | | | | | |

---

**Questions about scoring or interpretation?** Contact testing-standards@company.com
