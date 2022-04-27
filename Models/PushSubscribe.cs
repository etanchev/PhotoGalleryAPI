using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Models
{
    public class PushSubscribe
    {
        public string Endpoint { get; set; }
        public string ExpirationTime { get; set; }
        public Keys Keys { get; set; }
        
    }
    public class Keys
    {
        public string P256dh { get; set; }
        public string Auth { get; set; }
    }
}
