namespace Game100To1.Backend.Models;

/// <summary>
/// Конфигурация игровых вопросов
/// </summary>
public class GameQuestionsConfiguration
{
    /// <summary>
    /// Список всех вопросов игры
    /// </summary>
    public List<QuestionConfiguration> GameQuestions { get; set; } = new();
}

/// <summary>
/// Конфигурация отдельного вопроса
/// </summary>
public record QuestionConfiguration(string Text, List<AnswerConfiguration> Answers)
{
    public QuestionConfiguration() : this(string.Empty, new List<AnswerConfiguration>()) { }
}

/// <summary>
/// Конфигурация ответа
/// </summary>
public record AnswerConfiguration(string Text, int Points);