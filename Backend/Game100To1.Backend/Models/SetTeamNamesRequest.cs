namespace Game100To1.Backend.Models;

/// <summary>
/// Запрос для установки названий команд
/// </summary>
public record SetTeamNamesRequest(string Team1Name, string Team2Name)
{
    public SetTeamNamesRequest() : this(string.Empty, string.Empty) { }
}