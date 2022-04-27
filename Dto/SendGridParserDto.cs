using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Dto
{
    public class SendGridParserDto
    {

        public int Id { get; set; }

        public DateTime DateTime { get; set; }

        public string To { get; set; }
        public string From { get; set; }

        public string  Subject { get; set; }
        public string  Text { get; set; }

        public string  Html { get; set; }

        public bool IsRed { get; set; }
    }
}
