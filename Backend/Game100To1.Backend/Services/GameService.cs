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

    public GameStateResponse GetGameState() => new()
    {
        Teams = this.gameState.Teams,
        RoundPoints = this.gameState.RoundPoints,
        CurrentQuestion = this.gameState.CurrentQuestion == null
            ? null
            : this.gameState.CurrentMode == GameMode.Normal
                ? this.gameState.CurrentQuestion
                : new Question
                {
                    Text = this.gameState.CurrentQuestion.Text,
                    Answers = this.gameState.CurrentQuestion.Answers.Select((answer, i) => new Answer { Text = answer.Text, Points = CalculateRareAnswerPoints(i) }).ToList()
                },
        RevealedAnswers = this.gameState.RevealedAnswers.Select(a => a.Key).ToList(),
        CurrentMode = this.gameState.CurrentMode,
        CurrentRound = this.gameState.CurrentRound,
        IsGameActive = this.gameState.IsGameActive,
        IsRoundActive = this.gameState.IsRoundActive,
        RoundMultiplier = this.gameState.RoundMultiplier
    };

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

    public void RevealAnswer(int answerIndex, bool countPoints)
    {
        if (this.gameState.CurrentQuestion?.Answers != null &&
            answerIndex >= 0 && answerIndex < this.gameState.CurrentQuestion.Answers.Count)
        {
            this.gameState.RevealedAnswers.TryAdd(answerIndex, countPoints);
        }

        if (this.gameState.CurrentQuestion == null)
        {
            this.gameState.RoundPoints = 0;
            return;
        }

        if (this.gameState.CurrentMode == GameMode.Normal)
        {
            this.gameState.RoundPoints = 0;
            var answersToCount = this.gameState.RevealedAnswers.Where(a => a.Value).Select(a => a.Key).ToHashSet();
            foreach (var i in answersToCount)
            {
                this.gameState.RoundPoints += this.gameState.CurrentQuestion.Answers[i].Points;
            }
        }
        else
        {
            this.gameState.RoundPoints = this.gameState.RevealedAnswers.Any()
                ? CalculateRareAnswerPoints(this.gameState.RevealedAnswers.Where(a => a.Value).Max(x => x.Key))
                : 0;
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

    private int CalculateRareAnswerPoints(int index)
    {
        return index switch
        {
            0 => 15,
            1 => 30,
            2 => 60,
            3 => 120,
            4 => 180,
            5 => 240
        };
    }
}
