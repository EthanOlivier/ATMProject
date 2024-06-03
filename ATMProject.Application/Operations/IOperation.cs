namespace ATMProject.Application.Operations
{
    public interface IOperation<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        TResponse Execute(TRequest request);
    }
}
