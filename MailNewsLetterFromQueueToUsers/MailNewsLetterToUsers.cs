using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;

namespace MailNewsLetterFromQueueToUsers
{
    public class MailNewsLetterToUsers
    {
        private readonly IConfiguration _configuration;
        public MailNewsLetterToUsers(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        [FunctionName("MailNewsLetterToUsers")]
        public Task Run([QueueTrigger("newsletterqueue", Connection = "AzureWebJobsStorage")]QueueItemFM queueItem, ILogger log)
        {
            string response = "";
            log.LogInformation($"C# Queue trigger function processed: {queueItem}");
            MimeMessage message = new MimeMessage();
            message.To.Add(MailboxAddress.Parse(queueItem.Email));
            message.From.Add(new MailboxAddress("Digital Dragons", "digitaldragons571@gmail.com"));
            message.Subject = "NewsLetter From Digital Dragons : " + DateTime.Now.ToString("d");
            //We will say we are sending HTML. But there are options for plaintext etc.
            var body = new StringBuilder();
            
            body.AppendLine("<hr/>");
            body.AppendLine("<img src='https://dragonsstorage24.blob.core.windows.net/dragoncontainer/Logo4.jpg' style=\"width:70px; height:70px\"/>");
            body.Append("<img src='https://dragonsstorage24.blob.core.windows.net/dragoncontainer/Slogon3.jpg' style=\"width:300px; height:50px\"/>");
            body.AppendLine("<hr/>");
            body.AppendFormat("<b>" + "Hello {0}\n", queueItem.FullName + "," + "<b>" + "<br/><br/>");
            body.AppendLine("Welcome to the latest edition of our weekly newsletter!" +
                " We’re excited to share with you some exciting updates from our websites." + "<br/><br/>");
            body.AppendLine("<div style=\"background-color:whitesmoke\">");

            foreach (var item in queueItem.NewsLetterArticles)
            {
                body.AppendLine("<a href=\"https://digitaldragons.azurewebsites.net/NewsArticle/DetailNewsDisplay?id="+item.Id+"\">" + "<strong>" + item.LinkText + "</strong>" + "</a>" + "<br/>");
                body.AppendLine(item.ContentSummary + "<br/><br/>");
                body.AppendLine("<b>" + "Article written by : " + item.Author + "</b>" + "<br/><br/>");
            }
            body.AppendLine("</div>");
            body.AppendLine("<hr/>");
            body.AppendLine("Best Regadrs" + "<br/>");
            body.AppendLine("Digital Dragons Team" + "<br/>");
            body.AppendLine("<hr/>");
            body.AppendLine("<b>For more News, visit our website : </b>" + "<a href=\"https://digitaldragons.azurewebsites.net\">" + "Digital Drgons " + "</a>");
            body.AppendLine("<hr/>");

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
