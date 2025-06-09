using System.Collections.Concurrent;

namespace Bot.BLL.States;

public class UserStateService
{
    private readonly ConcurrentDictionary<long, BotState> _userStates = new();

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
}