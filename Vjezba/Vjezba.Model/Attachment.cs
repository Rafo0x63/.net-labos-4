using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
namespace Vjezba.Model
{
    public class Attachment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string FilePath { get; set; }

        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public Client Client { get; set; }

    }
}
