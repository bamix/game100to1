using Microsoft.AspNetCore.SignalR;
using Game100To1.Backend.Services;

namespace Game100To1.Backend.Hubs;

public class GameHub : Hub
{
    private readonly GameService _gameService;

    public GameHub(GameService gameService)
    {
        _gameService = gameService;
    }

    public override async Task OnConnectedAsync()
    {
        // Отправить текущее состояние игры новому клиенту
        await Clients.Caller.SendAsync("GameStateChanged", _gameService.GetGameState());
        await base.OnConnectedAsync();
    }

    public async Task JoinGame()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "GameRoom");
        await Clients.Caller.SendAsync("GameStateChanged", _gameService.GetGameState());
    }

    public async Task NotifyStateChanged()
    {
        await Clients.All.SendAsync("GameStateChanged", _gameService.GetGameState());
    }

    public async Task NotifyAnswerRevealed(int answerIndex)
    {
        await Clients.All.SendAsync("AnswerRevealed", answerIndex);
    }

    public async Task NotifyScoreUpdated()
    {
        await Clients.All.SendAsync("ScoreUpdated", _gameService.GetGameState().Teams);
    }

    public async Task NotifyErrorsUpdated()
    {
        await Clients.All.SendAsync("ErrorsUpdated", _gameService.GetGameState().Teams);
    }

    public async Task NotifyRoundStarted()
    {
        await Clients.All.SendAsync("RoundStarted", _gameService.GetGameState());
    }

    public async Task NotifyGameReset()
    {
        await Clients.All.SendAsync("GameReset", _gameService.GetGameState());
    }
}