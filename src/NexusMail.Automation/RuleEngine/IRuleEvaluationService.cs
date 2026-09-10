using System.Threading;
using System.Threading.Tasks;
using NexusMail.Contracts.Automation;

namespace NexusMail.Automation.RuleEngine;

public interface IRuleEvaluationService
{
    Task EvaluateAsync(EvaluateRulesMessage message, CancellationToken cancellationToken);
}
