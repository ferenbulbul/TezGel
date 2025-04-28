using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using TezGel.Application.Interfaces.Services;
using TezGel.Domain.Common;

namespace TezGel.Infrastructure.Services
{
    public class MailService : IMailService
    {
        private readonly MailSettings _mailSettings;

        public MailService(IOptions<MailSettings> mailSettings)
        {
            _mailSettings = mailSettings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {

            var smtpClient = new SmtpClient(_mailSettings.SmtpServer)
            {
                Port = _mailSettings.Port,
                Credentials = new NetworkCredential(_mailSettings.From, _mailSettings.Password),
                EnableSsl = true,
                UseDefaultCredentials = false
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_mailSettings.From, _mailSettings.Username),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };
            mailMessage.To.Add(to);
            try
            {
                await smtpClient.SendMailAsync(mailMessage);
            }
            catch (Exception ex)
            {
                // Hata yönetimi
                throw new InvalidOperationException("E-posta gönderimi başarısız oldu", ex);
            }
        }
    }
}