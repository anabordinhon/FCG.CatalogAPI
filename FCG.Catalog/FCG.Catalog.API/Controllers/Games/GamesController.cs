using FCG.Catalog.API.Common.Outputs;
using FCG.Catalog.Application.Common.Contracts;
using FCG.Catalog.Application.Games.;
using FCG.Catalog.Application.Games.Ports;
using FCG.Catalog.Application.Games.UseCases.Commands.AddGame;
using FCG.Catalog.Application.Games.UseCases.Queries.GetGameById;
using FCG.Catalog.Application.Games.UseCases.Queries.GetGamesPaged;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FCG.Catalog.API.Controllers.Games;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GamesController : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> GetGamesPaged(
        [FromServices] IGetGamesPagedQueryHandler handler,
        CancellationToken cancellationToken,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetGamesPagedQuery(page, pageSize);
        var result = await handler.Handle(query, cancellationToken);

        return result.ToOkActionResult();
    }

    [HttpGet("search")]
    [AllowAnonymous]
    public async Task<IActionResult> Search(
    [FromQuery] string q,
    [FromServices] IGameSearchRepository searchRepository,
    CancellationToken cancellationToken,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("O parâmetro 'q' é obrigatório.");

        var results = await searchRepository.SearchAsync(q, page, pageSize, cancellationToken);
        return Ok(results);
    }

    [HttpGet("{publicId}")]
    public async Task<IActionResult> GetGameById(
        [FromRoute] Guid publicId,
        [FromServices] IGetGameByIdQueryHandler handler,
        CancellationToken cancellationToken)
    {
        var query = new GetGameByIdQuery(publicId);
        var result = await handler.Handle(query, cancellationToken);
        return result.ToOkActionResult();
    }

    [Authorize(Roles = nameof(EUserRoleContract.Admin))]
    [HttpPost]
    public async Task<IActionResult> AddOrUpdatGame(
        [FromBody] AddOrUpdateGameInput input,
        [FromServices] IAddOrUpdateGameCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var command = input.MapToCommand();
        var result = await handler.Handle(command, cancellationToken);

        if (!result.IsSuccess)
            return result.ToOkActionResult();

        return result.ToCreatedActionResult($"/api/games/{result.Data.PublicId}");
    }
}