using ATMProject.Application.Users;
using ATMProject.System;

namespace ATMProject.Application.Operations.Authorization;
public class AuthorizationOperationDecorator<TRequest, TResponse>
    : IOperation<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    private readonly IOperation<TRequest, TResponse> _innerOperation;
    private readonly IUserContextService _userContextService;

    public AuthorizationOperationDecorator(
        IOperation<TRequest, TResponse> innerOperation, 
        IUserContextService userContextService
    )
    {
        _innerOperation = innerOperation;
        _userContextService = userContextService;
    }

    public TResponse Execute(TRequest request)
    {
        if (
            request.GetType().HasAttribute<RequiresAdminAttribute>() &&
            _userContextService.GetUserContext().UserRole != UserRole.Admin
        )
        {
            throw new Exception($"Non Admin user is not authorized to use Operation {_innerOperation.GetType().Name}");
        }
        return _innerOperation.Execute(request);
    }
}
