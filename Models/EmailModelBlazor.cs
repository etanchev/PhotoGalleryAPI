using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Models
{
    public class EmailModelBlazor
    {
        public string Name { get; set; }
        public string Tel { get; set; }
        public string Email { get; set; }
        public List<string> Body { get; set; }
    }
}
