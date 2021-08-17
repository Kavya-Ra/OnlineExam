using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class AttendExamViewModel
    {
        public int ExamId { get; set; }
        public string ExamName { get; set; }
        public string ClassName { get; set; }
        public string SubjectName { get; set; }
        public string CourseName { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; }
        public int StudentPhoto { get; set; }
    }
}