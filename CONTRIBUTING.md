# Contributing to the Automation Testing Standard

Thank you for your interest in improving our testing standards! This document outlines how to propose changes, updates, and enhancements to the automation testing standard.

## Who Can Contribute?

Anyone can propose changes:
- Developers
- Test engineers
- DevOps engineers
- Tech leads
- Architects

## Types of Contributions

### 1. Clarifications and Corrections
- Typos and grammar fixes
- Broken links
- Unclear explanations
- Missing information

### 2. Standards Updates
- New best practices
- Tool recommendations
- Process improvements
- Example additions

### 3. Major Changes
- New test types or phases
- Governance changes
- Compliance requirements
- Structural reorganization

## How to Propose Changes

### Small Changes (Clarifications, Typos)

1. **Create an Issue**
   - Navigate to the repository issues
   - Click "New Issue"
   - Select template: "Documentation Fix"
   - Describe the issue clearly
   - Add label: `documentation`

2. **Submit a Pull Request**
   - Fork the repository
   - Create a branch: `docs/fix-typo-in-section-X`
   - Make your changes
   - Submit PR with clear description
   - Link to related issue

**Approval:** Single approver from Test Leads team

### Medium Changes (Examples, Tool Updates)

1. **Create an Issue First**
   - Template: "Standards Enhancement"
   - Describe proposed change
   - Explain rationale and benefits
   - Add label: `enhancement`

2. **Discuss in Issue**
   - Wait for feedback from maintainers
   - Address questions and concerns
   - Refine proposal based on feedback

3. **Submit Pull Request**
   - Create branch: `feature/add-example-for-X`
   - Implement changes
   - Update relevant sections
   - Include examples if applicable
   - Submit PR linking to issue

**Approval:** Two approvers from Test Leads/Architecture team

### Major Changes (Standards, Governance)

1. **Write a Proposal Document**
   - Template: Use `/templates/proposal-template.md`
   - Include:
     - Problem statement
     - Proposed solution
     - Impact analysis (teams, projects, timelines)
     - Migration plan
     - Alternatives considered
     - Implementation cost/effort

2. **Submit for Review**
   - Create issue with label: `major-change`
   - Attach proposal document
   - Request Architecture Review Board (ARB) review

3. **Present to ARB**
   - Schedule presentation (monthly ARB meeting)
   - Present proposal (15-20 minutes)
   - Q&A and discussion
   - ARB votes on approval

4. **Implement if Approved**
   - Create implementation plan
   - Submit PR with changes
   - Update version history
   - Communicate changes to all teams

**Approval:** Architecture Review Board consensus

## Pull Request Guidelines

### PR Title Format

```
[Type] Brief description

Types:
- docs: Documentation changes
- feat: New features/standards
- fix: Corrections
- refactor: Reorganization
- chore: Maintenance
```

Examples:
- `docs: Fix typo in section 3.2`
- `feat: Add TypeScript testing examples`
- `fix: Correct coverage threshold in pipeline example`

### PR Description Template

```markdown
## Description
Brief description of changes

## Motivation
Why is this change needed?

## Changes Made
- Change 1
- Change 2
- Change 3

## Impact
- [ ] No impact on existing projects
- [ ] Teams need to take action
- [ ] Requires communication/training

## Testing
How did you verify these changes?

## Related Issues
Closes #123
Related to #456

## Checklist
- [ ] Changes follow style guide
- [ ] Version history updated (if applicable)
- [ ] Examples tested and working
- [ ] Documentation is clear
- [ ] No broken links
```

### Review Process

1. **Automated Checks**
   - Markdown linting
   - Link validation
   - Spell checking

2. **Human Review**
   - Technical accuracy
   - Clarity and readability
   - Alignment with principles
   - Impact assessment

3. **Approval Requirements**
   - Small: 1 approver
   - Medium: 2 approvers
   - Major: ARB approval

4. **Merge**
   - Squash and merge (keep history clean)
   - Delete source branch
   - Update version number
   - Announce changes

## Style Guide

### Writing Style

- **Clear and concise:** Prefer short sentences
- **Actionable:** Use imperative mood ("Run tests", not "Tests should be run")
- **Consistent terminology:** Use glossary terms
- **Examples:** Include code examples where helpful
- **Inclusive:** Use gender-neutral language

### Formatting

- **Headings:** Use title case
- **Lists:** Use bullets for unordered, numbers for sequences
- **Code blocks:** Always specify language
- **Links:** Use descriptive text, not "click here"
- **Tables:** Use for structured comparisons

### Code Examples

- **Complete:** Include all necessary context
- **Tested:** Verify examples actually work
- **Commented:** Add explanatory comments
- **Realistic:** Use meaningful names, realistic scenarios
- **Formatted:** Follow language conventions

Example:

```csharp
// ✅ Good - clear, complete, realistic
[Fact]
public async Task ProcessOrder_WithValidInput_ReturnsSuccess()
{
    // Arrange
    var order = new Order { Id = "123", Amount = 99.99m };
    var service = new OrderService();
    
    // Act
    var result = await service.Process(order);
    
    // Assert
    result.Success.Should().BeTrue();
}

// ❌ Bad - incomplete, unclear
[Fact]
public void Test1()
{
    var x = new Thing();
    Assert.True(x.Do());
}
```

## Review and Update Cycle

### Quarterly Reviews

- **When:** First week of each quarter
- **Who:** Architecture & Test Leads
- **Process:**
  1. Review all feedback from previous quarter
  2. Assess industry best practices
  3. Evaluate tool updates
  4. Propose updates
  5. Communicate changes

### Version Control

Semantic versioning: MAJOR.MINOR.PATCH

- **MAJOR:** Breaking changes, major restructuring
- **MINOR:** New sections, significant additions
- **PATCH:** Clarifications, corrections, small updates

### Change Communication

When standards are updated:
1. Update version in document
2. Add entry to version history (Appendix C)
3. Post announcement in #testing-standards channel
4. Send email to all tech leads
5. Update training materials
6. Include in next Community of Practice meeting

## Community of Practice

### Weekly Meetings

- **When:** Thursdays 2-3 PM
- **Format:** Open discussion, Q&A
- **Agenda:** 
  - Recently merged changes
  - Upcoming proposals
  - Challenges and solutions
  - Tool demonstrations

### Office Hours

- **When:** Thursdays 3-4 PM (immediately after CoP)
- **Format:** 1-on-1 or small group help
- **Topics:** Implementation questions, code reviews, architecture

## Recognition

Contributors are recognized in:
- Monthly newsletter
- Quarterly all-hands
- Version history acknowledgments
- Internal DevOps awards

## Questions?

- **Slack:** #testing-standards
- **Email:** testing-standards@company.com
- **Office Hours:** Thursdays 3-4 PM

## Maintainers

Current maintainers of this standard:

- Architecture Team (@architecture-team)
- Test Leads (@test-leads)
- DevOps Team (@devops-team)

---

Thank you for helping improve our testing standards! Every contribution, no matter how small, makes a difference. 🙏
