using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using MimeKit.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;

namespace Ddi.Registry.Web.Services
{
    public class EmailSender : IEmailSender
    {
        EmailConfiguration c;
        public EmailSender(EmailConfiguration configuration)
        {
            this.c = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(c.FromName, c.FromAddress));
            message.To.Add(MailboxAddress.Parse(email));
            message.Subject = subject;

            message.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

            using (var client = new SmtpClient())
            {
                //client.SslProtocols = SslProtocols.Ssl3 | SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12 | SslProtocols.Tls13;
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                await client.ConnectAsync(c.Host, c.Port, c.EnableSSL);
                await client.AuthenticateAsync(c.UserName, c.Password);
                await client.SendAsync(message);
                await client.DisconnectAsync(true);
            }
        }
    }

    public class EmailConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string FromName { get; set; }
        public string FromAddress { get; set; }
    }
}
