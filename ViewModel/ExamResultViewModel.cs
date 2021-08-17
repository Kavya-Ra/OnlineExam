using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class ExamResultViewModel
    {
        public List<GetAllAttentedExamByStudentId_Result> AllExam { get; set; }
        public GetAllExamById_Result ExDetails { get; set; }
        public AttendExam AttendDetails { get; set; }
        public List<GetExamReultByQus_Result> ExResult { get; set; }
        public int? StudentId { get; set; }
    }
}