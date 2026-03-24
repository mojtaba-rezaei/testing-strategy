# AI-Generated Content Directory

This directory contains AI-generated prompts, instructions, and tracking logs for maintaining the testing strategy documentation.

## Purpose

The `.ai` folder serves as a dedicated space for:
- **AI Prompts**: Reusable prompts for generating testing strategy content
- **Generation Logs**: Historical tracking of all AI-generated content
- **AI Agent Instructions**: Comprehensive instructions for AI agents to generate tests and documentation

## Structure

```
.ai/
├── README.md                               # This file
├── generation-log.md                       # Chronological log of all AI generations
└── prompts/
    ├── testing-strategy-prompt.md          # Original strategy generation prompt
    └── unit-test-generator.md              # Comprehensive unit test generation instructions
```

## How to Use This Directory

### For Humans
- **Review Prompts**: Understand how the testing strategy was originally generated
- **Track Changes**: Use generation-log.md to see what was AI-generated and when
- **Update Prompts**: Modify prompts when strategy evolves
- **Governance**: Ensure AI-generated content aligns with company standards

### For AI Agents
- **Generate Unit Tests**: Use `unit-test-generator.md` for comprehensive test generation
  - Scans ALL testable components in the Function App (not just single classes)
  - Uses CSV mapping specifications for precise mapper/converter test assertions
  - Asks the user for clarification instead of guessing unclear behavior
  - Delegates to subagents for heavy workloads (3+ classes)
  - Verifies tests compile and pass before completing
- **Evolve Strategy**: Use `testing-strategy-prompt.md` to regenerate or update the strategy
- **Follow Standards**: All prompts reference the core `AUTOMATION_TESTING_STANDARD.md`

## Key Principles

1. **Transparency**: All AI-generated content is tracked in generation-log.md
2. **Reusability**: Prompts are designed to be reusable across projects
3. **Version Control**: Prompts evolve with the testing strategy
4. **Human Oversight**: AI-generated content must be reviewed by humans before adoption
5. **Always Ask, Never Guess**: AI agents must ask users for clarification when behavior or context is unclear
6. **Subagent Delegation**: Heavy workloads are split across subagents for efficiency and quality
7. **CSV-Driven Precision**: Mapper/converter tests use CSV mapping specifications when available

## Related Documentation

- [AUTOMATION_TESTING_STANDARD.md](../architecture/docs/AUTOMATION_TESTING_STANDARD.md) - The single source of truth
- [NAMING_CONVENTIONS.md](../architecture/docs/NAMING_CONVENTIONS.md) - Test naming standards
- [PHASE_1_UNIT_TESTING.md](../architecture/docs/PHASE_1_UNIT_TESTING.md) - Unit testing guidance
- [PHASE_2_INTEGRATION_TESTING.md](../architecture/docs/PHASE_2_INTEGRATION_TESTING.md) - Integration testing guidance

## Maintenance

- **Update Date**: February 16, 2026
- **Maintainer**: Integration Platform Team
- **Review Frequency**: Quarterly or when testing standards change

## Warning

⚠️ **Do not execute AI prompts blindly**. Always:
1. Review generated content for accuracy
2. Validate against current standards
3. Test generated code before committing
4. Update prompts when standards evolve
