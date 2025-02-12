using Microsoft.AspNetCore.Identity.UI.Services;
//using System.Net.Mail;
using MailKit.Net.Smtp;
using System.Net;
using MimeKit.Text;
using MimeKit;
using Org.BouncyCastle.Tls;
using Microsoft.AspNetCore.Identity;

using MailKit.Security;

namespace NewsProject.Services
{
    public class EmailSender : IEmailSender
    {
        public async Task SendEmailAsync(string email, string subject, string content)
        {
            string response = "";
            var message = new MimeMessage();
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            var config = builder.Build();
            // Load settings from appsettingsjson
            var smtpServer = config["SmtpServer"];
            var smtpPort = int.Parse(config["SmtpPort"]);
            var smtpUsername = config["SmtpUsername"];
            var smtpPassword = config["SmtpPassword"];
            //var senderEmail = config["SenderEmail"];
            //var senderName = _configuration["EmailSettings:SenderName"];

            //Create email message
            //message.Sender = MailboxAddress.Parse();
            //message.Sender.Name = senderName;
            message.From.Add(new MailboxAddress("Dragon", smtpUsername));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;
            message.Body = new TextPart(TextFormat.Html) { Text = content };
            using (var smtpClient = new SmtpClient())
            {

                // Bypass SSL certificate validation
                smtpClient.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;
                try
                {
                    //The last parameter here is to use SSL(Which you should!)
                    await smtpClient.ConnectAsync(smtpServer, smtpPort, SecureSocketOptions.SslOnConnect);
                }
                catch (SmtpCommandException ex)
                {
                    response = "Error trying to connect:" + ex.Message + " StatusCode: " + ex.StatusCode;
                    return;
                }
                //Remove any Auth functionality as we won't be using it.

                smtpClient.AuthenticationMechanisms.Remove("XOAUTH2"); // Disable OAuth2

                await smtpClient.AuthenticateAsync(smtpUsername, smtpPassword);
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
            return;        
        }
    }
}
