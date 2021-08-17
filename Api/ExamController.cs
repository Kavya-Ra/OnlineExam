using OnlineExam.DbContext;
using OnlineExam.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace OnlineExam.Api
{
    public class ExamController : ApiController
    {
        private readonly Exam_DBEntities db = new Exam_DBEntities();

        [Route("api/Exam/QsAs/{id:int}/{fromId:int}")]
        [ResponseType(typeof(GetAllQusByExamId_Result))]
        public IHttpActionResult GetExamQsAs(int id, int fromId)
        {
            List<GetAllQusByExamId_Result> GetAllQusByExamId = db.GetAllQusByExamId(id, fromId).ToList();
            return Ok(GetAllQusByExamId);
        }

        [Route("api/Exam/DeleteExam/{id:int}")]
        [ResponseType(typeof(Exam))]
        public IHttpActionResult DeletePerformance(int id)
        {
            Exam exam = db.Exams.Find(id);
            if (exam == null)
            {
                return NotFound();
            }

            List<Exam_QnTable> exam_Qn = db.Exam_QnTable.Where(e => e.ExamId == exam.Id).ToList();
            foreach (var item in exam_Qn)
            {
                db.Exam_QnTable.Remove(item);
            }
            db.Exams.Remove(exam);
            db.SaveChanges();

            return Ok(exam);
        }

        [Route("api/Exam/DeleteExamQsAs/{id}/{examId:int}")]
        [ResponseType(typeof(Exam_QnTable))]
        public IHttpActionResult DeletePerformance(string id, int examId)
        {
            Exam_QnTable exam_Qn = db.Exam_QnTable.Where(e => e.QnId == id && e.ExamId == examId).FirstOrDefault();
            if (exam_Qn == null)
            {
                return NotFound();
            }

            db.Exam_QnTable.Remove(exam_Qn);
            db.SaveChanges();

            return Ok(exam_Qn);
        }

        [Route("api/Exam/GetQsasByDtp/{id:int}")]
        [ResponseType(typeof(Exam_QnTable))]
        public IHttpActionResult GetQsasByDtp(int id)
        {
            List<GetAllDtpQusAnsByUserId_Result> qsas = db.GetAllDtpQusAnsByUserId(id).Where(q=>q.IsDeleted == 0).ToList();
            return Ok(qsas);
        }

        [HttpPost]
        [Route("api/Exam/GetQsAsBank")]
        [ResponseType(typeof(Student_AcademicPerformance))]
        public IHttpActionResult GetQsAsFromQnBank(Exam exam)
        {
            List<DataEntry_QuestionBank> result = db.DataEntry_QuestionBank
                .Where(d => d.IsDeleted == 0 && d.PgmId == exam.PgmId && d.CourseId == exam.CourseId && 
                d.SubjectId == exam.SubjectId && d.IsDeleted == 0 && d.IsActive == 1).ToList();
            return Ok(result);
        }

        [HttpPost]
        [Route("api/Exam/GetQsAsManual")]
        [ResponseType(typeof(Student_AcademicPerformance))]
        public IHttpActionResult GetQsAsFromManual(Exam exam)
        {
            List<Teachers_QuestionBank> result = db.Teachers_QuestionBank
                .Where(d => d.IsDeleted == 0 && d.PgmId == exam.PgmId && d.CourseId == exam.CourseId && 
                d.SubjectId == exam.SubjectId && d.CreatedBy == exam.CreatedBy && d.IsDeleted == 0 && d.IsActive == 1).ToList();
            return Ok(result);
        }

        [HttpPost]
        [Route("api/Exam/GetExamEditQsAs")]
        [ResponseType(typeof(Student_AcademicPerformance))]
        public IHttpActionResult GetExamEditQsAs(Exam exam)
        {
            List<GetAllQusForEdit_Result> editQsas = db.GetAllQusForEdit(exam.Id, exam.QsAsFrom, exam.CourseId, exam.PgmId, exam.SubjectId).Where(e=>e.IsDeleted == 0 && e.IsActive == 1).ToList();
            return Ok(editQsas);
        }

        [Route("api/Exam/Result")]
        [ResponseType(typeof(ExamResult))]
        public IHttpActionResult Result(ExamViewModel examView)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            foreach (var item in examView.ExamResults)
            {
                ExamResult exam = new ExamResult()
                {
                    ExamId = item.ExamId,
                    StudentId = item.StudentId,
                    QuestionId = item.QuestionId,
                    SelectedAnswer = item.SelectedAnswer,
                    CorrectAnswer = item.CorrectAnswer,
                    NotVisited = item.NotVisited,
                    MarkForReview = item.MarkForReview,
                    AnsMarkForReview = item.AnsMarkForReview,
                    QsNo = item.QsNo
                };
                db.ExamResults.Add(exam);
            }

            db.SaveChanges();
            return Ok(examView);
        }

        [Route("api/Exam/ResultAttend")]
        [ResponseType(typeof(ExamResult))]
        public IHttpActionResult ResultAttend(ExamViewModel attend)
        {
            int eid = attend.Attends[0].ExamId;
            int sid = attend.Attends[0].StudentId;
            AttendExam attendExam = db.AttendExams.Where(a => a.ExamId == eid && a.StudentId == sid).FirstOrDefault();
            if (attendExam != null)
            {
                attendExam.TQ = attend.Attends[0].TQ;
                attendExam.TA = attend.Attends[0].TA;
                attendExam.CA = attend.Attends[0].CA;
                attendExam.IA = attend.Attends[0].IA;
                attendExam.ES = attend.Attends[0].ES;
                db.Entry(attendExam).State = EntityState.Modified;
                db.SaveChanges();
                return Ok(attendExam);
            }

            return BadRequest(ModelState);
        }

    }
}
