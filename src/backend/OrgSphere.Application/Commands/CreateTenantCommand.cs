using DispatchR.Abstractions.Send;
using OrgSphere.Application.DTOs;
using OrgSphere.Domain.Entities;
using OrgSphere.Domain.Interfaces;
using OrgSphere.Domain.ValueObjects;

namespace OrgSphere.Application.Commands;

public record CreateTenantCommand(string Name, string Slug) : IRequest<CreateTenantCommand, ValueTask<TenantDto>>;

public class CreateTenantHandler : IRequestHandler<CreateTenantCommand, ValueTask<TenantDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTenantHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async ValueTask<TenantDto> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = new Tenant
        {
            Name = request.Name,
            Slug = request.Slug
        };

        var created = await _unitOfWork.Tenants.CreateAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new TenantDto
        {
            Id = created.Id.Value,
            Name = created.Name,
            Slug = created.Slug,
            Plan = created.Plan,
            IsActive = created.IsActive,
            CreatedAt = created.CreatedAt
        };
    }
}
