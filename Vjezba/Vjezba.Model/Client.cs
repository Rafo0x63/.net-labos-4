using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Vjezba.Model;

public class Client
{
	[Key]
	public int ID { get; set; }
	[Required(ErrorMessage = "Ime je obavezno")]
	[StringLength(30, MinimumLength = 1, ErrorMessage = "Ime mora sadrzavati izmedu 1 i 30 slova")]
	public string FirstName { get; set; }
	[Required(ErrorMessage = "Prezime je obavezno")]
	[StringLength(30, MinimumLength = 1, ErrorMessage = "Prezime mora sadrzavati izmedu 1 i 30 slova")]
	public string LastName { get; set; }
	public string Email { get; set; }
	public char Gender { get; set; }
	public string Address { get; set; }
	public string PhoneNumber { get; set; }

	[ForeignKey(nameof(City))]
	public int? CityID { get; set; }
	public City? City { get; set; }

	public string FullName => $"{FirstName} {LastName}";

	public virtual ICollection<Meeting>? Meetings { get; set; }

	[Range(0, 100, ErrorMessage = "Godine radnog staza moraju biti izmedu 0 i 100")]
	public int? WorkingExperience { get; set; }

	public DateTime? DateOfBirth { get; set; }	
}