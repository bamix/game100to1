namespace Game100To1.Backend.Models;

/// <summary>
/// Стандартный ответ API с сообщением
/// </summary>
public record ApiResponse();

/// <summary>
/// Ответ с состоянием игры
/// </summary>
public record GameStateResponse(GameState GameState) : ApiResponse;

/// <summary>
/// Ответ при запуске новой игры
/// </summary>
public record StartGameResponse(string Message) : ApiResponse
{
    public StartGameResponse() : this("Новая игра запущена") { }
}

/// <summary>
/// Ответ при запуске раунда
/// </summary>
public record StartRoundResponse(string Message) : ApiResponse
{
    public StartRoundResponse() : this("Раунд запущен") { }
}

/// <summary>
/// Ответ при переходе к следующему раунду
/// </summary>
public record NextRoundResponse(string Message) : ApiResponse
{
    public NextRoundResponse() : this("Переход к следующему раунду") { }
}

/// <summary>
/// Ответ при загрузке нового вопроса
/// </summary>
public record NextQuestionResponse(string Message) : ApiResponse
{
    public NextQuestionResponse() : this("Новый вопрос загружен") { }
}

/// <summary>
/// Ответ при открытии ответа
/// </summary>
public record RevealAnswerResponse(string Message, int AnswerIndex) : ApiResponse;

/// <summary>
/// Ответ при начислении очков
/// </summary>
public record AwardPointsResponse(string Message, int TeamId, int AnswerIndex) : ApiResponse;

/// <summary>
/// Ответ при добавлении ошибки
/// </summary>
public record AddErrorResponse(string Message, int TeamId) : ApiResponse;

/// <summary>
/// Ответ при удалении ошибки
/// </summary>
public record RemoveErrorResponse(string Message, int TeamId) : ApiResponse;

/// <summary>
/// Ответ при установке очков команды
/// </summary>
public record SetTeamScoreResponse(string Message, int TeamId, int Score) : ApiResponse;

/// <summary>
/// Ответ при установке названий команд
/// </summary>
public record SetTeamNamesResponse(string Message, string Team1, string Team2) : ApiResponse;

/// <summary>
/// Ответ при установке настроек раунда
/// </summary>
public record SetRoundSettingsResponse(string Message, int Multiplier, string Mode) : ApiResponse;
