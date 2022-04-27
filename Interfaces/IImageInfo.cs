using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Interfaces
{
    public interface IImageInfo
    {

        public string FileName { get; set; }
        public string Base64Url { get; set; }


    }
}
