using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using PhotoGalleryAPI.Data;
using PhotoGalleryAPI.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Services
{
    public class DbRepository : IRepository ,IRepositoryAdmin
    {
        private readonly ApplicationDbContext _db;
        public DbRepository(ApplicationDbContext dbContext)
        {
            _db = dbContext;
        }
        public async Task DeleteMailById(SendGridDbModel mailEntity)
        {
            _db.SendGridDbModel.Remove(mailEntity);
            await _db.SaveChangesAsync();

        }

        public async Task UpdateMailById(SendGridDbModel mailEntity, JsonPatchDocument patchEntity)
        {
            patchEntity.ApplyTo(mailEntity);
            await _db.SaveChangesAsync();

        }

        public async Task<IEnumerable<SendGridDbModel>> GetEmails() => await _db.SendGridDbModel.OrderByDescending(x => x).ToListAsync();

        public SendGridDbModel GetMailById(int? mailId) => _db.SendGridDbModel.Where(id => id.Id == mailId).FirstOrDefault();

        public async Task RecordReceivedEmail(SendGridDbModel emailDbModel)
        {

            _db.Entry(emailDbModel).State = EntityState.Added;
            _db.Add(emailDbModel);
            await _db.SaveChangesAsync();
        }


        //SentMail methods
        public async Task RecordSentMail(EmailDbModel SentEmail)
        {
            _db.Entry(SentEmail).State = EntityState.Added;
            _db.Add(SentEmail);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<EmailDbModel>> GetSentEmails() => await _db.EmailModels.OrderByDescending(x => x).ToListAsync();

        public async Task DeleteSentMailById(EmailDbModel mailEntity)
        {
            _db.EmailModels.Remove(mailEntity);
            await _db.SaveChangesAsync();
        }
        public EmailDbModel GetSentMailById(int? mailId) => _db.EmailModels.Where(id => id.Id == mailId).FirstOrDefault();


        //Subscription Push metod

        public async Task<List<PushSubscribeDbModel>> GetSubscribtions() => await _db.PushSubscribe.ToListAsync();



        //Admin
        public async Task<IEnumerable<FolderModel>> GetFolders() => await _db.Folders.OrderByDescending(x => x).ToListAsync();

        public async Task<FolderModel> GetFolderInfo(int? folderID) => await _db.Folders.Where(folder => folder.Id == folderID).FirstOrDefaultAsync();

        public async Task DeleteFolder(int? folderID)
        {
          var folderRecord = await  _db.Folders.Where(folder => folder.Id == folderID).FirstOrDefaultAsync();
           _db.Folders.Remove(folderRecord);
           await _db.SaveChangesAsync();
        }
       


        public async Task RecordFolderInfo(FolderModel folderModel)
        {
            _db.Entry(folderModel).State = EntityState.Added;
            _db.Add(folderModel);
            await _db.SaveChangesAsync();

            

        }

      
    }
}