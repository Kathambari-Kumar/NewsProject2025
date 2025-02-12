using System;
using System.Net.Http;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;

namespace UpgradeQueueTriggered
{
    public class SendConfirmation
    {

        private readonly IConfiguration _configuration;
        public SendConfirmation(IConfiguration configuration)
        {
            _configuration = configuration;

        }

        [FunctionName("SendConfirmation")]

        public async Task Run([QueueTrigger("upgradesubscriptionqueue", Connection = "AzureWebJobsStorage")] MessageFM myQueueItem, ILogger log)
        {

            log.LogInformation($"C# Queue trigger function processed: {myQueueItem}");
            log.LogInformation($"Name : {myQueueItem.Name.ToString()}");
            log.LogInformation($"Email :{myQueueItem.Email.ToString()}");

            if (string.IsNullOrWhiteSpace(myQueueItem.Name) || string.IsNullOrWhiteSpace(myQueueItem.Email))
            {
                log.LogError("Invalid message received: Missing Name or Email.");
                return;
            }


            string response = "";
            var message = new MimeMessage();

            message.Sender = MailboxAddress.Parse("digitaldragons571@gmail.com");
            message.Sender.Name = "Dragon news site";
            message.To.Add(MailboxAddress.Parse(myQueueItem.Email));
            message.From.Add(message.Sender);
            message.Subject = "Subscription Upgrade";
            message.Body = new TextPart(TextFormat.Html) { Text = $"Thank you {myQueueItem.Name} for upgrading your current subscription" };


            //Be careful that the SmtpClient class is the one from Mailkit not the framework!

            using (var smtpClient = new SmtpClient())
            {


                try

                {
                    // Ignore certificate validation errors by setting the callback to always return true
                    smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

                    //The last parameter here is to use SSL (Which you should!)

                    await smtpClient.ConnectAsync(_configuration["SmtpServer"], Convert.ToInt32(_configuration["SmtpPort"]), true);

                }
                catch (SmtpCommandException ex)
                {

                    response = "Error trying to connect:" + ex.Message + " StatusCode: " + ex.StatusCode;

                    return;
                }
                catch (SmtpProtocolException ex)
                {

                    response = "Protocol error while trying to connect:" + ex.Message;

                    return;

                }

                //Remove any OAuth functionality as we won't be using it.

                smtpClient.AuthenticationMechanisms.Remove("XOAUTH2");

                smtpClient.Authenticate(_configuration["SmtpUsername"], _configuration["SmtpPassword"]);


                try

                {


                    await smtpClient.SendAsync(message);
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

                await smtpClient.DisconnectAsync(true);



            }




        }

    }
}
