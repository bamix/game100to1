using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Game100To1.Backend.Services;
using Game100To1.Backend.Hubs;
using Game100To1.Backend.Models;

namespace Game100To1.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GameController(GameService gameService, IHubContext<GameHub> hubContext) : ControllerBase
{
    /// <summary>
    /// Получить текущее состояние игры
    /// </summary>
    /// <returns>Объект состояния игры</returns>
    /// <response code="200">Возвращает текущее состояние игры</response>
    [HttpGet("state")]
    [ProducesResponseType<GameStateResponse>(StatusCodes.Status200OK)]
    public ActionResult<GameStateResponse> GetGameState()
    {
        var gameState = gameService.GetGameState();
        return Ok(new GameStateResponse(gameState));
    }

    /// <summary>
    /// Начать новую игру
    /// </summary>
    /// <returns>Подтверждение запуска</returns>
    /// <response code="200">Игра успешно запущена</response>
    [HttpPost("start")]
    [ProducesResponseType<StartGameResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StartGameResponse>> StartNewGame()
    {
        gameService.StartNewGame();
        await hubContext.Clients.All.SendAsync("GameStateChanged", gameService.GetGameState());
        return Ok(new StartGameResponse("Новая игра запущена"));
    }

    /// <summary>
    /// Начать текущий раунд
    /// </summary>
    /// <returns>Подтверждение запуска раунда</returns>
    /// <response code="200">Раунд успешно запущен</response>
    [HttpPost("start-round")]
    [ProducesResponseType<StartRoundResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<StartRoundResponse>> StartRound()
    {
        gameService.StartRound();
        await hubContext.Clients.All.SendAsync("GameStateChanged", gameService.GetGameState());
        return Ok(new StartRoundResponse("Раунд запущен"));
    }

    /// <summary>
    /// Перейти к следующему раунду
    /// </summary>
    /// <returns>Подтверждение перехода</returns>
    /// <response code="200">Переход к следующему раунду выполнен</response>
    [HttpPost("next-round")]
    [ProducesResponseType<NextRoundResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<NextRoundResponse>> NextRound()
    {
        gameService.NextRound();
        await hubContext.Clients.All.SendAsync("GameStateChanged", gameService.GetGameState());
        return Ok(new NextRoundResponse("Переход к следующему раунду"));
    }

    /// <summary>
    /// Загрузить следующий вопрос
    /// </summary>
    /// <returns>Подтверждение загрузки</returns>
    /// <response code="200">Новый вопрос загружен</response>
    [HttpPost("next-question")]
    [ProducesResponseType<NextQuestionResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<NextQuestionResponse>> NextQuestion()
    {
        gameService.NextQuestion();
        await hubContext.Clients.All.SendAsync("GameStateChanged", gameService.GetGameState());
        return Ok(new NextQuestionResponse("Новый вопрос загружен"));
    }

    /// <summary>
    /// Открыть ответ по индексу
    /// </summary>
    /// <param name="answerIndex">Индекс ответа (0-4)</param>
    /// <returns>Подтверждение открытия ответа</returns>
    /// <response code="200">Ответ успешно открыт</response>
    [HttpPost("reveal-answer/{answerIndex}")]
    [ProducesResponseType<RevealAnswerResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RevealAnswerResponse>> RevealAnswer(int answerIndex)
    {
        gameService.RevealAnswer(answerIndex);
        await hubContext.Clients.All.SendAsync("AnswerRevealed", answerIndex);
        await hubContext.Clients.All.SendAsync("GameStateChanged", gameService.GetGameState());
        return Ok(new RevealAnswerResponse($"Ответ {answerIndex + 1} открыт", answerIndex));
    }

    /// <summary>
    /// Начислить очки команде за ответ
    /// </summary>
    /// <param name="teamId">ID команды (1 или 2)</param>
    /// <param name="answerIndex">Индекс ответа (0-4)</param>
    /// <returns>Подтверждение начисления очков</returns>
    /// <response code="200">Очки успешно начислены</response>
    [HttpPost("award-points/{teamId}/{answerIndex}")]
    [ProducesResponseType<AwardPointsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AwardPointsResponse>> AwardPoints(int teamId, int answerIndex)
    {
        gameService.AwardPoints(teamId, answerIndex);
        await hubContext.Clients.All.SendAsync("ScoreUpdated", gameService.GetGameState().Teams);
        await hubContext.Clients.All.SendAsync("GameStateChanged", gameService.GetGameState());
        return Ok(new AwardPointsResponse($"Очки начислены команде {teamId}", teamId, answerIndex));
    }

    /// <summary>
    /// Добавить ошибку команде
    /// </summary>
    /// <param name="teamId">ID команды (1 или 2)</param>
    /// <returns>Подтверждение добавления ошибки</returns>
    /// <response code="200">Ошибка успешно добавлена</response>
    [HttpPost("add-error/{teamId}")]
    [ProducesResponseType<AddErrorResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AddErrorResponse>> AddError(int teamId)
    {
        gameService.AddError(teamId);
        await hubContext.Clients.All.SendAsync("ErrorsUpdated", gameService.GetGameState().Teams);
        await hubContext.Clients.All.SendAsync("GameStateChanged", gameService.GetGameState());
        return Ok(new AddErrorResponse($"Ошибка добавлена команде {teamId}", teamId));
    }

    /// <summary>
    /// Убрать ошибку у команды
    /// </summary>
    /// <param name="teamId">ID команды (1 или 2)</param>
    /// <returns>Подтверждение удаления ошибки</returns>
    /// <response code="200">Ошибка успешно убрана</response>
    [HttpPost("remove-error/{teamId}")]
    [ProducesResponseType<RemoveErrorResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<RemoveErrorResponse>> RemoveError(int teamId)
    {
        gameService.RemoveError(teamId);
        await hubContext.Clients.All.SendAsync("ErrorsUpdated", gameService.GetGameState().Teams);
        await hubContext.Clients.All.SendAsync("GameStateChanged", gameService.GetGameState());
        return Ok(new RemoveErrorResponse($"Ошибка убрана у команды {teamId}", teamId));
    }

    /// <summary>
    /// Установить названия команд
    /// </summary>
    /// <param name="request">Запрос с названиями команд</param>
    /// <returns>Подтверждение обновления названий</returns>
    /// <response code="200">Названия команд успешно обновлены</response>
    [HttpPost("set-team-names")]
    [ProducesResponseType<SetTeamNamesResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SetTeamNamesResponse>> SetTeamNames([FromBody] SetTeamNamesRequest request)
    {
        gameService.SetTeamNames(request.Team1Name, request.Team2Name);
        await hubContext.Clients.All.SendAsync("GameStateChanged", gameService.GetGameState());
        return Ok(new SetTeamNamesResponse("Названия команд обновлены", request.Team1Name, request.Team2Name));
    }

    /// <summary>
    /// Установить настройки раунда (множитель и режим)
    /// </summary>
    /// <param name="request">Запрос с настройками раунда</param>
    /// <returns>Подтверждение обновления настроек</returns>
    /// <response code="200">Настройки раунда успешно обновлены</response>
    [HttpPost("set-round-settings")]
    [ProducesResponseType<SetRoundSettingsResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<SetRoundSettingsResponse>> SetRoundSettings([FromBody] SetRoundSettingsRequest request)
    {
        var mode = (GameMode)request.Mode;
        gameService.SetRoundSettings(request.Multiplier, mode);
        await hubContext.Clients.All.SendAsync("GameStateChanged", gameService.GetGameState());
        
        var modeString = mode == GameMode.Normal ? "Обычный" : "Редкий ответ";
        return Ok(new SetRoundSettingsResponse($"Настройки раунда обновлены: x{request.Multiplier}, {modeString}", request.Multiplier, modeString));
    }

    /// <summary>
    /// Получить информацию о доступных SignalR событиях
    /// </summary>
    /// <returns>Список доступных SignalR событий</returns>
    /// <response code="200">Возвращает информацию о SignalR Hub</response>
    [HttpGet("signalr-info")]
    [ProducesResponseType<SignalRInfo>(StatusCodes.Status200OK)]
    public ActionResult<SignalRInfo> GetSignalRInfo()
    {
        var signalRInfo = new SignalRInfo(
            "/gamehub",
            new List<SignalREvent>
            {
                new("GameStateChanged", "Отправляется при изменении состояния игры"),
                new("AnswerRevealed", "Отправляется при открытии ответа"),
                new("ScoreUpdated", "Отправляется при обновлении счета"),
                new("ErrorsUpdated", "Отправляется при изменении ошибок команд"),
                new("RoundStarted", "Отправляется при запуске раунда"),
                new("GameReset", "Отправляется при сбросе игры")
            },
            new List<SignalRMethod>
            {
                new("JoinGame", "Присоединиться к игре")
            }
        );

        return Ok(signalRInfo);
    }

    /// <summary>
    /// Получить список всех доступных вопросов
    /// </summary>
    /// <returns>Список вопросов, загруженных из конфигурации</returns>
    /// <response code="200">Возвращает все вопросы игры</response>
    [HttpGet("questions")]
    [ProducesResponseType<QuestionsResponse>(StatusCodes.Status200OK)]
    public ActionResult<QuestionsResponse> GetQuestions()
    {
        var questions = gameService.GetAllQuestions();
        var questionsResponse = new QuestionsResponse(
            questions.Count,
            questions.Select((q, index) => new QuestionSummary(
                index + 1,
                q.Text,
                q.Answers.Count,
                q.Answers.Sum(a => a.Points)
            )).ToList()
        );

        return Ok(questionsResponse);
    }
}
