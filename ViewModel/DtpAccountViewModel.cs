using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class DtpAccountViewModel
    {
        
        [Required]
        [Display(Name = "Firs tName")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; }

        [Required]
        [Display(Name = "WhatsApp Number")]
        public string WhatsApp { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Location")]
        public string Location { get; set; }

        [Required]
        [Display(Name = "Place")]
        public string Place { get; set; }

        public int? Id { get; set; }

        public int CuserId { get; set; }

        [Required]
        [Display(Name = "Rgistration Id")]
        public string DtpRegId { get; set; }
    }
}