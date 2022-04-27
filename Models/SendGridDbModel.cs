using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PhotoGalleryAPI.Models
{
    public class SendGridDbModel 
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        // <summary>
        // The Domain Keys Identified Email code for the email
        // </summary>
        /// 

        public DateTime DateTime  { get; set; }


        public string To { get; set; }

        /// <summary>
        /// The HTML body of the email
        /// </summary>
        public string Html { get; set; }

        /// <summary>
        /// The email address the email was sent from
        /// </summary>
        public string From { get; set; }

        /// <summary>
        /// The Text body of the email
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// A JSON string containing the SMTP envelope. This will have two variables: to, which is an array of recipients, and from, which is the return path for the message.
        /// </summary>
        public string Envelope { get; set; }

        /// <summary>
        /// Number of attachments included in email
        /// </summary>
        public int Attachments { get; set; }

        /// <summary>
        /// The subject of the email
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// A JSON string containing the character sets of the fields extracted from the message.
        /// </summary>
        public string Charsets { get; set; }

        //check if  mail has been red
        public bool IsRed { get; set; }


    }
}
