namespace CityInfo.API.Services;

public class LocalMailService : IMailService
{
    private readonly string _mailTo;
    private readonly string _mailFrom;

    public LocalMailService(IConfiguration configuration)
    {
        _mailTo = configuration["mailSettings:mailTo"];
        _mailFrom = configuration["mailSettings:mailFrom"];
    }
    public void Send(string subject, string message)
    {
        Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, with {nameof(LocalMailService)}.");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Message: {message}");
    }
}