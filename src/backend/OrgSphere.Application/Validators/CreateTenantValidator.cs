using FluentValidation;
using OrgSphere.Application.DTOs;

namespace OrgSphere.Application.Validators;

public class CreateTenantValidator : BaseValidator<CreateTenantRequest>
{
    public CreateTenantValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tenant name is required")
            .MaximumLength(100).WithMessage("Tenant name must not exceed 100 characters");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Tenant slug is required")
            .MaximumLength(50).WithMessage("Tenant slug must not exceed 50 characters")
            .Matches("^[a-z0-9-]+$").WithMessage("Tenant slug must contain only lowercase letters, numbers, and hyphens");
    }
}

public record CreateTenantRequest(string Name, string Slug);
