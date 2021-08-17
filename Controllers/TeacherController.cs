using OnlineExam.Authentication;
using OnlineExam.DbContext;
using OnlineExam.ViewModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OnlineExam.Controllers
{
    [CustomAuthorize(Roles = "Teacher")]
    public class TeacherController : Controller
    {
        private readonly Exam_DBEntities db = new Exam_DBEntities();

        // GET: Teacher
        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public new ActionResult Profile()
        {
            return View();
        }

        public ActionResult Dashboard()
        {
            int id = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().Id;
            DateTime today = DateTime.Now.Date;

            TeacherDashboardViewModel teacherDashboard = new TeacherDashboardViewModel()
            {
                GetExam = db.GetExamByTeacherId(id, today).Where(e=>e.IsActive == 1).ToList(),
                TeacherId = id,
                ContactedExamCount = db.GetAllExamByTeacherId(id).Count(),
                CourseCount = db.GetCourseDetailsByUserId(id, 0).Count(),
                StudentCount = db.GetGroupUserByTeacherId(id).Count()
            };

            return View(teacherDashboard);
        }

        public ActionResult QaAsList()
        {
            if (TempData["StatusMessage"] != null)
            {
                ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            int id = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().Id;
            var list = db.GetAllTeacherQusAnsByUserId(id).ToList();
            return View(list);
        }

        public async Task<ActionResult> QsAs(int? id)
        {
            if (id == null)
            {
                ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name");
                ViewBag.ClassId = new SelectList(db.Classes.Where(s => s.IsDeleted == 0), "Id", "Name");
                ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name");
                ViewBag.CourseId = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.ChapterId = new SelectList(Enumerable.Empty<SelectListItem>());

                QsAsViewModel dtpQA = new QsAsViewModel()
                {
                    Questions = ""
                };

                return View(dtpQA);

            }
            else
            {
                var data = await db.Teachers_QuestionBank.Where(d => d.Id == id).FirstOrDefaultAsync();
                QsAsViewModel dtpQA = new QsAsViewModel()
                {
                    Id = data.Id,
                    Questions = data.Questions,
                    Option1 = data.Option1,
                    Option2 = data.Option2,
                    Option3 = data.Option3,
                    Option4 = data.Option4,
                    PrevQnYear = data.PrevQnYear,
                    CorrectAns = data.CorrectAns,
                    Mark = data.Mark,
                    PgmId = data.PgmId,
                    ClassId = data.ClassId,
                    CourseId = data.CourseId,
                    SubjectId = data.SubjectId,
                    ChapterId = data.ChapterId,
                    Photo = data.Photo
                };

                ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", data.PgmId);
                ViewBag.ClassId = new SelectList(db.Classes.Where(s => s.IsDeleted == 0), "Id", "Name", data.ClassId);
                ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == data.ClassId), "Id", "Name", data.CourseId);
                ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", data.SubjectId);
                ViewBag.ChapterId = new SelectList(db.Chapters.Where(p => p.IsDeleted == 0 && p.SubId == data.SubjectId), "Id", "Name", data.ChapterId);

                return View(dtpQA);
            }
        }

        [HttpGet]
        public JsonResult Course(int ID)
        {
            var sub = new SelectList(db.Courses.Where(s => s.ClassId == ID && s.IsDeleted == 0), "Id", "Name");
            return Json(sub, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult Chapters(int ID)
        {
            var chap = new SelectList(db.Chapters.Where(c => c.SubId == ID && c.IsDeleted == 0), "Id", "Name");
            return Json(chap, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> QsAs(QsAsViewModel dtpQAView, HttpPostedFileBase fileQus)
        {
            if (dtpQAView.Questions != null || fileQus != null || dtpQAView.Photo != null)
            {
                var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", "jpeg" };
                var uploadPath = "";
                var oldPath = "";
                string alpha = "Question_";
                Random random = new Random();
                int unique = random.Next(10000, 99999);
                int y = DateTime.Now.Year;
                int m = DateTime.Now.Month;
                var upFileName = alpha + y + m + unique;

                if (!ModelState.IsValid)
                {
                    ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", dtpQAView.PgmId);
                    ViewBag.ClassId = new SelectList(db.Classes.Where(p => p.IsDeleted == 0), "Id", "Name", dtpQAView.ClassId);
                    ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == dtpQAView.ClassId), "Id", "Name", dtpQAView.CourseId);
                    ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", dtpQAView.SubjectId);
                    ViewBag.ChapterId = new SelectList(db.Chapters.Where(p => p.IsDeleted == 0 && p.SubId == dtpQAView.SubjectId), "Id", "Name", dtpQAView.ChapterId);

                    ViewBag.ErrorMessage = "Please fill in all the required fields";
                    return View(dtpQAView);
                }

                if (dtpQAView.Id != null)
                {
                    if (fileQus != null)
                    {
                        oldPath = dtpQAView.Photo;
                        var FileExt = Path.GetExtension(fileQus.FileName);
                        if (allowedExtensions.Contains(FileExt))
                        {
                            string dtpRegId = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().UniqueID;
                            string myfile = dtpRegId + "_" + upFileName + FileExt;
                            uploadPath = Path.Combine(Server.MapPath("~/Uploads/QuestionTeacher/"), myfile);
                            dtpQAView.Photo = "../../Uploads/QuestionTeacher/" + myfile;
                        }
                    }

                    Teachers_QuestionBank data = db.Teachers_QuestionBank.Find(dtpQAView.Id);
                    if (data != null)
                    {
                        data.Questions = dtpQAView.Questions;
                        data.Option1 = dtpQAView.Option1;
                        data.Option2 = dtpQAView.Option2;
                        data.Option3 = dtpQAView.Option3;
                        data.Option4 = dtpQAView.Option4;
                        data.PrevQnYear = dtpQAView.PrevQnYear;
                        data.CorrectAns = dtpQAView.CorrectAns;
                        data.Mark = dtpQAView.Mark;
                        data.ModifiedDateTime = DateTime.Now;
                        data.ModifiedBy = dtpQAView.CuserId;
                        data.PgmId = dtpQAView.PgmId;
                        data.ClassId = dtpQAView.ClassId;
                        data.CourseId = dtpQAView.CourseId;
                        data.SubjectId = dtpQAView.SubjectId;
                        data.ChapterId = dtpQAView.ChapterId;
                        data.Photo = dtpQAView.Photo;
                        db.Entry(data).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        if (fileQus != null)
                        {
                            fileQus.SaveAs(uploadPath);

                            string path = Server.MapPath(oldPath);
                            FileInfo file = new FileInfo(path);
                            if (file.Exists)//check file exsit or not
                            {
                                file.Delete();
                            }
                        }

                        TempData["StatusMessage"] = "Questions Answers Edited Succesfully.";
                        return RedirectToAction("QaAsList");
                    }

                    ViewBag.ErrorMessage = "Please fill in all the required fields";
                    return View(dtpQAView);
                }
                else
                {
                    if (fileQus != null)
                    {
                        var FileExt = Path.GetExtension(fileQus.FileName);
                        if (allowedExtensions.Contains(FileExt))
                        {
                            string dtpRegId = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().UniqueID;
                            string myfile = dtpRegId + "_" + upFileName + FileExt;
                            uploadPath = Path.Combine(Server.MapPath("~/Uploads/QuestionTeacher"), myfile);
                            dtpQAView.Photo = "../../Uploads/QuestionTeacher/" + myfile;
                        }
                    }

                    Teachers_QuestionBank data = new Teachers_QuestionBank()
                    {
                        Questions = dtpQAView.Questions,
                        Option1 = dtpQAView.Option1,
                        Option2 = dtpQAView.Option2,
                        Option3 = dtpQAView.Option3,
                        Option4 = dtpQAView.Option4,
                        PrevQnYear = dtpQAView.PrevQnYear,
                        CorrectAns = dtpQAView.CorrectAns,
                        Mark = dtpQAView.Mark,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        DeletedDateTime = DateTime.Now,
                        CreatedBy = dtpQAView.CuserId,
                        PgmId = dtpQAView.PgmId,
                        ClassId = dtpQAView.ClassId,
                        CourseId = dtpQAView.CourseId,
                        SubjectId = dtpQAView.SubjectId,
                        ChapterId = dtpQAView.ChapterId,
                        Photo = dtpQAView.Photo
                    };

                    db.Teachers_QuestionBank.Add(data);
                    await db.SaveChangesAsync();

                    if (fileQus != null)
                    {
                        fileQus.SaveAs(uploadPath);
                    }

                    TempData["StatusMessage"] = "Questions Answers Created Succesfully.";
                    return RedirectToAction("QaAsList");
                }
            }
            else
            {
                ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", dtpQAView.PgmId);
                ViewBag.ClassId = new SelectList(db.Classes.Where(p => p.IsDeleted == 0), "Id", "Name", dtpQAView.ClassId);
                ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == dtpQAView.ClassId), "Id", "Name", dtpQAView.CourseId);
                ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", dtpQAView.SubjectId);
                ViewBag.ChapterId = new SelectList(db.Chapters.Where(p => p.IsDeleted == 0 && p.SubId == dtpQAView.SubjectId), "Id", "Name", dtpQAView.ChapterId);

                ViewBag.ErrorMessage = "Please enter Questions or Select a Image Questions";
                return View(dtpQAView);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteQsAS(int? deleteQsAsId)
        {
            if (deleteQsAsId != null)
            {
                Teachers_QuestionBank dataEntry = db.Teachers_QuestionBank.Find(deleteQsAsId);
                dataEntry.IsDeleted = 1;
                dataEntry.DeletedDateTime = DateTime.Now;
                db.Entry(dataEntry).State = EntityState.Modified;
                db.SaveChanges();

                TempData["StatusMessage"] = "Questions Answers Deleted Succesfully.";
                return RedirectToAction("QaAsList");
            }

            TempData["ErrorMessage"] = "Questions Answers Not Deleted.";
            return RedirectToAction("QaAsList");
        }

        public ActionResult Exams()
        {
            if (TempData["StatusMessage"] != null)
            {
                ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            int id = db.Users.Where(u => u.UserName == User.Identity.Name).FirstOrDefault().Id;
            List<GetAllExamByTeacherId_Result> list = db.GetAllExamByTeacherId(id).ToList();
            return View(list);
        }

        public ActionResult Exam(int? Id)
        {
            if(Id != null)
            {
                var data = db.Exams.Include(d => d.Exam_QnTable).Where(e=>e.Id == Id).FirstOrDefault();

                ExamCreateViewModel viewModel = new ExamCreateViewModel()
                {
                    Name = data.Name,
                    FromDate = data.FromDate,
                    ToDate = data.ToDate,
                    ExamTime = data.ExamTime,
                    TotalMark = data.TotalMark,
                    ExamQns = data.Exam_QnTable,
                    QsAsFrom = data.QsAsFrom
                };

                ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", data.PgmId);
                ViewBag.ClassId = new SelectList(db.Classes.Where(p => p.IsDeleted == 0), "Id", "Name", data.ClassId);
                ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", data.SubjectId);
                ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == data.ClassId), "Id", "Name", data.CourseId);
                ViewBag.ExGroupId = new SelectList(db.Groups.Where(p => p.IsDeleted == 0 && p.SubjectId == data.SubjectId), "Id", "GroupName", data.ExGroupId);


                return View(viewModel);
            }
           
            ViewBag.ClassId = new SelectList(db.Classes.Where(d => d.IsDeleted == 0), "Id", "Name");
            ViewBag.PgmId = new SelectList(db.Programmes.Where(d => d.IsDeleted == 0), "Id", "Name");
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(d => d.IsDeleted == 0), "Id", "Name");
            ViewBag.CourseId = new SelectList(Enumerable.Empty<SelectListItem>());
            ViewBag.ExGroupId = new SelectList(Enumerable.Empty<SelectListItem>());

            return View();
        }

        //  GetCourseWiseClass
        public ActionResult GetCourseWiseClass(int id)
        {
            var result = new SelectList(db.Courses.Where(r => r.ClassId == id && r.IsDeleted == 0), "Id", "Name");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        //  GetCourseWiseClass
        public ActionResult GetGroup(int id)
        {
            var result = new SelectList(db.Groups.Where(r => r.SubjectId == id && r.IsDeleted == 0), "Id", "GroupName");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Exam(ExamCreateViewModel examCreateView)
        {
            ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", examCreateView.PgmId);
            ViewBag.ClassId = new SelectList(db.Classes.Where(p => p.IsDeleted == 0), "Id", "Name", examCreateView.ClassId);
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", examCreateView.SubjectId);
            ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == examCreateView.ClassId), "Id", "Name", examCreateView.CourseId);
            ViewBag.ExGroupId = new SelectList(db.Groups.Where(p => p.IsDeleted == 0 && p.SubjectId == examCreateView.SubjectId), "Id", "GroupName", examCreateView.ExGroupId);

            var qnIds = examCreateView.QnIds;            

            if (ModelState.IsValid)
            {
                if(examCreateView.Id != null)
                {
                    Exam getExam = db.Exams.Find(examCreateView.Id);
                    if(getExam != null)
                    {
                        getExam.Name = examCreateView.Name;
                        getExam.PgmId = examCreateView.PgmId;
                        getExam.ClassId = examCreateView.ClassId;
                        getExam.CourseId = examCreateView.CourseId;
                        getExam.SubjectId = examCreateView.SubjectId;
                        getExam.FromDate = examCreateView.FromDate;
                        getExam.ToDate = examCreateView.ToDate;
                        getExam.ExamTime = examCreateView.ExamTime;
                        getExam.TotalMark = examCreateView.TotalMark;
                        getExam.CreatedBy = examCreateView.CuserId;
                        getExam.ExGroupId = examCreateView.ExGroupId;
                        getExam.QsAsFrom = examCreateView.QsAsFrom;
                        getExam.ModifiedBy = examCreateView.CuserId;
                        getExam.ModifiedDateTime = DateTime.Now;

                        db.Entry(getExam).State = EntityState.Modified;
                        db.SaveChanges();

                        if (qnIds != null)
                        {
                            List<string> result = qnIds.Split('|').ToList();

                            foreach (string qnId in result)
                            {
                                if (qnId != "")
                                {
                                    var ExamQnTable = new Exam_QnTable()
                                    {
                                        QnId = qnId,
                                        ExamId = (int)examCreateView.Id,
                                        IsDataEntryQn = examCreateView.QsAsFrom
                                    };
                                    db.Exam_QnTable.Add(ExamQnTable);
                                    db.SaveChanges();
                                }
                            }                            
                        }

                        TempData["StatusMessage"] = "Exam Edited Succesfully.";
                        return RedirectToAction("Exams");
                    }

                    ViewBag.ErrorMessage = "I can't find any exam with this ID";
                    return View(examCreateView);
                }
                else
                {
                    var Exam = new Exam()
                    {
                        Name = examCreateView.Name,
                        PgmId = examCreateView.PgmId,
                        ClassId = examCreateView.ClassId,
                        CourseId = examCreateView.CourseId,
                        SubjectId = examCreateView.SubjectId,
                        FromDate = examCreateView.FromDate,
                        ToDate = examCreateView.ToDate,
                        ExamTime = examCreateView.ExamTime,
                        TotalMark = examCreateView.TotalMark,
                        CreatedBy = examCreateView.CuserId,
                        ExGroupId = examCreateView.ExGroupId,
                        QsAsFrom = examCreateView.QsAsFrom,
                        IsDeleted = 0,
                        IsActive = 0,
                        DeletedBy = 0,
                        ModifiedBy = 0,
                        ModifiedDateTime = DateTime.Now,
                        DeletedDateTime = DateTime.Now,
                        CreatedDateTime = DateTime.Now
                    };

                    if (qnIds != null)
                    {
                        db.Exams.Add(Exam);
                        db.SaveChanges();
                        int examid = db.Exams.Max(item => item.Id);

                        List<string> result = qnIds.Split('|').ToList();

                        foreach (string qnId in result)
                        {
                            if (qnId != "")
                            {
                                var ExamQnTable = new Exam_QnTable()
                                {
                                    QnId = qnId,
                                    ExamId = examid,
                                    IsDataEntryQn = examCreateView.IsDataEntryQn
                                };
                                db.Exam_QnTable.Add(ExamQnTable);
                                db.SaveChanges();
                            }
                        }

                        TempData["StatusMessage"] = "Exam Created Succesfully.";
                        return RedirectToAction("Exams");
                    }
                    else
                    {
                        ViewBag.ErrorMessage = "Please select Questions";
                        return View(examCreateView);
                    }
                }    
            }

            ViewBag.ErrorMessage = "Please fill in all the required fields";
            return View(examCreateView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActiveExam(int? activeId)
        {
            if (activeId == null)
            {
                TempData["ErrorMessage"] = "Account Not Activated";
                return RedirectToAction("Exams");
            }
            else
            {
                Exam exam = await db.Exams.Where(c => c.Id == activeId).FirstOrDefaultAsync();
                if (exam != null)
                {
                    exam.IsActive = 1;
                    db.Entry(exam).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "Account Activated Succesfully.";
                    return RedirectToAction("Exams");
                }
                TempData["ErrorMessage"] = "Account Not Activated";
                return RedirectToAction("Exams");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> InactiveExam(int? inactiveId)
        {
            if (inactiveId == null)
            {
                TempData["ErrorMessage"] = "Account Not Inactivated";
                return RedirectToAction("Exams");
            }
            else
            {
                Exam programmes = await db.Exams.Where(c => c.Id == inactiveId).FirstOrDefaultAsync();
                if (programmes != null)
                {
                    programmes.IsActive = 0;
                    db.Entry(programmes).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "Account Inactivated Succesfully.";
                    return RedirectToAction("Exams");
                }
                TempData["ErrorMessage"] = "Account Not Inactivated";
                return RedirectToAction("Exams");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteExam(int deleteQsAsId)
        {
            Exam exam = db.Exams.Find(deleteQsAsId);
            if (exam == null)
            {
                TempData["ErrorMessage"] = "Exam Not Deleted";
                return RedirectToAction("Exams");
            }
            
            List<Exam_QnTable> exam_Qn = db.Exam_QnTable.Where(e => e.ExamId == exam.Id).ToList();
            foreach (var item in exam_Qn)
            {
                db.Exam_QnTable.Remove(item);
            }
            db.Exams.Remove(exam);
            db.SaveChanges();

            TempData["StatusMessage"] = "Exam Deleted Succesfully.";
            return RedirectToAction("Exams");
        }
    }
}