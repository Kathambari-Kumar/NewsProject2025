using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;

namespace MailToSubscriptionExpiryUsersFromQueue
{
    public class MailToSubscriptionExpiryUsers
    {
        private readonly IConfiguration _configuration;
        public MailToSubscriptionExpiryUsers(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [FunctionName("MailToSubscriptionExpiryUsers")]
        public Task Run([QueueTrigger("subscriptionexpiryqueue", Connection = "AzureWebJobsStorage")]QueueItemFM queueItem, ILogger log)
        {
            string response = "";
            log.LogInformation($"C# Queue trigger function processed: {queueItem}");
            MimeMessage message = new MimeMessage();
            message.To.Add(MailboxAddress.Parse(queueItem.Email));
            message.From.Add(new MailboxAddress("Digital Dragons", "digitaldragons571@gmail.com"));
            var expiryDate = queueItem.Expires.ToString("d");
            message.Subject = $"{queueItem.FullName}, your subscription is expiring on {expiryDate} – Act now!";
            //We will say we are sending HTML. But there are options for plaintext etc.
            var body = new StringBuilder();
            body.AppendLine("<div style=\"width:420px;\">");
            body.AppendLine("<hr/>");
            body.AppendLine("<img src='https://dragonsstorage24.blob.core.windows.net/dragoncontainer/Logo4.jpg' style=\"width:70px; height:70px\"/>");
            body.Append("<img src='https://dragonsstorage24.blob.core.windows.net/dragoncontainer/Slogon3.jpg' style=\"width:300px; height:50px\"/>");
            body.AppendLine("<hr/>");
            body.AppendFormat("<b>" + "Hello {0}\n", queueItem.FullName + "," + "<b>" + "<br/><br/>");
            body.AppendLine("We hope you’ve been enjoying your experience with Digital Dragons." +
                             " We wanted to remind you that your subscription will expire Soon." + "<br/>");
            body.AppendLine("<br/>");
            body.AppendLine("To ensure an uninterrupted service, simply renew your subscription by following these steps:" + "<br/>");
            body.AppendLine("<br/>");
            body.AppendLine("1.Log in to your account" + "<br/>");
            body.AppendLine("2.Go to the MyPage menu" + "<br/>");
            body.AppendLine("3.select Upgrade Subscription" + "<br/><br/>");
            body.AppendLine("<div style=\"background-color:whitesmoke\">");
            body.AppendLine("<br/>" + "Available Subscription List : " + "<br/>");
            body.AppendLine("<hr/>");
            body.AppendLine("<table>");
            body.AppendLine("<tr>");
            body.AppendLine("<td style=\"width:150px;\">" + "TypeName" + "</td>");
            body.AppendLine("<td style=\"width:200px;\">" + "Description" + "</td>");
            body.AppendLine("<td style=\"width:50px;\">" + "Price" + "</td>");
            body.AppendLine("</tr>");
            body.AppendLine("<body>");
            foreach (var item in queueItem.SubscriptionTypes)
            {
                body.AppendLine("<tr>");
                body.AppendLine("<td>" + item.TypeName + "</td>");
                body.AppendLine("<td>" + item.Description + "</td>");
                body.AppendLine("<td>" + item.Price + "<td>");
                body.AppendLine("</tr>");
                body.AppendLine("<br/>");
            }
            body.AppendLine("</body>");
            body.AppendLine("</table>");
            body.AppendLine("</div>");
            body.AppendLine("<br/>");
            body.AppendLine("If you have any questions or need assistance, please don’t hesitate to contact our support team.");
            body.AppendLine("<br/>");
            body.AppendLine("Best Regadrs" + "<br/>");
            body.AppendLine("Digital Dragons Team" + "<br/>");
            body.AppendLine("<hr/>");
            body.AppendLine("<b>Visit our website : </b>" + "<a href=\"\">" + "Digital Drgons " + "</a>");
            body.AppendLine("<hr/>");
            body.AppendLine("</div>");
            string content = body.ToString();
            message.Body = new TextPart(TextFormat.Html) { Text = content };
            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new SmtpClient())
            {
                try
                {
                    //The last parameter here is to use SSL (Which you should!)
                    emailClient.Connect(_configuration["SmtpServer"], Convert.ToInt32(_configuration["SmtpPort"]), true);
                }
                catch (SmtpCommandException ex)
                {
                    response = "Error trying to connect:" + ex.Message + " StatusCode: " + ex.StatusCode;
                    return Task.FromResult(response);
                }
                catch (SmtpProtocolException ex)
                {
                    response = "Protocol error while trying to connect:" + ex.Message;
                    return Task.FromResult(response);
                }
                //Remove any OAuth functionality as we won't be using it.
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
                emailClient.Authenticate(_configuration["SmtpUsername"], _configuration["SmtpPassword"]);
                try
                {
                    emailClient.Send(message);
                }
                catch (SmtpCommandException ex)
                {
                    response = "Error sending message: " + ex.Message + " StatusCode: " + ex.StatusCode;
                    switch (ex.ErrorCode)
                    {
                        case SmtpErrorCode.RecipientNotAccepted:
                            response += " Recipient not accepted: " + ex.Mailbox;
                            break;

                        case SmtpErrorCode.SenderNotAccepted:
                            response += " Sender not accepted: " + ex.Mailbox;
                            Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);
                            break;

                        case SmtpErrorCode.MessageNotAccepted:
                            response += " Message not accepted.";
                            break;
                    }
                }
                emailClient.Disconnect(true);
            }
            return Task.CompletedTask;
        }
    }
}
