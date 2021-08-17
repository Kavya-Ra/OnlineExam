using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class ExamCreateViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }
        [Required]
        [Display(Name = "Group")]
        public int ExGroupId { get; set; }
        [Required]
        [Display(Name = "Programme")]
        public int PgmId { get; set; }
        [Required]
        [Display(Name = "Class")]
        public int ClassId { get; set; }
        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }
        [Required]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }
        [Required]
        [Display(Name = "To Date")]
        public DateTime ToDate { get; set; }
        [Required]
        [Display(Name = "From Date")]
        public DateTime FromDate { get; set; }
        [Required]
        [Display(Name = "Exam Time")]
        public string ExamTime { get; set; }
        [Required]
        [Display(Name = "Total Mark")]
        public int TotalMark { get; set; }

        [Display(Name = "Question")]
        public string QnIds { get; set; }
        [Required]
        public int CuserId { get; set; }
        [Required]
        public int IsDataEntryQn { get; set; }
        [Required]
        public int QsAsFrom { get; set; }
        public virtual ICollection<Exam_QnTable> ExamQns { get; set; }
    }
}