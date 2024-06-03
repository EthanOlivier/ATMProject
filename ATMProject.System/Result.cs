namespace ATMProject.System;

public class IResult
{
    bool Success { get; }
    string ErrorMessage { get; }
}

public class Result : IResult
{
    private bool _success;
    private string? _errorMessage;

    public bool Success => _success;
    public string ErrorMessage
    {
        get
        {
            if (Success)
            {
                throw new Exception("Cannot retrieve ErrorMessage, as the result was successful.");
            }
            return _errorMessage!;
        }
    }

    private Result(bool success, string errorMessage)
    {
        _success = success;
        _errorMessage = errorMessage;
    }

    public static Result Succeeded()
    {
        return new Result(
            success: true,
            errorMessage: string.Empty
        );
    }
    public static Result Failed(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
        {
            throw new Exception("Cannot create Failed Result. Error Message cannot be null.");
        }
        return new Result(
            success: false,
            errorMessage: errorMessage
        );
    }
}

public class Result<TResultType> : IResult where TResultType : class
{
    private bool _success;
    private string? _errorMessage;
    private TResultType? _resultData;

    public bool Success => _success;
    public string ErrorMessage
    {
        get
        {
            if (Success)
            {
                throw new Exception("Cannot retrieve ErrorMessage, as the result was successful.");
            }
            return _errorMessage!;
        }
    }
    public TResultType ResultData
    {
        get
        {
            if (!Success)
            {
                throw new Exception("Cannot retrieve ResultData as the result was not successful");
            }
            return _resultData!;
        }
    }

    private Result(bool success, string errorMessage, TResultType? resultData)
    {
        _success = success;
        _errorMessage = errorMessage;
        _resultData = resultData;
    }

    public static Result<TResultType> Succeeded(TResultType resultData)
    {
        return new Result<TResultType>(
            success: true,
            errorMessage: string.Empty,
            resultData: resultData
        );
    }
    public static Result<TResultType> Failed(string errorMessage)
    {
        if (string.IsNullOrEmpty(errorMessage))
        {
            throw new Exception("Cannot create Failed Result. Error Message cannot be null.");
        }
        return new Result<TResultType>(
            success: false,
            errorMessage: errorMessage,
            resultData: null
        );
    }
}

