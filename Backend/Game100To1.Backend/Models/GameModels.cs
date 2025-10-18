namespace Game100To1.Backend.Models;

public class GameState
{
    public List<Team> Teams { get; set; } = new();
    public int CurrentRound { get; set; } = 1;
    public Question? CurrentQuestion { get; set; }
    public int RoundMultiplier { get; set; } = 1;
    public bool IsGameActive { get; set; } = false;
    public bool IsRoundActive { get; set; } = false;
    public List<int> RevealedAnswers { get; set; } = new();
    public GameMode CurrentMode { get; set; } = GameMode.Normal;
    public int RoundPoints { get; set; } = 0; // Накопленные очки за раунд
}

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Score { get; set; } = 0;
    public int Errors { get; set; } = 0;
}

public class Question
{
    public string Text { get; set; } = string.Empty;
    public List<Answer> Answers { get; set; } = new();
}

public class Answer
{
    public string Text { get; set; } = string.Empty;
    public int Points { get; set; }
}

public enum GameMode
{
    Normal,    // x1, x2, x3
    RareAnswer // Раунд "самый редкий ответ"
}
