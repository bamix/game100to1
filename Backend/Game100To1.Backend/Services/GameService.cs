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

    public void StartNewGame()
    {
        this.gameState.CurrentRound = 1;
        this.gameState.IsGameActive = true;
        this.gameState.IsRoundActive = false;
        this.gameState.RevealedAnswers.Clear();
        this.gameState.CurrentMode = GameMode.Normal;
        this.gameState.RoundMultiplier = 1;

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

        // Установка множителя в зависимости от раунда
        this.gameState.RoundMultiplier = this.gameState.CurrentRound switch
        {
            1 or 2 => 1,
            3 => 2,
            4 => 3,
            5 => 1, // Для раунда "редкий ответ"
            _ => 1
        };

        // Установка режима игры
        this.gameState.CurrentMode = this.gameState.CurrentRound == 5 ? GameMode.RareAnswer : GameMode.Normal;
    }

    public void NextRound()
    {
        if (this.gameState.CurrentRound < 5)
        {
            this.gameState.CurrentRound++;
            this.gameState.IsRoundActive = false;
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
            }
        }
    }

    public void AwardPoints(int teamId, int answerIndex)
    {
        var team = this.gameState.Teams.FirstOrDefault(t => t.Id == teamId);
        if (team != null && this.gameState.CurrentQuestion != null &&
            answerIndex >= 0 && answerIndex < this.gameState.CurrentQuestion.Answers.Count)
        {
            var answer = this.gameState.CurrentQuestion.Answers[answerIndex];
            int points = answer.Points * this.gameState.RoundMultiplier;

            // В режиме "редкий ответ" очки получает команда с самым низким ответом
            if (this.gameState.CurrentMode == GameMode.RareAnswer)
            {
                // Найти самый низкий открытый ответ
                var revealedAnswers = this.gameState.RevealedAnswers
                    .Select(i => this.gameState.CurrentQuestion.Answers[i])
                    .ToList();

                if (revealedAnswers.Any())
                {
                    var lowestAnswer = revealedAnswers.OrderBy(a => a.Points).First();
                    if (answer.Points == lowestAnswer.Points)
                    {
                        team.Score += answer.Points; // В этом режиме множитель не применяется
                    }
                }
            }
            else
            {
                team.Score += points;
            }
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
