using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class AdminDashboardViewModel
    {
        public List<GetExamByDate_Result> GetExam { get; set; }
        public int TotalExam { get; set; }
        public int TotalBatch { get; set; }
        public int TotalStudents { get; set; }
    }
}