using OnlineExam.DbContext;
using OnlineExam.ViewModel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace OnlineExam.Controllers
{
    public class RegistrationController : Controller
    {

        private readonly Exam_DBEntities db = new Exam_DBEntities();

        // GET: Registration
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Teacher()
        {

            if (TempData["StatusMessage"] != null)
            {
                ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
                ViewBag.ApplicationName = TempData["ApplicationName"].ToString();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            string alpha = "ECT";
            Random random = new Random();
            int unique = random.Next(10000, 99999);
            int y = DateTime.Now.Year;
            int m = DateTime.Now.Month;
            var uniqueID = alpha + y + m + unique;

            TeacherRegViewModel model = new TeacherRegViewModel()
            {
                TeachRegId = uniqueID
            };

            return View(model);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Teacher(TeacherRegViewModel teacherRegView)
        {
            if (ModelState.IsValid)
            {
                

                Teachers_Registration teachers_Registration = new Teachers_Registration
                {
                    TeachRegId = teacherRegView.TeachRegId,
                    FirstName = teacherRegView.FirstName,
                    MiddleName = teacherRegView.MiddleName,
                    LastName = teacherRegView.LastName,
                    Email = teacherRegView.Email,
                    WhatsApp = teacherRegView.WhatsApp,
                    PrimarySubject = teacherRegView.PrimarySubject,
                    SecondarySubject = teacherRegView.SecondarySubject,
                    Location = teacherRegView.Location,
                    Street = teacherRegView.Street,
                    Address = teacherRegView.Address,
                    PO = teacherRegView.PO,
                    District = teacherRegView.District,
                    State = teacherRegView.State,
                    Country = teacherRegView.Country,
                    Time = String.Join(",", teacherRegView.Time),
                    Weekends = teacherRegView.Weekends,
                    Weekdays = teacherRegView.Weekdays,
                    StudentGrade = String.Join(",", teacherRegView.StudentGrade),
                    DeletedDateTime = DateTime.Now
                };

                db.Teachers_Registration.Add(teachers_Registration);
                await db.SaveChangesAsync();

                TempData["ApplicationName"] = teacherRegView.FirstName + " " + teacherRegView.MiddleName + " " + teacherRegView.LastName;
                TempData["StatusMessage"] = "Thank You for your registration we will reach you soon.";
                return RedirectToAction("TeacherRegisterSuccess");

            }

            ViewBag.ErrorMessage = "Please fill in all the required fields";
            return View(teacherRegView);

        }

        public ActionResult TeacherRegisterSuccess()
        {
            return RedirectToAction("Teacher");
        }

        public ActionResult Student()
        {
            if (TempData["StatusMessage"] != null)
            {
                ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
                ViewBag.ApplicationName = TempData["ApplicationName"].ToString();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            string alpha = "ECS";
            Random random = new Random();
            int unique = random.Next(10000, 99999);
            int y = DateTime.Now.Year;
            int m = DateTime.Now.Month;
            var uniqueID = alpha + y + m + unique;

            StudentRegistrationViewModel model = new StudentRegistrationViewModel()
            {
                RegId = uniqueID
            };

            ViewBag.ClassID = new SelectList(db.Classes.Where(r => r.IsDeleted == 0), "Id", "Name");
            ViewBag.ProgramID = new SelectList(db.Programmes.Where(r => r.IsDeleted == 0), "Id", "Name");
            ViewBag.SubProgramID = new SelectList(db.SubPrograms.Where(r => r.IsDeleted == 0), "Id", "Name");
            ViewBag.CourseID = new SelectList(db.Courses.Where(r => r.IsDeleted == 0), "Id", "Name");
            return View(model);

        }

        public ActionResult GetCourseWiseClass(int id)
        {
            var result = new SelectList(db.Courses.Where(r => r.ClassId == id && r.IsDeleted == 0), "Id", "Name");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetSubPgmWisePgm(int id)
        {
            var result = new SelectList(db.SubPrograms.Where(r => r.PgmId == id && r.IsDeleted == 0), "Id", "Name");
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Student(StudentRegistrationViewModel model, HttpPostedFileBase file_Photo, HttpPostedFileBase file_FrSign, HttpPostedFileBase file_MrSign)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ClassID = new SelectList(db.Classes.Where(r => r.IsDeleted == 0), "Id", "Name");
                ViewBag.ProgramID = new SelectList(db.Programmes.Where(r => r.IsDeleted == 0), "Id", "Name");
                ViewBag.SubProgramID = new SelectList(db.SubPrograms.Where(r => r.IsDeleted == 0), "Id", "Name");
                ViewBag.CourseID = new SelectList(db.Courses.Where(r => r.IsDeleted == 0), "Id", "Name");

                ViewBag.ErrorMessage = "Please fill in all the required fields.";

                return View(model);
            }



            ////IMAGE OF STUDENT
            var allowedExtensions = new[] { ".Jpg", ".png", ".jpg", "jpeg" };

            var ext = Path.GetExtension(file_Photo.FileName);
            if (allowedExtensions.Contains(ext))
            {
                var fileName = model.RegId + "_Student";
                var extension = System.IO.Path.GetExtension(file_Photo.FileName).ToLower();

                using (var img = System.Drawing.Image.FromStream(file_Photo.InputStream))
                {
                    var Photo = String.Format("~/Uploads/StudentRegistration/Image/{0}{1}", fileName, extension);
                    SaveToFolder(img, fileName, extension, new Size(200, 200), Photo);
                    model.Photo = "../../Uploads/StudentRegistration/Image/" + fileName + extension;
                }
            }

            var extfr = Path.GetExtension(file_FrSign.FileName);
            if (allowedExtensions.Contains(extfr))
            {

                var fileName = model.RegId + "_F_Sign";
                var extension = System.IO.Path.GetExtension(file_FrSign.FileName).ToLower();

                using (var img = System.Drawing.Image.FromStream(file_FrSign.InputStream))
                {
                    var FrSign = String.Format("~/Uploads/StudentRegistration/Sign/{0}{1}", fileName, extension);
                    SaveToFolder(img, fileName, extension, new Size(200, 200), FrSign);
                    model.FrSign = "../../Uploads/StudentRegistration/Sign/" + fileName + extension;
                }
            }


            var extmr = Path.GetExtension(file_MrSign.FileName);
            if (allowedExtensions.Contains(extmr))
            {
                var fileName = model.RegId + "_M_Sign";
                var extension = System.IO.Path.GetExtension(file_MrSign.FileName).ToLower();

                using (var img = System.Drawing.Image.FromStream(file_MrSign.InputStream))
                {
                    var MrSign = String.Format("~/Uploads/StudentRegistration/Sign/{0}{1}", fileName, extension);
                    SaveToFolder(img, fileName, extension, new Size(200, 200), MrSign);
                    model.MrSign = "../../Uploads/StudentRegistration/Sign/" + fileName + extension;
                }
            }

            //BASIC REGISTRATION//

            Student_Registration StudentRegistration = new Student_Registration()
            {

                RegId = model.RegId,
                ExamAttendingYear = model.ExamAttendingYear,
                PreferredDay = model.PreferredDay,
                ApplnDate = model.ApplnDate,
                AcademicYear = model.AcademicYear,
                AdmissionTestDate = model.AdmissionTestDate,
                StudentName = model.StudentName,
                PreferredTime = model.PreferredTime,
                WhatsappNo = model.WhatsappNo,
                DOB = model.DOB,
                Gender = model.Gender,
                Religion = model.Religion,
                Caste = model.Caste,
                Community = model.Community,
                BloodGroup = model.BloodGroup,
                Nationalty = model.Nationalty,
                PresentAddress = model.PresentAddress,
                Area = model.Area,
                Location = model.Location,
                State = model.State,
                District = model.District,
                Pincode = model.Pincode,
                QuickContNo = model.QuickContNo,
                Photo = model.Photo,
                QuickWhatsApp = model.QuickWhatsApp,
                PgmId = model.PgmId,
                ClassId = model.ClassId,
                CourseId = model.CourseId,
                SubPgmId = model.SubPgmId,
                CreatedDate = DateTime.Now,
                IsDeleted = 0
            };


            //PARENT REGISTRATION//

            Student_Parent StudentParent = new Student_Parent()
            {
                RegId = model.RegId,
                FrName = model.FrName,
                FrOcc = model.FrOcc,
                FrMobNo = model.FrMobNo,
                FrMailid = model.MailidFr,
                FrDistrict = model.FrDistrict,
                FrSign = model.FrSign,
                FrState = model.FrState,
                FrWhatsapp = model.WhatsappFr,
                MrName = model.MrName,
                MrOcc = model.MrOcc,
                MrMobNo = model.MrMobNo,
                MrMailid = model.MailidMr,
                MrDistrict = model.MrDistrict,
                MrSign = model.MrSign,
                MrState = model.MrState,
                MrWhatsapp = model.WhatsappMr,
                CreatedDate = DateTime.Now,
                IsDeleted = 0
            };

            //HOME DETAILS//

            Student_HomeCountryDetails StudentHomeCountryDetails = new Student_HomeCountryDetails()
            {
                RegId = model.RegId,
                AddressHome1 = model.homeAddress,
                AddressHome2 = model.Address2,
                AreaHome = model.AreaHome,
                PincodeHome = model.PincodeHome,
                QuickHomeContact = model.homeContact,
                LocationHome = model.LocationHome,
                StateHome = model.StateHome,
                EmaiIdHome = model.emailHome,
                QuickHomeWhatsapp = model.QuickHomeWhatsapp,
                DistrictHome = model.DistHome,
                CreatedDate = DateTime.Now,
                IsDeleted = 0

            };


            db.Student_Registration.Add(StudentRegistration);
            db.Student_Parent.Add(StudentParent);
            db.Student_HomeCountryDetails.Add(StudentHomeCountryDetails);
            await db.SaveChangesAsync();


            
            TempData["ApplicationName"] = model.StudentName;
            TempData["StatusMessage"] = "Thank You for your registration we will reach you soon.";
            return RedirectToAction("StudentRegisterSuccess");
        }


        public ActionResult StudentRegisterSuccess()
        {
            return RedirectToAction("Student");
        }


        public Size NewImageSize(Size imageSize, Size newSize)
        {
            Size finalSize;
            double tempval;
            if (imageSize.Height > newSize.Height || imageSize.Width > newSize.Width)
            {
                if (imageSize.Height > imageSize.Width)
                    tempval = newSize.Height / (imageSize.Height * 1.0);
                else
                    tempval = newSize.Width / (imageSize.Width * 1.0);

                finalSize = new Size((int)(tempval * imageSize.Width), (int)(tempval * imageSize.Height));
            }
            else
                finalSize = imageSize; // image is already small size

            return finalSize;
        }

        private void SaveToFolder(Image img, string fileName, string extension, Size newSize, string pathToSave)
        {
            // Get new resolution
            Size imgSize = NewImageSize(img.Size, newSize);
            using (System.Drawing.Image newImg = new Bitmap(img, imgSize.Width, imgSize.Height))
            {
                newImg.Save(Server.MapPath(pathToSave), img.RawFormat);
            }
        }
    }
}