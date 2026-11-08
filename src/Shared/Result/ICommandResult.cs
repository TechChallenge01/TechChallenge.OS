using System.Net;
using System.Text.Json.Serialization;

namespace Shared.Result;

public interface ICommandResult
{
    string Message { get; set; }
    [JsonIgnore] HttpStatusCode StatusCode { get; set; }
}

public interface ICommandResult<T> : ICommandResult
{
    T Data { get; set; }
}
