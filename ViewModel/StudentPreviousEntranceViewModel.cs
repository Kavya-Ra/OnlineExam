using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class StudentPreviousEntranceViewModel
    {
        [Required]
        [Display(Name = "Previous Entrance Exam")]
        public string PrevEntranceExamName { get; set; }
        [Required]
        [Display(Name = "RollNo")]
        public string RollNo { get; set; }
        [Required]
        [Display(Name = "Attempted Year")]
        public string AttemptedYear { get; set; }
        [Required]
        [Display(Name = "Mark")]
        public string Mark { get; set; }
        [Required]
        [Display(Name = "Rank")]
        public string Rank { get; set; }
        [Required]
        [Display(Name = "No Of Attempts")]
        public string NoOfAttempts { get; set; }
    }
}