using Game100To1.Backend.Models;
using Microsoft.Extensions.Options;

namespace Game100To1.Backend.Services;

public class GameService
{
    private readonly GameState gameState = new();
    private List<Question> questions = new();
    private readonly Random random = new();
    private readonly GameQuestionsConfiguration questionsConfig;

    public GameService(IOptions<GameQuestionsConfiguration> questionsConfig)
    {
        this.questionsConfig = questionsConfig.Value;
        LoadQuestions();
        InitializeTeams();
    }

    public GameState GetGameState() => this.gameState;

    public List<Question> GetAllQuestions() => this.questions;

    public void InitializeTeams()
    {
        this.gameState.Teams =
        [
            new() { Id = 1, Name = "Команда 1", Score = 0, Errors = 0 },
            new() { Id = 2, Name = "Команда 2", Score = 0, Errors = 0 }
        ];
    }

    public void SetTeamNames(string team1Name, string team2Name)
    {
        this.gameState.Teams[0].Name = team1Name;
        this.gameState.Teams[1].Name = team2Name;
    }

    public void SetRoundSettings(int multiplier, GameMode mode)
    {
        this.gameState.RoundMultiplier = multiplier;
        this.gameState.CurrentMode = mode;
    }

    public void StartNewGame()
    {
        this.gameState.CurrentRound = 1;
        this.gameState.IsGameActive = true;
        this.gameState.IsRoundActive = true; // Сразу активируем раунд
        this.gameState.RevealedAnswers.Clear();
        this.gameState.CurrentMode = GameMode.Normal;
        this.gameState.RoundMultiplier = 1;
        this.gameState.RoundPoints = 0; // Сброс счетчика очков раунда

        foreach (var team in this.gameState.Teams)
        {
            team.Score = 0;
            team.Errors = 0;
        }

        LoadRandomQuestion();
    }

    public void StartRound()
    {
        this.gameState.IsRoundActive = true;
        this.gameState.RevealedAnswers.Clear();

        // Сброс ошибок для всех команд
        foreach (var team in this.gameState.Teams)
        {
            team.Errors = 0;
        }

        // Множитель и режим теперь устанавливаются вручную админом
        // Не изменяем RoundMultiplier и CurrentMode автоматически
    }

    public void NextRound()
    {
        if (this.gameState.CurrentRound < 5)
        {
            this.gameState.CurrentRound++;
            this.gameState.IsRoundActive = true; // Сразу активируем раунд
            
            // Сбрасываем раскрытые ответы
            this.gameState.RevealedAnswers.Clear();
            
            // Сбрасываем счетчик очков раунда
            this.gameState.RoundPoints = 0;
            
            // Сбрасываем ошибки команд
            foreach (var team in this.gameState.Teams)
            {
                team.Errors = 0;
            }
            
            LoadRandomQuestion();
        }
    }

    public void NextQuestion()
    {
        this.gameState.IsRoundActive = false;
        LoadRandomQuestion();
    }

    public void RevealAnswer(int answerIndex)
    {
        if (answerIndex >= 0 && answerIndex < (this.gameState.CurrentQuestion?.Answers.Count ?? 0))
        {
            if (!this.gameState.RevealedAnswers.Contains(answerIndex))
            {
                this.gameState.RevealedAnswers.Add(answerIndex);
                
                // Добавляем очки к счетчику раунда
                var answer = this.gameState.CurrentQuestion.Answers[answerIndex];
                this.gameState.RoundPoints += answer.Points;
            }
        }
    }

    public void RevealAnswerWithoutPoints(int answerIndex)
    {
        if (answerIndex >= 0 && answerIndex < (this.gameState.CurrentQuestion?.Answers.Count ?? 0))
        {
            if (!this.gameState.RevealedAnswers.Contains(answerIndex))
            {
                this.gameState.RevealedAnswers.Add(answerIndex);
                // Не добавляем очки к счетчику раунда
            }
        }
    }

    public void AwardRoundPoints(int teamId)
    {
        var team = this.gameState.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team != null)
        {
            // Присваиваем все накопленные очки раунда команде с применением множителя
            int finalPoints = this.gameState.RoundPoints * this.gameState.RoundMultiplier;
            team.Score += finalPoints;
            
            // Сбрасываем счетчик очков раунда
            this.gameState.RoundPoints = 0;
        }
    }

    public void AddError(int teamId)
    {
        var team = this.gameState.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team != null && team.Errors < 3)
        {
            team.Errors++;
        }
    }

    public void RemoveError(int teamId)
    {
        var team = this.gameState.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team != null && team.Errors > 0)
        {
            team.Errors--;
        }
    }

    public void SetTeamScore(int teamId, int newScore)
    {
        var team = this.gameState.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team != null)
        {
            team.Score = Math.Max(0, newScore); // Не позволяем отрицательные очки
        }
    }

    private void LoadQuestions()
    {
        try
        {
            if (this.questionsConfig.GameQuestions?.Any() == true)
            {
                this.questions = this.questionsConfig.GameQuestions.Select(q => new Question
                {
                    Text = q.Text,
                    Answers = q.Answers.Select(a => new Answer
                    {
                        Text = a.Text,
                        Points = a.Points
                    }).ToList()
                }).ToList();
            }
            else
            {
                // Fallback to default questions if configuration is empty
                this.questions = GetDefaultQuestions();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading questions from configuration: {ex.Message}");
            // Fallback questions if configuration fails
            this.questions = GetDefaultQuestions();
        }
    }

    private void LoadRandomQuestion()
    {
        if (this.questions.Any())
        {
            var randomIndex = this.random.Next(this.questions.Count);
            this.gameState.CurrentQuestion = this.questions[randomIndex];
        }
    }

    private List<Question> GetDefaultQuestions()
    {
        return new List<Question>
        {
            new()
            {
                Text = "Что делают утром после пробуждения?",
                Answers = new List<Answer>
                {
                    new() { Text = "Чистят зубы", Points = 30 },
                    new() { Text = "Завтракают", Points = 25 },
                    new() { Text = "Умываются", Points = 20 },
                    new() { Text = "Пьют кофе", Points = 15 },
                    new() { Text = "Делают зарядку", Points = 10 }
                }
            },
            new()
            {
                Text = "Что можно увидеть в парке?",
                Answers = new List<Answer>
                {
                    new() { Text = "Деревья", Points = 40 },
                    new() { Text = "Скамейки", Points = 30 },
                    new() { Text = "Фонтан", Points = 15 },
                    new() { Text = "Детей", Points = 10 },
                    new() { Text = "Собак", Points = 5 }
                }
            }
        };
    }
}
