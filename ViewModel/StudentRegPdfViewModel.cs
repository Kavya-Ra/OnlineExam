using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class StudentRegPdfViewModel
    {
        public GetAllStudentRegistrationByRegId_Result GetAllStudentRegistrationByRegId { get; set; }

        public List<Student_AcademicPerformancebyRegid_Result> Student_AcademicPerformancebyRegid { get; set; }

        public List<Student_PreviousEntrancebyRegid_Result> Student_PreviousEntrancebyRegid { get; set; }
    }
}