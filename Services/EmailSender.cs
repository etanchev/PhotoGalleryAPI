using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Services
{
    public class EmailSender
    {
        readonly private IWebHostEnvironment _env;
        readonly private ILogger<EmailSender> _logger;
        public EmailSender(IWebHostEnvironment env, ILogger<EmailSender> logger)
        {
            _env = env;
            _logger = logger;
        }
        public async Task SendGridSMTP(string apikey, string subject, string body, string emailTo, string attachments)
        {

            //MailAddress from = new MailAddress("mymail@webmail.PhotoGallery.photography") { };
            MailAddress from = new MailAddress("mymail@PhotoGallery.photography") { };
            MailAddress to = new MailAddress(emailTo);


            MailMessage mailMessage = new MailMessage(from, to)
            {

                From = from,
                Subject = subject,
                Body = body,

                IsBodyHtml = true

            };

            if(attachments != null)
            {
                List<string> fileNames = attachments.Split(";").ToList(); ;

                foreach (var fileName in fileNames)
                {

                    if (!String.IsNullOrEmpty(fileName))
                    {
                        string path = Path.Combine(_env.WebRootPath, "Attachments", fileName);

                        Attachment data = new Attachment(path, MediaTypeNames.Application.Octet);

                        // Add time stamp information for the file.
                        ContentDisposition disposition = data.ContentDisposition;
                        disposition.CreationDate = System.IO.File.GetCreationTime(fileName);
                        disposition.ModificationDate = System.IO.File.GetLastWriteTime(fileName);
                        disposition.ReadDate = System.IO.File.GetLastAccessTime(fileName);
                        // Add the file attachment to this email message.
                        mailMessage.Attachments.Add(data);
                    }
                }
            }



            // Create  the file attachment for this email message.


            SmtpClient smtpClient = new SmtpClient()
            {
                Port = 587,
                Host = "smtp.sendgrid.net",
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("apikey", apikey),
            };

           await  smtpClient.SendMailAsync(mailMessage);

        }

        public async Task<bool> SendBlazorEmail(string apikey, string subject, string body, string emailTo)
        {

         
            MailAddress from = new MailAddress("picked-images@PhotoGallery.photography") { };
            MailAddress to = new MailAddress(emailTo);
            MailAddress carbonCopy1 = new MailAddress("mymail@PhotoGallery.photography") { };

            MailMessage mailMessage = new MailMessage(from, to)
            {

                From = from,
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
               
            };

           
            mailMessage.CC.Add(carbonCopy1);


            SmtpClient smtpClient = new SmtpClient()
            {
                Port = 587,
                Host = "smtp.sendgrid.net",
                Timeout = 10000,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("apikey", apikey),
            };


            //smtpClient.SendCompleted += SmtpClient_SendCompleted;        

            try
            {
                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception e)
            {

                _logger.LogInformation(e.ToString());
                return false;
            }
           

        }

        //private void SmtpClient_SendCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        //{
          
        //}
    }
}
