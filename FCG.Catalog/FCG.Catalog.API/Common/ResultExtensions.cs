using FCG.Catalog.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.API.Common.Outputs;

public static class ResultExtensions
{
    public static IActionResult ToCreatedActionResult<T>(this ResultData<T> result, string path = "")
    {
        if (!result.IsSuccess)
            return new BadRequestObjectResult(result);

        return new CreatedResult(path, result);
    }

    public static IActionResult ToAcceptedActionResult<T>(this ResultData<T> result, string path = "")
    {
        if (!result.IsSuccess)
            return new BadRequestObjectResult(result);

        if (!string.IsNullOrEmpty(path))
        {
            return new AcceptedResult(path, result);
        }

        return new AcceptedResult(path, result);
    }
    public static IActionResult ToOkActionResult<T>(this ResultData<T> result)
    {
        if (!result.IsSuccess)
            return new BadRequestObjectResult(result);

        return new OkObjectResult(result);
    }

    public static IActionResult ToNoContentActionResult<T>(this ResultData<T> result)
    {
        if (!result.IsSuccess)
            return new BadRequestObjectResult(result);

        return new NoContentResult();
    }

    public static IActionResult ToUnauthorizedActionResult<T>(this ResultData<T> result)
    {
        if (!result.IsSuccess)
            return new BadRequestObjectResult(result);

        return new UnauthorizedObjectResult(new { message = "Não autorizado" });
    }

}