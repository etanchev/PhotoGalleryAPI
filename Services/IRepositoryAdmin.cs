using PhotoGalleryAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Services
{
    public interface IRepositoryAdmin
    {
        public Task<IEnumerable<FolderModel>> GetFolders();
        public Task<FolderModel> GetFolderInfo(int? folderID);

        public Task RecordFolderInfo(FolderModel folderModel);
        public Task DeleteFolder(int? folderID);
    }
}
