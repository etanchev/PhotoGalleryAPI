using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoGalleryAPI.Models
{
    public class FolderModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        
        public string FolderName { get; set; }

        public Guid Guid { get; set; } 
        public DateTime DateCreated { get; set; } 

        public string FirstImage { get; set; }
    }
   
}
