using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SIIR.Utilities
{
    public class EmailSender : IEmailSender
    {
        private string _smtpServer;
        private int _smtpPort;
        private string _fromEmailAddress;
        private string _fromEmailPassword;

        public EmailSender(string smtpServer, int smtpPort, string fromEmailAddress, string fromEmailPassword)
        {
            _smtpServer = smtpServer;
            _smtpPort = smtpPort;
            _fromEmailAddress = fromEmailAddress;
            _fromEmailPassword = fromEmailPassword;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            var smtp = new SmtpClient
            {
                Host = _smtpServer,
                Port = _smtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_fromEmailAddress, _fromEmailPassword)
            };

            using (var message = new MailMessage(_fromEmailAddress, toEmail))
            {
                message.Subject = subject;
                message.Body = htmlMessage;
                message.IsBodyHtml = true;
                await smtp.SendMailAsync(message);
            }
        }
    }
}
