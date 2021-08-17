using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class TeacherRegViewModel
    {
        public int? Id { get; set; }

        [Required]
        public string TeachRegId { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
        [Required]
        [Display(Name = "MiddleName")]
        public string MiddleName { get; set; }
        [Required]
        [Display(Name = "LastName")]
        public string LastName { get; set; }
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
        [Required]
        [Display(Name = "WhatsApp")]
        public string WhatsApp { get; set; }
        [Required]
        [Display(Name = "PrimarySubject")]
        public string PrimarySubject { get; set; }
        [Required]
        [Display(Name = "SecondarySubject")]
        public string SecondarySubject { get; set; }
        [Required]
        [Display(Name = "Location")]
        public string Location { get; set; }
        [Required]
        [Display(Name = "Street")]
        public string Street { get; set; }
        [Required]
        [Display(Name = "Address")]
        public string Address { get; set; }
        [Required]
        [Display(Name = "PO")]
        public string PO { get; set; }
        [Required]
        [Display(Name = "District")]
        public string District { get; set; }
        [Required]
        [Display(Name = "State")]
        public string State { get; set; }
        [Required]
        [Display(Name = "Class")]
        public string[] StudentGrade { get; set; }
        [Required]
        [Display(Name = "Weekdays")]
        public string Weekdays { get; set; }
        [Required]
        [Display(Name = "Weekends")]
        public string Weekends { get; set; }
        [Required]
        [Display(Name = "Timing")]
        public string[] Time { get; set; }
        [Required]
        [Display(Name = "Country")]
        public string Country { get; set; }

        public ICollection<Subject> Subjects { get; set; }
    }
}