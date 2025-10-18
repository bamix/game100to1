namespace Game100To1.Backend.Models;

/// <summary>
/// Запрос для установки настроек раунда
/// </summary>
public record SetRoundSettingsRequest(int Multiplier, int Mode)
{
    public SetRoundSettingsRequest() : this(1, 0) { }
}