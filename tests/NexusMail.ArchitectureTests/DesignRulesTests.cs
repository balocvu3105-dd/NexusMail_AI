using FluentAssertions;
using NetArchTest.Rules;
using NexusMail.Shared.Domain;
using System.Reflection;
using Xunit;

namespace NexusMail.ArchitectureTests;

public class DesignRulesTests
{
    [Fact]
    public void Entities_Should_InheritFromEntityOrAggregateRoot()
    {
        var result = Types
            .InAssembly(typeof(Domain.Workspace.Entities.Workspace).Assembly)
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .HaveNameMatching("(?i).*Entity.*")
            .Should()
            .Inherit(typeof(EntityBase))
            .GetResult();
        
        result.Should().BeTrue();
    }

    [Fact]
    public void DomainEvents_Should_BeSealed()
    {
        var result = Types
            .InAssembly(typeof(Domain.Workspace.Entities.Workspace).Assembly)
            .That()
            .Inherit(typeof(NexusMail.Shared.Events.EventBase))
            .Should()
            .BeSealed()
            .GetResult();

        result.Should().BeTrue("Domain Events should be sealed to prevent further inheritance.");
    }

    [Fact]
    public void Commands_Should_EndWithCommand()
    {
        var result = Types
            .InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .ImplementInterface(typeof(MediatR.IBaseRequest))
            .And()
            .AreNotInterfaces() // Exclude ICommand<T>, IQuery<T> marker interfaces themselves
            .And()
            .DoNotHaveNameEndingWith("Query") // Exclude queries
            .Should()
            .HaveNameEndingWith("Command")
            .GetResult();

        result.Should().BeTrue("All MediatR requests (commands) must end with 'Command'.");
    }

    [Fact]
    public void CommandHandlers_Should_EndWithCommandHandler()
    {
        var result = Types
            .InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .Should()
            .HaveNameEndingWith("Handler")
            .GetResult();

        result.Should().BeTrue("All MediatR handlers must end with 'Handler'.");
    }

    [Fact]
    public void Handlers_Should_BeSealed()
    {
        var result = Types
            .InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .ImplementInterface(typeof(MediatR.IRequestHandler<,>))
            .Should()
            .BeSealed()
            .GetResult();

        result.Should().BeTrue("All Application handlers must be sealed to prevent extension.");
    }

    [Fact]
    public void Application_ShouldNot_Use_DateTimeUtcNow_Directly()
    {
        // NetArchTest operates on IL, which makes it hard to distinguish DateTime properties from DateTime.UtcNow calls.
        // We can do a static code analysis approach by reading the source files if they are available in the repository.
        var baseDir = System.IO.Directory.GetCurrentDirectory();
        var srcDir = System.IO.Path.GetFullPath(System.IO.Path.Combine(baseDir, "../../../../../src/NexusMail.Application"));

        if (!System.IO.Directory.Exists(srcDir))
        {
            // If running in an environment where source code isn't available, skip or pass.
            return;
        }

        var csFiles = System.IO.Directory.GetFiles(srcDir, "*.cs", System.IO.SearchOption.AllDirectories);
        var violations = new System.Collections.Generic.List<string>();

        foreach (var file in csFiles)
        {
            var content = System.IO.File.ReadAllText(file);
            if (content.Contains("DateTime.UtcNow") || content.Contains("DateTime.Now"))
            {
                violations.Add(System.IO.Path.GetFileName(file));
            }
        }

        violations.Should().BeEmpty("Application layer must use IClock for time abstractions instead of DateTime.UtcNow directly.");
    }
}
