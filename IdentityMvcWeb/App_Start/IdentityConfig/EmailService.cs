using System.Configuration;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace IdentityMvcWeb.IdentityConfig
{
    public class EmailService : IIdentityMessageService
    {
        public async Task SendAsync(IdentityMessage message)
        {
            var msg = new SendGridMessage();
            msg.AddTo(message.Destination);
            msg.From = new EmailAddress("noreply@identitymvcweb.com", "identity mvc web");
            msg.Subject = message.Subject;
            msg.PlainTextContent = message.Body;
            msg.HtmlContent = message.Body;

            var sendGridClient = new SendGridClient(ConfigurationManager.AppSettings["SendGridApiKey"]);
            await sendGridClient.SendEmailAsync(msg);
        }
    }
}