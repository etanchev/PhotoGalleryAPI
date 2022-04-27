using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Models
{
    public class ImageInfoServer
    {
        public string Base64Url { get; set; }

        public string FileName { get; set; }

        public bool IsLocalFile { get; set; }
    }
}
