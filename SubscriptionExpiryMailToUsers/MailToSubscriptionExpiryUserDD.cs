using System;
using Azure.Storage.Queues.Models;
using System.Text;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using MimeKit;
using MimeKit.Text;
using Microsoft.Extensions.Configuration;
using MailKit.Net.Smtp;

namespace SubscriptionExpiryMailToUsers
{
    public class MailToSubscriptionExpiryUsersDD
    {
        private readonly IConfiguration _configuration;
        public MailToSubscriptionExpiryUsersDD(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [FunctionName("MailToSubscriptionExpiryUsersDD")]
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
            body.AppendFormat("<b>" + "Hello {0}\n", queueItem.FullName + "," + "<b>" + "<br/><br/>");
            foreach (var item in queueItem.SubscriptionTypes)
            {
                body.AppendLine(item.TypeName);
                body.AppendLine(item.Description);
                body.AppendLine(item.Price.ToString());
            }
            string content = body.ToString();
            message.Body = new TextPart(TextFormat.Html) { Text = content };
            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            using (var emailClient = new SmtpClient())
            {
                try
                {
                    emailClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
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
