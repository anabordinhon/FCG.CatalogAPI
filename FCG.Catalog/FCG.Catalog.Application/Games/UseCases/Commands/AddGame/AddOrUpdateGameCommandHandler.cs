using FCG.Catalog.Application.Common;
using FCG.Catalog.Application.Games.Mappers;
using FCG.Catalog.Application.Games.Outputs;
using FCG.Catalog.Application.Games.Ports;
using FCG.Catalog.Domain.Games.Entities;
using FCG.Catalog.Domain.Games.Ports;


namespace FCG.Catalog.Application.Games.UseCases.Commands.AddGame;

public class AddOrUpdateGameCommandHandler : IAddOrUpdateGameCommandHandler
{
    private readonly IGameCommandRepository _gameCommandRepository;
    private readonly IGameSearchRepository _gameSearchRepository;


    public AddOrUpdateGameCommandHandler(
        IGameCommandRepository gameCommandRepository,
        IGameSearchRepository gameSearchRepository)
    {
        _gameCommandRepository = gameCommandRepository;
        _gameSearchRepository = gameSearchRepository;
    }

    public async Task<ResultData<GameOutput>> Handle(AddOrUpdateGameCommand command, CancellationToken cancellationToken)
    {
        Game game;

        if (command.PublicId.HasValue)
        {
            game = await _gameCommandRepository.GetByIdAsync(command.PublicId.Value, cancellationToken);

            if (game == null)
                return ResultData<GameOutput>.Error("Registro não encontrado.");

            // A verificação de duplicidade só é necessária se os campos que formam a chave única (Description, Developer) forem alterados.
            if (game.Description != command.Description || game.Developer != command.Developer)
            {
                var gameExists = await _gameCommandRepository.GameExistsAsync(
                    command.PublicId,
                    command.Description,
                    command.Developer,
                    cancellationToken
                );

                if (gameExists)
                    return ResultData<GameOutput>.Error("Já existe um jogo com a mesma descrição e desenvolvedora.");
            }

            game.UpdateDetails(
                command.Name,
                command.Description,
                command.Genre,
                command.ReleaseDate,
                command.Developer,
                command.Price,
                command.AgeRating
            );
            await _gameCommandRepository.UpdateAsync(game, cancellationToken);
        }
        else
        {
            var gameExists = await _gameCommandRepository.GameExistsAsync(
                null, // Na criação, não há ID para excluir da busca
                command.Description,
                command.Developer,
                cancellationToken
            );

            if (gameExists)
                return ResultData<GameOutput>.Error("Já existe um jogo com a mesma descrição e desenvolvedora.");

            game = Game.Create(
                command.Name,
                command.Description,
                command.Genre,
                command.ReleaseDate,
                command.Developer,
                command.Price,
                command.AgeRating
            );

            await _gameCommandRepository.AddAsync(game, cancellationToken);
        }

        var indexDto = new GameIndexDto(
            game.PublicId,
            game.Name,
            game.Description,
            game.Developer,
            game.Genre.ToString(),
            game.Price.Value
        );

        await _gameSearchRepository.IndexAsync(indexDto, cancellationToken);

        var gameOutput = game.ToOutput();

        return ResultData<GameOutput>.Success(gameOutput);
    }
}