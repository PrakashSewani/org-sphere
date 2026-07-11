using DispatchR.Abstractions.Send;
using FluentValidation;

namespace OrgSphere.Application.Behaviors;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators = validators;

    public required IRequestHandler<TRequest, TResponse> NextPipeline { get; set; }

    public TResponse Handle(TRequest request, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = _validators
                .Select(v => v.Validate(context))
                .ToList();

            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }
        }

        return NextPipeline.Handle(request, cancellationToken);
    }
}
