using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;

namespace OnlineExam.Api
{
    public class RegisterController : ApiController
    {
        private readonly Exam_DBEntities db = new Exam_DBEntities();
        private readonly Exam_DBrole DbRole = new Exam_DBrole();

        [Route("api/Register/GetAcademic")]
        public IHttpActionResult GetAcademicPerformance(string regId)
        {
            var data = db.Student_AcademicPerformance.Where(e => e.RegId == regId).ToList();
            return Ok(data);
        }

        [Route("api/Register/GetEntrance")]
        public IHttpActionResult GetEntrance(string regId)
        {
            var data = db.Student_PreviousEntrance.Where(e => e.RegId == regId).ToList();
            return Ok(data);
        }


        [Route("api/Register/AcademicPerformance")]
        [ResponseType(typeof(Student_AcademicPerformance))]
        public IHttpActionResult AcademicPerformance(Student_AcademicPerformance student_AcademicPerformance)
        {
            student_AcademicPerformance.CreatedDate = DateTime.Now;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Student_AcademicPerformance.Add(student_AcademicPerformance);
            db.SaveChanges();


            return Ok(new { id = student_AcademicPerformance.Id });
        }


        [Route("api/Register/DeletePerformance/{id:int}")]
        [ResponseType(typeof(Student_AcademicPerformance))]
        public IHttpActionResult DeletePerformance(int id)
        {
            Student_AcademicPerformance student_AcademicPerformance = db.Student_AcademicPerformance.Find(id);
            if (student_AcademicPerformance == null)
            {
                return NotFound();
            }

            db.Student_AcademicPerformance.Remove(student_AcademicPerformance);
            db.SaveChanges();

            return Ok(student_AcademicPerformance);
        }        


        [Route("api/Register/PreviousEntrance")]
        [ResponseType(typeof(Student_PreviousEntrance))]
        public IHttpActionResult PreviousEntrance(Student_PreviousEntrance student_PreviousEntrance)
        {
            student_PreviousEntrance.CreatedDate = DateTime.Now;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Student_PreviousEntrance.Add(student_PreviousEntrance);
            db.SaveChanges();
            return Ok(new { id = student_PreviousEntrance.Id });
        }


        [Route("api/Register/DeletePreviousEntrance/{id:int}")]
        [ResponseType(typeof(Student_PreviousEntrance))]
        public IHttpActionResult DeletePreviousEntrance(int id)
        {
            Student_PreviousEntrance student_PreviousEntrance = db.Student_PreviousEntrance.Find(id);
            if (student_PreviousEntrance == null)
            {
                return NotFound();
            }

            db.Student_PreviousEntrance.Remove(student_PreviousEntrance);
            db.SaveChanges();

            return Ok(student_PreviousEntrance);
        }

        [Route("api/Register/adduser/{id}/{CuserId:int}")]
        [ResponseType(typeof(Student_PreviousEntrance))]
        public IHttpActionResult AddUserFromRegistration(string id, int CuserId)
        {
            GetAllStudentRegistrationByRegId_Result data = db.GetAllStudentRegistrationByRegId(id).FirstOrDefault();
            if (data == null)
            {
                return NotFound();
            }

            User acc = db.Users.Where(u => u.UniqueID == id || u.Email == data.FrMailid || u.UserName == id).FirstOrDefault();
            if(acc != null)
            {
                return Ok(new { Status = 0, Message = "The User (Unique ID or Email Id or Username) already Exist !" });
            }

            var roleid = 3;

            var user = new User
            {
                UserName = data.HregId,
                Email = data.FrMailid,
                MobileNo = data.FrMobNo,
                FirstName = data.StudentName,
                LastName = data.FrName,
                Password = data.FrMobNo,
                RoleId = roleid,
                UniqueID = data.HregId,
                CreatedDate = DateTime.Now,
                DeletedDate = DateTime.Now,
                CreatedBy = CuserId,
                ActivationCode = Guid.NewGuid().ToString()
            };

            db.Users.Add(user);
            db.SaveChanges();
            int userId = db.Users.Max(item => item.Id);
            AddRole(userId);

            return Ok(new { Status = 1, Message = data.StudentName + " added into user accound. Please find him on users list !", Data = user });

        }

        public void AddRole(int userId)
        {
                UserRole userRole = new UserRole()
                {
                    RoleId = 3,
                    UserId = userId
                };

                DbRole.UserRoles.Add(userRole);
                DbRole.SaveChangesAsync();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool Student_AcademicPerformanceExists(int id)
        {
            return db.Student_AcademicPerformance.Count(e => e.Id == id) > 0;
        }
    }
}
