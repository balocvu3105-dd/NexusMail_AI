using FluentAssertions;
using NetArchTest.Rules;
using Xunit;

namespace NexusMail.ArchitectureTests;

public class LayerTests
{
    private const string DomainNamespace = "NexusMail.Domain";
    private const string ApplicationNamespace = "NexusMail.Application";
    private const string InfrastructureNamespace = "NexusMail.Infrastructure";
    private const string ApiNamespace = "NexusMail.API";

    private const string SharedNamespace = "NexusMail.Shared";

    [Fact]
    public void Shared_ShouldNot_HaveDependencyOnOtherProjects()
    {
        var result = Types
            .InAssembly(typeof(Shared.Domain.IDomainEvent).Assembly)
            .ShouldNot()
            .HaveDependencyOn(DomainNamespace)
            .Or()
            .HaveDependencyOn(ApplicationNamespace)
            .Or()
            .HaveDependencyOn(InfrastructureNamespace)
            .Or()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.Should().BeTrue("Shared layer must not depend on any specific bounded context or infrastructure.");
    }

    [Fact]
    public void Domain_ShouldNot_HaveDependencyOnOtherProjects()
    {
        var result = Types
            .InAssembly(typeof(Domain.Workspace.Entities.Workspace).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApplicationNamespace)
            .Or()
            .HaveDependencyOn(InfrastructureNamespace)
            .Or()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.Should().BeTrue("Domain layer must be completely independent.");
    }

    [Fact]
    public void Application_ShouldNot_HaveDependencyOnInfrastructureOrApi()
    {
        var result = Types
            .InAssembly(typeof(Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(InfrastructureNamespace)
            .Or()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.Should().BeTrue("Application layer must only depend on Domain.");
    }

    [Fact]
    public void Infrastructure_ShouldNot_HaveDependencyOnApi()
    {
        var result = Types
            .InAssembly(typeof(Infrastructure.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn(ApiNamespace)
            .GetResult();

        result.Should().BeTrue("Infrastructure layer must not depend on API.");
    }

    [Fact]
    public void Handlers_ShouldNot_HaveDependencyOnEntityFrameworkCore()
    {
        var result = Types
            .InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        result.Should().BeTrue("Application Handlers must use Abstractions/Repositories, not EF Core directly.");
    }

    [Fact]
    public void Handlers_ShouldNot_Inject_IUnitOfWork()
    {
        // IUnitOfWork must ONLY be used by UnitOfWorkBehavior in the pipeline.
        // Handlers must delegate transaction responsibility to the pipeline.
        var result = Types
            .InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .ShouldNot()
            .HaveDependencyOn("NexusMail.Application.Abstractions.Persistence")
            .GetResult();

        result.Should().BeTrue("Handlers must not inject IUnitOfWork. Transaction commit is the sole responsibility of UnitOfWorkBehavior.");
    }

    [Fact]
    public void Handlers_ShouldNot_Inject_IServiceProvider()
    {
        // IServiceProvider is a service locator anti-pattern.
        // Handlers must use explicit constructor injection only.
        var result = Types
            .InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.Extensions.DependencyInjection.IServiceProvider")
            .GetResult();

        result.Should().BeTrue("Handlers must not use IServiceProvider (service locator anti-pattern). Use constructor injection.");
    }

    [Fact]
    public void Handlers_ShouldNot_Inject_IHttpContextAccessor()
    {
        // IHttpContextAccessor leaks HTTP infrastructure into the Application layer.
        // Use ICurrentUser or IWorkspaceContext abstractions instead.
        var result = Types
            .InAssembly(typeof(Application.DependencyInjection).Assembly)
            .That()
            .HaveNameEndingWith("Handler")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.AspNetCore.Http.IHttpContextAccessor")
            .GetResult();

        result.Should().BeTrue("Handlers must not inject IHttpContextAccessor. Use ICurrentUser or IWorkspaceContext abstractions.");
    }

    [Fact]
    public void Domain_ShouldNot_ReferenceAIProvider()
    {
        // Guardrail 10
        var result = Types
            .InAssembly(typeof(Domain.Workspace.Entities.Workspace).Assembly)
            .ShouldNot()
            .HaveDependencyOn("NexusMail.AI")
            .Or()
            .HaveDependencyOn("OpenAI")
            .Or()
            .HaveDependencyOn("Anthropic")
            .GetResult();

        result.Should().BeTrue("Domain layer must not reference any AI provider implementations or SDKs.");
    }

    [Fact]
    public void Application_ShouldDependOnlyOnAIAbstractions()
    {
        // Guardrail 14
        var result = Types
            .InAssembly(typeof(Application.DependencyInjection).Assembly)
            .ShouldNot()
            .HaveDependencyOn("NexusMail.AI")
            .Or()
            .HaveDependencyOn("OpenAI")
            .Or()
            .HaveDependencyOn("Anthropic")
            .GetResult();

        result.Should().BeTrue("Application AI features must depend only on AI abstractions, never on concrete provider implementations or AI SDKs.");
    }

    [Fact]
    public void AIWorker_ShouldNot_HaveDependencyOnAutomation()
    {
        // Guardrail 12 (AI không invoke Automation)
        // Since we don't have the exact assembly for Worker.AI in this context yet, we check the worker namespace
        var result = Types
            .InAssembly(typeof(NexusMail.Worker.AI.Consumers.SummaryRequestedConsumer).Assembly)
            .ShouldNot()
            .HaveDependencyOn("NexusMail.Automation")
            .GetResult();
            
        result.Should().BeTrue("Worker.AI must not invoke or depend on Automation layer directly.");
    }
}
