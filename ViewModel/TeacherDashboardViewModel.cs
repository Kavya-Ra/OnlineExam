using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class TeacherDashboardViewModel
    {
        public List<GetExamByTeacherId_Result> GetExam { get; set; }
        public List<GetAllExamByTeacherId_Result> AllExam { get; set; }
        public int TeacherId { get; set; }
        public int ContactedExamCount { get; set; }
        public int CourseCount { get; set; }
        public int StudentCount { get; set; }
    }
}