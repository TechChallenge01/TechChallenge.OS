using System.Net;

namespace Shared.Result;

public class CommandResult : ICommandResult
{
    public CommandResult(string message, HttpStatusCode statusCode)
    {
        Message = message;
        StatusCode = statusCode;
    }

    public string Message { get; set; }
    public HttpStatusCode StatusCode { get; set; }
}

public class CommandResult<T> : CommandResult, ICommandResult<T>
{
    public CommandResult(string message, HttpStatusCode statusCode, T data) : base(message, statusCode)
    {
        Data = data;
    }

    public T Data { get; set; }
}
