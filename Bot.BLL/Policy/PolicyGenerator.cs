namespace Bot.BLL.Policy;

public class PolicyGenerator
{
    public string GeneratePolicy(string clientName, string vin)
    {
        var dateIssued = DateTime.UtcNow.ToString("dd.MM.yyyy");
        var policyNumber = Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        var insuredAmount = "100 USD";

        return $"🚘 *Car Insurance Policy*\n\n" +
               $"*Policy Number:* `{policyNumber}`\n" +
               $"*Date Issued:* `{dateIssued}`\n" +
               $"*Client Name:* `{clientName}`\n" +
               $"*Vehicle VIN:* `{vin}`\n" +
               $"*Insured Amount:* `{insuredAmount}`\n\n" +
               $"Thank you for choosing our services. Drive safely! 🚗";
    }
}