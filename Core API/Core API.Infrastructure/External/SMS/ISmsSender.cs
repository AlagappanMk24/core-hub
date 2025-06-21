namespace Core_API.Infrastructure.External.SMS
{
    public interface ISmsSender
    {
        Task SendSmsAsync(string number, string message);
    }
}