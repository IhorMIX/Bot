using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Bot.BLL.States;

public class UserStateService
{
    private readonly ConcurrentDictionary<long, BotState> _userStates = new();
    private readonly ConcurrentDictionary<long, Dictionary<string, string>> _userData = new();

    public BotState GetState(long chatId)
    {
        if (_userStates.TryGetValue(chatId, out var state))
            return state;
        return BotState.WaitingForPassport;
    }

    public void SetState(long chatId, BotState state)
    {
        _userStates.AddOrUpdate(chatId, state, (_, __) => state);
    }

    public void SetUserData(long chatId, string key, string value)
    {
        var dict = _userData.GetOrAdd(chatId, _ => new Dictionary<string, string>());
        dict[key] = value;
    }

    public string GetUserData(long chatId, string key)
    {
        if (_userData.TryGetValue(chatId, out var dict) && dict.TryGetValue(key, out var value))
            return value;
        return null;
    }
}