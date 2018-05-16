using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace IdentityMvcWeb.IdentityConfig
{
    public class SmsService : IIdentityMessageService
    {
        public Task SendAsync(IdentityMessage message)
        {
            string twilioSid = ConfigurationManager.AppSettings["TwilioSid"];
            string twilioApiKey = ConfigurationManager.AppSettings["TwilioApiKey"];
            string twilioFromNr = ConfigurationManager.AppSettings["TwilioFromNr"];

            TwilioClient.Init(twilioSid, twilioApiKey);

            var fromNr = new Twilio.Types.PhoneNumber(twilioFromNr);
            var toNr = new Twilio.Types.PhoneNumber(message.Destination);
            var sms = MessageResource.Create(
                body: message.Body,
                from: fromNr,
                to: toNr,
                pathAccountSid: twilioSid
            );

            Trace.TraceInformation(sms.Status.ToString());

            // Twilio doesn't currently have an async API, so we return success.
            return Task.FromResult(0);
        }
    }
}