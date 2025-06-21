using Microsoft.Extensions.Logging;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace Core_API.Infrastructure.External.SMS
{
    //public class TwilioService : ISmsSender
    //{
    //    private readonly SMSSettings _smsSettings;
    //    private readonly ILogger<TwilioService> _logger;
    //    public TwilioService(IOptions<SMSSettings> smsSettings, ILogger<TwilioService> logger)
    //    {
    //        _smsSettings = smsSettings.Value;
    //        _logger = logger;
    //        if (string.IsNullOrEmpty(_smsSettings.AccountSID) ||
    //          string.IsNullOrEmpty(_smsSettings.AuthToken))
    //        {
    //            throw new ArgumentNullException("Twilio credentials are not configured");
    //        }

    //        TwilioClient.Init(_smsSettings.AccountSID, _smsSettings.AuthToken);
    //    }

    //    public async Task SendSmsAsync(string number, string message)
    //    {
    //        try
    //        {
    //            var response = await MessageResource.CreateAsync(
    //          to: new Twilio.Types.PhoneNumber(number),
    //          from: new Twilio.Types.PhoneNumber(_smsSettings.FromNumber),
    //          body: message);

    //            _logger.LogInformation($"SMS sent to {number}. Status: {response.Status}");
    //        }
    //        catch (Exception ex)
    //        {
    //            _logger.LogError(ex, $"Failed to send SMS to {number}");
    //            throw new Exception($"Failed to send SMS: {ex.Message}");
    //        }
    //    }
    //}
}
