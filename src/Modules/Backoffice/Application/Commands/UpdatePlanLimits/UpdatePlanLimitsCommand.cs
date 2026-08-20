using System.Text.Json;
using PersonaScript.BuildingBlocks.CQRS;
using PersonaScript.BuildingBlocks.Results;
using PersonaScript.Modules.Backoffice.Domain;
using PersonaScript.Modules.Backoffice.Domain.Repositories;
using PersonaScript.Modules.Billing.Domain;

namespace PersonaScript.Modules.Backoffice.Application.Commands.UpdatePlanLimits;

public record UpdatePlanLimitsCommand(
    Guid AdminUserId,
    string AdminEmail,
    Guid PlanId,
    string Name,
    string Description,
    decimal MonthlyPrice,
    decimal YearlyPrice,
    int MaxActivePersonas,
    int MaxScriptsPerMonth,
    int MaxAiAnalysesPerMonth) : ICommand;

public sealed class UpdatePlanLimitsCommandHandler(
    IPlanRepository planRepository,
    IAdminAuditLogRepository auditLogRepository) : ICommandHandler<UpdatePlanLimitsCommand>
{
    public async Task<Result> Handle(UpdatePlanLimitsCommand command, CancellationToken cancellationToken)
    {
        var plan = await planRepository.GetByIdAsync(command.PlanId, cancellationToken);
        if (plan is null)
        {
            return Result.Failure(Error.NotFound("UpdatePlanLimits.NotFound", "Plano não encontrado."));
        }

        var updateResult = plan.UpdateLimits(
            command.Name,
            command.Description,
            command.MonthlyPrice,
            command.YearlyPrice,
            command.MaxActivePersonas,
            command.MaxScriptsPerMonth,
            command.MaxAiAnalysesPerMonth);

        if (updateResult.IsFailure)
        {
            return updateResult;
        }

        planRepository.Update(plan);

        var audit = AdminAuditLog.Record(
            "UPDATE_PLAN_LIMITS",
            command.AdminUserId,
            command.AdminEmail,
            Guid.Empty,
            command.AdminEmail,
            JsonSerializer.Serialize(new
            {
                command.PlanId,
                command.Name,
                command.MonthlyPrice,
                command.YearlyPrice,
                command.MaxActivePersonas,
                command.MaxScriptsPerMonth,
                command.MaxAiAnalysesPerMonth
            })).Value;


        await auditLogRepository.AddAsync(audit, cancellationToken);

        return Result.Success();
    }
}
