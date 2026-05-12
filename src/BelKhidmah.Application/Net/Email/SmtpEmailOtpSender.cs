using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Dependency;
using BelKhidmah.Otp;
using Castle.Core.Logging;
using Microsoft.Extensions.Configuration;

namespace BelKhidmah.Net.Email
{
    public class SmtpEmailOtpSender : IEmailOtpSender, ITransientDependency
    {
        public ILogger Logger { get; set; } = NullLogger.Instance;

        private readonly IConfiguration _configuration;

        public SmtpEmailOtpSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendAsync(string recipient, string code, string template = null)
        {
            try
            {
                var smtp = _configuration.GetSection("Email:Smtp");
                var host = smtp["Host"];
                var port = int.Parse(smtp["Port"] ?? "587");
                var username = smtp["Username"];
                var password = smtp["Password"];
                var ssl = bool.Parse(smtp["EnableSsl"] ?? "true");
                var from = _configuration["Email:From"];
                var fromName = _configuration["Email:FromName"];

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = ssl,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false
                };

                var body = $@"
<div style=""font-family:Arial,sans-serif;max-width:480px;margin:0 auto;"">
  <h2 style=""color:#333;"">Your Verification Code</h2>
  <p style=""font-size:32px;font-weight:bold;letter-spacing:8px;color:#8B1874;"">{code}</p>
  <p style=""color:#666;"">This code expires in 5 minutes. Do not share it with anyone.</p>
</div>";

                using var message = new MailMessage
                {
                    From = new MailAddress(from, fromName),
                    Subject = "Verification Code",
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(recipient);

                await client.SendMailAsync(message);
            }
            catch (Exception ex)
            {

                throw;
            }
            
        }
    }
}
