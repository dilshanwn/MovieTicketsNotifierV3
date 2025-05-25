using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using MovieTicketsNotifierV3FuncApp.Models;
using MovieTicketsNotifierV3FuncApp.Responses;

namespace MovieTicketsNotifierV3FuncApp.Utils
{
    public static class SmtpUtil
    {
        public static async Task<bool> SendEmail(IConfiguration configuration, MovieShowTimeMatchFoundResponse Movie, string emailRecipient = null)
        {

            bool sendEmail = false;
            try
            {
                var EmailUserName = configuration["EmailUserName"];
                var EmailPassword = configuration["EmailPassword"];
                var EmailRecipient = emailRecipient ?? configuration["EmailRecipient"];

                var subject = $"{Movie?.Theater?.MovieName?.ToUpper()} - {Movie?.Experience?.ExperienceName} Tickets Released";
                var body = $"<h1>{subject}</h1><br><br>";

                var showtimes = Movie?.Experience?.Showtimes ?? new List<Showtime>();

                body += $"<h3>Show Times for {Movie?.Theater?.Name} on {Movie?.Theater?.Date} - {Movie?.Experience?.ExperienceName} </h3><br>";

                body += $"<ul>";
                foreach (var showtime in showtimes)
                {
                    var showtimeUrl = $"https://www.scopecinemas.com/seat-plan/{showtime.CinemaId}/{showtime.Id}";
                    body += $"<li> {showtime.ShowtimeName} - <a href=\"{showtimeUrl}\">Link</a> </li>";
                }
                body += $"</ul>";

                var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential(EmailUserName, EmailPassword),
                    EnableSsl = true,
                };
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(EmailUserName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(EmailRecipient);

                await smtpClient.SendMailAsync(mailMessage);

                sendEmail = true;
            }
            catch (Exception)
            {
                return false;
            }
            return sendEmail;

        }
    }
}
