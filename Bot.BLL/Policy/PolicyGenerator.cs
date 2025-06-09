using System;

namespace Bot.BLL.Policy;

public class PolicyGenerator
{
    public string GeneratePolicy(string clientName, string vin)
    {
        return
            $"📝 Insurance policy\nName: {clientName}\nVIN: {vin}\nSum: 100 USD\nDate: {DateTime.UtcNow:dd.MM.yyyy}";
    }
}