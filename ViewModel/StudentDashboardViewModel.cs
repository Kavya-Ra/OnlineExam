using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class StudentDashboardViewModel
    {
        public List<GetExamByUserId_Result> GetExamByUserId { get; set; }
        public int StudentId { get; set; }
        public int AttendExamCount { get; set; }
        public int CourseCount { get; set; }
    }
}