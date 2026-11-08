using System.Net;
using Shared.Result;

namespace API.Extensions;

public static class MinimalApiResultExtension
{
    public static IResult ToMinimalApiResult(this ICommandResult result)
        => result.StatusCode switch
        {
            HttpStatusCode.OK => Results.Ok(result),
            HttpStatusCode.Created => Results.Created(string.Empty, result),
            HttpStatusCode.NoContent => Results.NoContent(),
            HttpStatusCode.PartialContent => Results.Json(result, statusCode: 206),
            HttpStatusCode.NotFound => Results.NotFound(result),
            HttpStatusCode.BadRequest => Results.BadRequest(result),
            HttpStatusCode.Forbidden => Results.Forbid(),
            _ => Results.Json(result, statusCode: (int)result.StatusCode)
        };
}
