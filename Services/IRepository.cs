using Microsoft.AspNetCore.JsonPatch;
using PhotoGalleryAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Services
{
   public  interface IRepository
    {
         public Task<IEnumerable<SendGridDbModel>> GetEmails();
         public Task RecordReceivedEmail(SendGridDbModel sendGridDbSet);
         public SendGridDbModel GetMailById(int? mailId);
         public Task DeleteMailById(SendGridDbModel mailEntity);
         public Task UpdateMailById(SendGridDbModel mailEntity, JsonPatchDocument patchEntity);

        //sent mail methods
        public Task<IEnumerable<EmailDbModel>> GetSentEmails();
        public Task RecordSentMail(EmailDbModel sendGridDbSet);
        public EmailDbModel GetSentMailById(int? mailId);
        public Task DeleteSentMailById(EmailDbModel mailEntity);

        //push 
        public Task<List<PushSubscribeDbModel>> GetSubscribtions();


    }
}
