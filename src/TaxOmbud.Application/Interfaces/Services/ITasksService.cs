using TaxOmbud.Application.Tasks.DTOs;

namespace TaxOmbud.Application.Interfaces.Services;

public interface ITasksService
{
    Task<Guid> CreateCaseTaskAsync(CreateCaseTaskCommand request, CancellationToken cancellationToken = default);
    Task<DeleteCaseTaskCommand> DeleteCaseTaskAsync(DeleteCaseTaskCommand request, CancellationToken cancellationToken = default);
    Task<UpdateCaseTaskCommand> UpdateCaseTaskAsync(UpdateCaseTaskCommand request, CancellationToken cancellationToken = default);
    Task<CaseTaskDto> GetCaseTaskByIdAsync(GetCaseTaskByIdQuery request, CancellationToken cancellationToken = default);
    Task<List<CaseTaskDto>> GetCaseTasksAsync(GetCaseTasksQuery request, CancellationToken cancellationToken = default);
}
