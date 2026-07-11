using FluentValidation;

namespace OrgSphere.Application.Validators;

public static class ValidatorExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithErrorCode<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, string errorCode)
    {
        return rule.WithMessage(errorCode);
    }
}

public abstract class BaseValidator<T> : AbstractValidator<T>
{
}
