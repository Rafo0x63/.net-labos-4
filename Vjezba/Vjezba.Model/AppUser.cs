using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vjezba.Model
{
    public class AppUser : IdentityUser
    {
        
        [Required(ErrorMessage = "OIB is required")]
        [StringLength(13, MinimumLength = 11, ErrorMessage = "OIB must be between 11 and 13 characters")]
        public string OIB {  get; set; }

        [Required(ErrorMessage = "JMBG is required")]
        [StringLength(13, MinimumLength = 11, ErrorMessage = "JMBG must be between 11 and 13 characters")]
        public string JMBG { get; set; }
    }
}
