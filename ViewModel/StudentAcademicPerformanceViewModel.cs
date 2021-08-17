using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class StudentAcademicPerformanceViewModel
    {
        [Required]
        [Display(Name = "Class")]
        public string Class { get; set; }
        [Required]
        [Display(Name = "Passed Year")]
        public string PassYear { get; set; }
        [Required]
        [Display(Name = "School")]
        public string SchoolAddress { get; set; }
        [Required]
        [Display(Name = "Registration No")]
        public string RegNo { get; set; }
        [Required]
        [Display(Name = "Board")]
        public string Board { get; set; }
        [Required]
        [Display(Name = "Physics Mark")]
        public string PhyMark { get; set; }
        [Required]
        [Display(Name = "Chemistry Mark")]
        public string ChemMark { get; set; }
        [Required]
        [Display(Name = "Biology Mark")]
        public string BiologyMark { get; set; }
        [Required]
        [Display(Name = "Maths Mark")]
        public string MathsMark { get; set; }
        [Required]
        [Display(Name = "Percentage Of Mark")]
        public string PercOfMark { get; set; }
    }
}