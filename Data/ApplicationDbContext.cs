using Microsoft.EntityFrameworkCore;
using PhotoGalleryAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
            
        }
        
        public DbSet<SendGridDbModel>  SendGridDbModel  { get; set; }
        public DbSet<EmailDbModel> EmailModels { get; set; }
        public DbSet<PushSubscribeDbModel> PushSubscribe { get; set; }

        public DbSet<FolderModel> Folders { get; set; }

    }
}
