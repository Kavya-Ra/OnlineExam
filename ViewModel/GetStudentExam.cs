using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class GetStudentExam
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public System.DateTime FromDate { get; set; }
        public System.DateTime ToDate { get; set; }
        public string ExamTime { get; set; }
        public int TotalMark { get; set; }
        public string PName { get; set; }
        public string ClassName { get; set; }
        public string SubjectName { get; set; }
        public string CourseName { get; set; }

        public int Attended { get; set; }
        public int StudentId { get; set; }
    }
}