# Contributing to DatabaseSeeder

First off, thank you for considering contributing to DatabaseSeeder! This project thrives through community involvement and collaboration. Every contribution, whether it's a bug fix, new feature, documentation improvement, or maintenance support, makes a real difference in keeping this project active and valuable for the .NET community.

## The Power of Community Development

DatabaseSeeder follows the open-source philosophy that the best software is built through community collaboration. The project welcomes and relies on contributors like you to:
- Help maintain and improve existing features
- Add new capabilities
- Fix bugs and issues
- Keep documentation current
- Support other users
- Share knowledge and expertise

Your contributions help ensure the project remains useful and well-maintained for everyone. Whether you can make a one-time contribution or become a regular maintainer, your help is invaluable in keeping this project thriving.

## Code of Conduct

By participating in this project, you are expected to uphold our Code of Conduct, which is to treat all contributors with respect and foster an open and welcoming environment.

## Getting Started

Contributions to DatabaseSeeder are made via Issues and Pull Requests (PRs) on GitHub.

- Search existing Issues and PRs before creating your own.
- We work off the `master` branch for main development.
- Feel free to take on any open issues that interest you.
- Help reviewing and testing PRs from other contributors is always appreciated.

### Development Process

1. Fork the repository and create your branch from `master`.
2. Name your branch with the format:
   - `feature/your-feature-name` for new features
   - `fix/issue-description` for bug fixes
   - `docs/what-you-documented` for documentation changes

3. Set up your development environment:
   ```bash
   git clone [your-fork-url]
   cd DatabaseSeeder
   dotnet restore
   ```

4. Update the tests and ensure all new features or bug fixes have tests.
5. Run all tests locally:
   ```bash
   dotnet test
   ```

### Making Changes

When making changes, please follow these guidelines:

#### Code Style

- Follow the existing code style and formatting.
- Use C# coding conventions from the .NET Foundation.
- Keep methods focused and concise.
- Use meaningful variable and method names.
- Add XML documentation to public APIs.

#### Architecture Guidelines

1. **Seeder Classes:**
   - Keep seeders single-responsibility focused
   - Implement proper dependency management
   - Follow asynchronous patterns where appropriate

2. **Database Operations:**
   - Use batching for large datasets
   - Implement proper transaction handling
   - Consider performance implications

3. **Entity Framework:**
   - Follow EF Core best practices
   - Consider database provider compatibility
   - Handle migrations appropriately

#### Testing

1. Write tests for:
   - New features
   - Bug fixes
   - Edge cases
   - Performance critical paths

2. Test Categories:
   - Unit tests for individual components
   - Integration tests for database operations
   - Performance tests for critical paths

### Pull Request Process

1. **Create your PR:**
   - Make sure your code builds without errors
   - Include relevant tests
   - Update documentation if needed

2. **PR Title and Description:**
   - Use clear, descriptive titles
   - Reference any related issues
   - Explain your changes and reasoning

3. **Review Process:**
   - Address review feedback
   - Keep discussions focused and professional
   - Squash commits when requested

4. **Taking Ownership:**
   - Feel empowered to maintain and support the features you contribute
   - Consider helping with related issues and future improvements
   - Share your knowledge with other contributors

### Version Numbering

We follow semantic versioning (SemVer):

- Major version: Breaking changes
- Minor version: New features, backward compatible
- Patch version: Bug fixes, backward compatible

Use `+semver:` tags in your commit messages to indicate version impacts:
- `+semver: major` - Breaking changes
- `+semver: minor` - New features
- `+semver: patch` - Bug fixes

### Documentation

- Update the README.md if needed
- Add XML comments to public APIs
- Update or create wiki pages for major features
- Include code examples where helpful

## Project Sustainability

DatabaseSeeder's long-term success depends on community involvement at all levels:
- Code contributions
- Documentation improvements
- Issue reporting and triage
- PR reviews
- User support
- Feature discussions

Contributors who show consistent involvement may be invited to take on maintainer roles with additional repository access and responsibilities. This helps ensure the project remains actively maintained and can continue to grow through distributed community effort.

## Release Process

1. **Versioning:**
   - Versions are managed through GitVersion
   - Release tags follow the format `v1.2.3`

2. **Publishing:**
   - NuGet packages are automatically published for tagged releases
   - Documentation is updated with each release

## Where to Get Help

- Check the [README.md](README.md)
- Look through [existing issues](https://github.com/Tomtomcapo/DatabaseSeeder/issues)
- Create a new issue for questions
- Join project discussions

## Recognition

Contributors will be recognized in:
- Release notes
- Project README
- Documentation where appropriate

Your contributions help keep DatabaseSeeder a vibrant and well-maintained tool for the .NET community. Thank you for being part of this project!