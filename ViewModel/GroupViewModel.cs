using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class GroupViewModel
    {
        public int? Id { get; set; }

        [Required]
        [Display(Name = "GroupName")]
        public string GroupName { get; set; }
        [Required]
        [Display(Name = "Programme")]
        public int PgmId { get; set; }
        [Required]
        [Display(Name = "Course")]
        public int CourseId { get; set; }
        [Required]
        [Display(Name = "Sub Program")]
        public int SubPgmId { get; set; }
        [Required]
        [Display(Name = "Subject")]
        public int SubjectId { get; set; }
        [Required]
        [Display(Name = "Class")]
        public int ClassId { get; set; }

        [Required]
        [Display(Name = "Teacher")]
        public int[] TeacherId { get; set; }

        [Required]
        [Display(Name = "Student")]
        public int[] StudentId { get; set; }

        public ICollection<Programme> Programmes { get; set; }
        public ICollection<SubProgram> SubPrograms { get; set; }
        public ICollection<Subject> Subjects { get; set; }
        public ICollection<Class> Classes { get; set; }
        public ICollection<Cours> Course { get; set; }
        public IEnumerable<User> Users { get; set; }
    }
}