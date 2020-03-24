using MailKit.Net.Smtp;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OXG.CRM_System.Data
{
    public static class EmailService
    {
        public static async Task SendEmailAsync(string email, string subject, string message, string filepath)
        {
            var emailMessage = new MimeMessage();

            emailMessage.From.Add(new MailboxAddress("ООО 'Рога и копыта'", "semes@gmail.com"));
            emailMessage.To.Add(new MailboxAddress("", email));
            emailMessage.Subject = subject;
            var builder = new BodyBuilder();
            builder.Attachments.Add(filepath);
            builder.TextBody = message;
            emailMessage.Body = builder.ToMessageBody();//new TextPart(MimeKit.Text.TextFormat.Html)
            //{
            //    Text = message
            //};

            

            using (var client = new SmtpClient())
            {
                await client.ConnectAsync("smtp.gmail.com", 587, false); //либо использум порт 465 587
                await client.AuthenticateAsync("semes212@gmail.com", "1425367980"); //логин-пароль от аккаунта
                await client.SendAsync(emailMessage);

                await client.DisconnectAsync(true);
            }

            
        }
    }
}
