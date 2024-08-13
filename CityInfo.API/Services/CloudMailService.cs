namespace CityInfo.API.Services;

public class CloudMailService : IMailService
{
    private readonly string _mailTo;
    private readonly string _mailFrom;

    public CloudMailService(IConfiguration configuration)
    {
        _mailTo = configuration["mailSettings:mailTo"];
        _mailFrom = configuration["mailSettings:mailFrom"];
    }
    
    public void Send(string subject, string message)
    {
        Console.WriteLine($"Mail from {_mailFrom} to {_mailTo}, with {nameof(CloudMailService)}.");
        Console.WriteLine($"Subject: {subject}");
        Console.WriteLine($"Message: {message}");
    }
}