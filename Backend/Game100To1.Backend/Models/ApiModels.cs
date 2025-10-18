namespace Game100To1.Backend.Models;

/// <summary>
/// Информация о SignalR Hub и событиях
/// </summary>
public record SignalRInfo(
    string HubUrl,
    List<SignalREvent> Events,
    List<SignalRMethod> Methods
)
{
    public SignalRInfo() : this("/gamehub", new List<SignalREvent>(), new List<SignalRMethod>()) { }
}

/// <summary>
/// Информация о SignalR событии
/// </summary>
public record SignalREvent(string Name, string Description);

/// <summary>
/// Информация о методе SignalR Hub
/// </summary>
public record SignalRMethod(string Name, string Description);

/// <summary>
/// Ответ со списком всех вопросов
/// </summary>
public record QuestionsResponse(int TotalCount, List<QuestionSummary> Questions);

/// <summary>
/// Краткая информация о вопросе
/// </summary>
public record QuestionSummary(int Id, string Text, int AnswersCount, int TotalPoints);