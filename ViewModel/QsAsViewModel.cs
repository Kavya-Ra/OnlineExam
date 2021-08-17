using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class QsAsViewModel
    {
        public int? Id { get; set; }
        public string Questions { get; set; }
        [Required]
        [Display(Name = "Option1")]
        public string Option1 { get; set; }
        [Required]
        [Display(Name = "Option2")]
        public string Option2 { get; set; }
        [Required]
        [Display(Name = "Option3")]
        public string Option3 { get; set; }
        [Required]
        [Display(Name = "Option4")]
        public string Option4 { get; set; }
        [Required]
        [Display(Name = "Correct Answer")]
        public string CorrectAns { get; set; }
        [Required]
        [Display(Name = "Mark")]
        public string Mark { get; set; }
        [Required]
        [Display(Name = "Year")]
        public string PrevQnYear { get; set; }
        [Required]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }
        [Required]
        [Display(Name = "Chapter")]
        public int ChapterId { get; set; }
        [Required]
        [Display(Name = "Programme")]
        public int PgmId { get; set; }
        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }
        [Required]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        public string Photo { get; set; }
        public int CuserId { get; set; }

        public IEnumerable<Programme> Programmes { get; set; }
        public List<SubProgram> SubPrograms { get; set; }
        public ICollection<Subject> Subjects { get; set; }
        public ICollection<Chapter> Chapters { get; set; }
        public ICollection<Cours> Course { get; set; }
    }
}