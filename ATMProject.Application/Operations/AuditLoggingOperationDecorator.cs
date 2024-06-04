using System.Text.Json;

namespace ATMProject.Application.Operations;
public class AuditLoggingOperationDecorator<TRequest, TResponse> : IOperation<TRequest, TResponse>
    where TRequest : class
    where TResponse : class
{
    private readonly IOperation<TRequest, TResponse> _innerOperation;
    private readonly IUserContextService _userContextService;
    private readonly ILogger _logger;

    public AuditLoggingOperationDecorator(IOperation<TRequest, TResponse> innerOperation, IUserContextService userContextService, ILogger logger)
    {
        _innerOperation = innerOperation;
        _userContextService = userContextService;
        _logger = logger;
    }

    public TResponse Execute(TRequest request)
    {
        var result = _innerOperation.Execute(request);
        var output = new
        {
            Timestamp = DateTime.UtcNow,
            Request = JsonSerializer.Serialize(request),
            Result = JsonSerializer.Serialize(result),
            _userContextService.GetUserContext().UserId,
            _userContextService.GetUserContext().UserRole
        };
        _logger.Log(JsonSerializer.Serialize(output));
        return result;
    }
}
