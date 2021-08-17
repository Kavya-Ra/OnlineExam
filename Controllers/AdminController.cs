using OnlineExam.Authentication;
using OnlineExam.DbContext;
using OnlineExam.Models;
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
    [CustomAuthorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly Exam_DBEntities db = new Exam_DBEntities();
        private readonly Exam_DBrole DbRole = new Exam_DBrole();

        // GET: Admin
        public ActionResult Index()
        {
            return RedirectToAction("Dashboard");
        }

        public ActionResult Dashboard()
        {
            DateTime today = DateTime.Now.Date;

            AdminDashboardViewModel adminDashboard = new AdminDashboardViewModel()
            {
                GetExam = db.GetExamByDate(today).Where(e => e.IsActive == 1).ToList(),
                TotalExam = db.Exams.Where(e=>e.IsActive == 1).Count(),
                TotalBatch = db.Groups.Where(g=>g.IsDeleted == 0).Count(),
                TotalStudents = db.Users.Where(u=>u.RoleId == 3 && u.IsActive == 1).Count()
            };

            return View(adminDashboard);
        }

        public new ActionResult Profile()
        {
            return View();
        }

        public async Task<ActionResult> Role(int? id)
        {
            var allRoleData = await db.Roles.ToListAsync();

            if (id != null)
            {
                var oneRoleData = await db.Roles.Where(r => r.RoleId == id).FirstOrDefaultAsync();
                RoleViewModel roleView = new RoleViewModel()
                {
                    RoleName = oneRoleData.RoleName,
                    RoleId = oneRoleData.RoleId,
                    Roles = allRoleData
                };

                return View(roleView);
            }
            else
            {
                RoleViewModel roleView = new RoleViewModel()
                {
                    Roles = allRoleData
                };
                if (TempData["StatusMessage"] != null)
                {
                    ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
                }
                return View(roleView);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Role(RoleViewModel role)
        {
            var allRoleData = await db.Roles.ToListAsync();

            if (role.RoleId != null)
            {
                role.Roles = allRoleData;
                ViewBag.ErrorMessage = "Please Enter Role Name";
                return View(role);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var exists = await db.Roles.Where(r => r.RoleName == role.RoleName).FirstOrDefaultAsync();
                    if (exists != null)
                    {
                        role.Roles = allRoleData;
                        ViewBag.ErrorMessage = "Role Name already exists";
                        return View(role);
                    }
                    else
                    {
                        Role userRole = new Role()
                        {
                            RoleName = role.RoleName
                        };

                        db.Roles.Add(userRole);
                        await db.SaveChangesAsync();
                        TempData["StatusMessage"] = "Role Created Succesfully";
                        return RedirectToAction("Role");
                    }

                }

                role.Roles = allRoleData;
                ViewBag.ErrorMessage = "Please Enter Role Name";
                return View(role);
            }
        }

        public async Task<ActionResult> UserAccounts()
        {
            if (TempData["StatusMessage"] != null)
            {
                ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            var users = db.Users.Include(u => u.Roles);
            return View(await users.Where(u => u.IsDeleted == 0).ToListAsync());
        }

        public async Task<ActionResult> UserAccount(int? id)
        {
            if(id != null)
            {
                User data = await db.Users.Where(d => d.Id == id).FirstOrDefaultAsync();
                if (data != null)
                {
                    AccountViewModel user = new AccountViewModel
                    {
                        FirstName = data.FirstName,
                        LastName = data.LastName,
                        Email = data.Email,
                        MobileNo = data.MobileNo,
                        UserName = data.UserName,
                        RoleId = data.RoleId,
                        Password = data.Password,
                        ConfirmPassword = data.Password,
                        Roles = await db.Roles.ToListAsync()
                    };
                    return View(user);
                }                
            }

            AccountViewModel model = new AccountViewModel
            {
                Roles = await db.Roles.ToListAsync()
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UserAccount(AccountViewModel model)
        {
            if (model.Id != null)
            {
                User data = db.Users.Where(d => d.Id == model.Id).FirstOrDefault();
                if(data != null)
                {
                    if(data.Email != model.Email)
                    {
                        var check = db.Users.Where(d => d.Email == model.Email).FirstOrDefault();

                        if (check != null)
                        {
                            ViewBag.ErrorMessage = "Email already exists";
                            model.Roles = await db.Roles.ToListAsync();
                            return View(model);
                        }
                    }

                    if (data.UserName != model.UserName)
                    {
                        var check = db.Users.Where(d => d.UserName == model.UserName).FirstOrDefault();

                        if (check != null)
                        {
                            ViewBag.ErrorMessage = "Username already exists";
                            model.Roles = await db.Roles.ToListAsync();
                            return View(model);
                        }
                    }

                    if (ModelState.IsValid)
                    {
                        data.FirstName = model.FirstName;
                        data.LastName = model.LastName;
                        data.Email = model.Email;
                        data.MobileNo = model.MobileNo;
                        data.UserName = model.UserName;
                        data.Password = model.Password;
                        data.RoleId = model.RoleId;

                        db.Entry(data).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        AddRole(model);

                        TempData["StatusMessage"] = "Account Edited Succesfully.";
                        return RedirectToAction("UserAccounts");
                    }


                }

                model.Roles = await db.Roles.ToListAsync();
                ViewBag.ErrorMessage = "Please fill in all the required fields";
                return View(model);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    var data = db.Users.Where(d => d.Email == model.Email || d.UserName == model.UserName).FirstOrDefault();

                    if (data != null)
                    {
                        ViewBag.ErrorMessage = "Email or Username already exists";
                        model.Roles = await db.Roles.ToListAsync();
                        return View(model);
                    }

                    string alpha;
                    if (model.RoleId == 1)
                    {
                        alpha = "ECA";
                    }
                    else if (model.RoleId == 2)
                    {
                        alpha = "ECT";
                    }
                    else if (model.RoleId == 3)
                    {
                        alpha = "ECS";
                    }
                    else if (model.RoleId == 4)
                    {
                        alpha = "ECD";
                    }
                    else
                    {
                        alpha = "ECC";
                    }

                    Random random = new Random();
                    int unique = random.Next(10000, 99999);
                    int y = DateTime.Now.Year;
                    int m = DateTime.Now.Month;
                    var uniqueID = alpha + y + m + unique;

                    var user = new User
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                        MobileNo = model.MobileNo,
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        Password = model.Password,
                        RoleId = model.RoleId,
                        UniqueID = uniqueID,
                        CreatedDate = DateTime.Now,
                        DeletedDate = DateTime.Now,
                        CreatedBy = model.CuserId,
                        ActivationCode = Guid.NewGuid().ToString()
                    };                    

                    if (ModelState.IsValid)
                    {
                        db.Users.Add(user);
                        await db.SaveChangesAsync();

                        AddRole(model);

                        TempData["StatusMessage"] = "Account Created Succesfully";
                        return RedirectToAction("UserAccounts");
                    }

                    return View(model); ;
                }

                model.Roles = await db.Roles.ToListAsync();
                ViewBag.ErrorMessage = "Please fill in all the required fields";
                return View(model);
            }
        }

        public ViewResult AddRole(AccountViewModel model)
        {

            if (model.Id != null)
            {
                UserRole userR = (UserRole)DbRole.UserRoles.Where(u => u.UserId == model.Id).FirstOrDefault();
                DbRole.UserRoles.Remove(userR);
                DbRole.SaveChanges();

                UserRole userRole = new UserRole()
                {
                    RoleId = model.RoleId,
                    UserId = (int)model.Id
                };
                DbRole.UserRoles.Add(userRole);
                DbRole.SaveChangesAsync();
            }
            else
            {
                var user = db.Users.Where(u => u.UserName == model.UserName).FirstOrDefault();
                UserRole userRole = new UserRole()
                {
                    RoleId = model.RoleId,
                    UserId = user.Id
                };

                DbRole.UserRoles.Add(userRole);
                DbRole.SaveChangesAsync();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActiveAccount(int? activeId)
        {
            if (activeId == null)
            {
                TempData["ErrorMessage"] = "Account Not Activated";
                return RedirectToAction("UserAccounts");
            }
            else
            {
                User programmes = await db.Users.Where(c => c.Id == activeId).FirstOrDefaultAsync();
                if (programmes != null)
                {
                    programmes.IsActive = 1;
                    db.Entry(programmes).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "Account Activated Succesfully.";
                    return RedirectToAction("UserAccounts");
                }
                TempData["ErrorMessage"] = "Account Not Activated";
                return RedirectToAction("UserAccounts");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> InactiveAccount(int? inactiveId)
        {
            if (inactiveId == null)
            {
                TempData["ErrorMessage"] = "Account Not Inactivated";
                return RedirectToAction("UserAccounts");
            }
            else
            {
                User programmes = await db.Users.Where(c => c.Id == inactiveId).FirstOrDefaultAsync();
                if (programmes != null)
                {
                    programmes.IsActive = 0;
                    db.Entry(programmes).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "Account Inactivated Succesfully.";
                    return RedirectToAction("UserAccounts");
                }
                TempData["ErrorMessage"] = "Account Not Inactivated";
                return RedirectToAction("UserAccounts");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteAccount(int? deleteUserId)
        {
            if (deleteUserId == null)
            {
                TempData["ErrorMessage"] = "Account Not Deleted";
                return RedirectToAction("UserAccounts");
            }
            else
            {
                User programmes = await db.Users.Where(c => c.Id == deleteUserId).FirstOrDefaultAsync();
                if (programmes != null)
                {
                    programmes.IsDeleted = 1;
                    db.Entry(programmes).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "Account Deleted Succesfully.";
                    return RedirectToAction("UserAccounts");
                }
                TempData["ErrorMessage"] = "Account Not Deleted";
                return RedirectToAction("UserAccounts");
            }
        }

        public async Task<ActionResult> Programmes(int? id)
        {

            if (id == null)
            {
                ProgrammeViewModel viewModel = new ProgrammeViewModel()
                {
                    programmes = await db.Programmes.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                if (TempData["StatusMessage"] != null)
                {
                    ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
                }

                return View(viewModel);
            }
            else
            {
                var data = await db.Programmes.Where(d => d.Id == id).FirstOrDefaultAsync();
                ProgrammeViewModel viewModel = new ProgrammeViewModel()
                {
                    Id = data.Id,
                    Name = data.Name,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    IsDeleted = data.IsDeleted,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedTime = data.ModifiedTime,
                    DeletedDate = data.DeletedDate,
                    programmes = await db.Programmes.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Programmes(ProgrammeViewModel programmes)
        {
            var userId = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;

            if (programmes.Id != null)
            {
                if (ModelState.IsValid)
                {
                    Programme model = await db.Programmes.Where(c => c.Id == programmes.Id).FirstOrDefaultAsync();
                    model.Name = programmes.Name;
                    model.ModifiedTime = DateTime.Now;
                    model.ModifiedBy = userId;

                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Programme Edited Succesfully.";
                    return RedirectToAction("ProgrammesEdit");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                programmes.programmes = await db.Programmes.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(programmes);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    Programme model = new Programme()
                    {
                        Name = programmes.Name,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        ModifiedTime = DateTime.Now,
                        DeletedDate = DateTime.Now
                    };

                    db.Programmes.Add(model);
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Programme Created Succesfully.";
                    return RedirectToAction("Programmes");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                programmes.programmes = await db.Programmes.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(programmes);
            }
        }

        public ActionResult ProgrammesEdit()
        {
            return RedirectToAction("Programmes");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ProgrammeDelete(int? programmeId)
        {
            if (programmeId == null)
            {
                TempData["ErrorMessage"] = "Account Not Deleted";
                return RedirectToAction("Programmes");
            }
            else
            {
                Programme programmes = await db.Programmes.Where(c => c.Id == programmeId).FirstOrDefaultAsync();

                if (programmes != null)
                {
                    programmes.IsDeleted = 1;
                    programmes.DeletedDate = DateTime.Now;
                    db.Entry(programmes).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "Account Deleted Succesfully.";
                    return RedirectToAction("Programmes");
                }

                TempData["ErrorMessage"] = "Account Not Deleted";
                return RedirectToAction("Programmes");
            }
        }

        public async Task<ActionResult> SubProgramme(int? id)
        {
            var data1 = db.SubPrograms.Include(d => d.Programme);
            var subProg = await data1.Where(p => p.IsDeleted == 0).ToListAsync();

            if (id == null)
            {
                SubProgramViewModel viewModel = new SubProgramViewModel()
                {
                    SubPrograms = subProg,
                    Programmes = await db.Programmes.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                if (TempData["StatusMessage"] != null)
                {
                    ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
                }

                return View(viewModel);
            }
            else
            {
                var data = await db.SubPrograms.Where(d => d.Id == id).FirstOrDefaultAsync();
                SubProgramViewModel viewModel = new SubProgramViewModel()
                {
                    Id = data.Id,
                    Name = data.Name,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    IsDeleted = data.IsDeleted,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedTime = data.ModifiedTime,
                    DeletedDate = data.DeletedDate,
                    PgmId = data.PgmId,
                    SubPrograms = subProg,
                    Programmes = await db.Programmes.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubProgramme(SubProgramViewModel subProgram)
        {
            var userId = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;

            if (subProgram.Id != null)
            {
                if (ModelState.IsValid)
                {
                    SubProgram model = await db.SubPrograms.Where(c => c.Id == subProgram.Id).FirstOrDefaultAsync();
                    model.Name = subProgram.Name;
                    model.ModifiedTime = DateTime.Now;
                    model.ModifiedBy = userId;
                    model.PgmId = subProgram.PgmId;

                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Sub Programme Edited Succesfully.";
                    return RedirectToAction("EditSubProgramme");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";

                var data = db.SubPrograms.Include(d => d.Programme);
                subProgram.SubPrograms = await data.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(subProgram);
            }
            else
            {
                SubProgram model = new SubProgram()
                {
                    Name = subProgram.Name,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    DeletedDate = DateTime.Now,
                    PgmId = subProgram.PgmId
                };

                if (ModelState.IsValid)
                {
                    db.SubPrograms.Add(model);
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Sub Programme Created Succesfully.";
                    return RedirectToAction("SubProgramme");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields"; 
                var data = db.SubPrograms.Include(d => d.Programme);
                subProgram.SubPrograms = await data.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(subProgram);
            }
        }

        public ActionResult EditSubProgramme()
        {
            return RedirectToAction("SubProgramme");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubProgrammeDelete(int? subProgrammeId)
        {
            if (subProgrammeId == null)
            {
                TempData["ErrorMessage"] = "sub Programme Not Deleted";
                return RedirectToAction("SubProgramme");
            }
            else
            {
                SubProgram subProgramme = await db.SubPrograms.Where(c => c.Id == subProgrammeId).FirstOrDefaultAsync();

                if (subProgramme != null)
                {
                    subProgramme.IsDeleted = 1;
                    db.Entry(subProgramme).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "sub Programme Deleted Succesfully.";
                    return RedirectToAction("SubProgramme");
                }

                TempData["ErrorMessage"] = "sub Programme Not Deleted";
                return RedirectToAction("SubProgramme");
            }
        }


        public async Task<ActionResult> StdClass(int? id)
        {

            if (id == null)
            {
                StdClassViewModel viewModel = new StdClassViewModel()
                {
                    Classes = await db.Classes.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                if (TempData["StatusMessage"] != null)
                {
                    ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
                }

                return View(viewModel);
            }
            else
            {
                var data = await db.Classes.Where(d => d.Id == id).FirstOrDefaultAsync();
                StdClassViewModel viewModel = new StdClassViewModel()
                {
                    Id = data.Id,
                    Name = data.Name,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    IsDeleted = data.IsDeleted,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedTime = data.ModifiedTime,
                    DeletedDate = data.DeletedDate,
                    Classes = await db.Classes.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StdClass(StdClassViewModel stdClassView)
        {
            var userId = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;

            if (stdClassView.Id != null)
            {
                if (ModelState.IsValid)
                {
                    Class model = await db.Classes.Where(c => c.Id == stdClassView.Id).FirstOrDefaultAsync();
                    model.Name = stdClassView.Name;
                    model.ModifiedTime = DateTime.Now;
                    model.ModifiedBy = userId;

                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Class Edited Succesfully.";
                    return RedirectToAction("StdClassEdit");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                stdClassView.Classes = await db.Classes.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(stdClassView);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    Class model = new Class()
                    {
                        Name = stdClassView.Name,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        ModifiedTime = DateTime.Now,
                        DeletedDate = DateTime.Now
                    };

                    db.Classes.Add(model);
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Class Created Succesfully.";
                    return RedirectToAction("StdClass");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                stdClassView.Classes = await db.Classes.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(stdClassView);
            }
        }

        public ActionResult StdClassEdit()
        {
            return RedirectToAction("StdClass");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StdClassDelete(int? classId)
        {
            if (classId == null)
            {
                TempData["ErrorMessage"] = "Class Not Deleted";
                return RedirectToAction("StdClass");
            }
            else
            {
                Class stdClass = await db.Classes.Where(c => c.Id == classId).FirstOrDefaultAsync();

                if (classId != null)
                {
                    stdClass.IsDeleted = 1;
                    stdClass.DeletedDate = DateTime.Now;
                    db.Entry(stdClass).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "Class Deleted Succesfully.";
                    return RedirectToAction("StdClass");
                }

                TempData["ErrorMessage"] = "Class Not Deleted";
                return RedirectToAction("StdClass");
            }
        }

        public async Task<ActionResult> Course(int? id)
        {
            var classes = db.Courses.Include(c => c.Class);
            var courses = await classes.Where(p => p.IsDeleted == 0).ToListAsync();


            if (id == null)
            {
                CourseViewModel viewModel = new CourseViewModel()
                {
                    Courses = courses,
                    Classes = await db.Classes.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                if (TempData["StatusMessage"] != null)
                {
                    ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
                }

                return View(viewModel);
            }
            else
            {
                var data = await db.Courses.Where(d => d.Id == id).FirstOrDefaultAsync();
                CourseViewModel viewModel = new CourseViewModel()
                {
                    Id = data.Id,
                    Name = data.Name,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    IsDeleted = data.IsDeleted,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedTime = data.ModifiedTime,
                    DeletedDate = data.DeletedDate,
                    ClassId = data.ClassId,
                    Courses = courses,
                    Classes = await db.Classes.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Course(CourseViewModel courseView)
        {
            var userId = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;

            if (courseView.Id != null)
            {
                if (ModelState.IsValid)
                {
                    Cours model = await db.Courses.Where(c => c.Id == courseView.Id).FirstOrDefaultAsync();
                    model.Name = courseView.Name;
                    model.ModifiedTime = DateTime.Now;
                    model.ModifiedBy = userId;
                    model.ClassId = courseView.ClassId;

                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Course Edited Succesfully.";
                    return RedirectToAction("EditCourse");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                var classes = db.Courses.Include(c => c.Class);
                courseView.Courses = await classes.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(courseView);
            }
            else
            {
                Cours model = new Cours()
                {
                    Name = courseView.Name,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    DeletedDate = DateTime.Now,
                    ClassId = courseView.ClassId
                };

                if (ModelState.IsValid)
                {
                    db.Courses.Add(model);
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Course Created Succesfully.";
                    return RedirectToAction("Course");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                var classes = db.Courses.Include(c => c.Class);
                courseView.Courses = await classes.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(courseView);
            }
        }

        public ActionResult EditCourse()
        {
            return RedirectToAction("Course");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CourseDelete(int? CourseId)
        {
            if (CourseId == null)
            {
                TempData["ErrorMessage"] = "Course Not Deleted";
                return RedirectToAction("Course");
            }
            else
            {
                Cours course = await db.Courses.Where(c => c.Id == CourseId).FirstOrDefaultAsync();

                if (course != null)
                {
                    course.IsDeleted = 1;
                    db.Entry(course).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "Course Deleted Succesfully.";
                    return RedirectToAction("Course");
                }

                TempData["ErrorMessage"] = "Course Not Deleted";
                return RedirectToAction("Course");
            }
        }

        public async Task<ActionResult> Subjects(int? id)
        {

            if (id == null)
            {
                SubjectViewModel viewModel = new SubjectViewModel()
                {
                    Subjects = await db.Subjects.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                if (TempData["StatusMessage"] != null)
                {
                    ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
                }

                return View(viewModel);
            }
            else
            {
                var data = await db.Subjects.Where(d => d.Id == id).FirstOrDefaultAsync();
                SubjectViewModel viewModel = new SubjectViewModel()
                {
                    Id = data.Id,
                    Name = data.Name,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    IsDeleted = data.IsDeleted,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedTime = data.ModifiedTime,
                    DeletedDate = data.DeletedDate,
                    Subjects = await db.Subjects.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Subjects(SubjectViewModel subjectView)
        {
            var userId = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;

            if (subjectView.Id != null)
            {
                if (ModelState.IsValid)
                {
                    Subject model = await db.Subjects.Where(c => c.Id == subjectView.Id).FirstOrDefaultAsync();
                    model.Name = subjectView.Name;
                    model.ModifiedTime = DateTime.Now;
                    model.ModifiedBy = userId;

                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Subject Edited Succesfully.";
                    return RedirectToAction("SubjectEdit");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                subjectView.Subjects = await db.Subjects.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(subjectView);
            }
            else
            {
                if (ModelState.IsValid)
                {
                    Subject model = new Subject()
                    {
                        Name = subjectView.Name,
                        CreatedBy = userId,
                        CreatedDate = DateTime.Now,
                        ModifiedTime = DateTime.Now,
                        DeletedDate = DateTime.Now
                    };

                    db.Subjects.Add(model);
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Subject Created Succesfully.";
                    return RedirectToAction("Subjects");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                subjectView.Subjects = await db.Subjects.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(subjectView);
            }
        }

        public ActionResult SubjectEdit()
        {
            return RedirectToAction("Subjects");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SubjectDelete(int? subjectId)
        {
            if (subjectId == null)
            {
                TempData["ErrorMessage"] = "Subject Not Deleted";
                return RedirectToAction("Subjects");
            }
            else
            {
                Subject subject = await db.Subjects.Where(c => c.Id == subjectId).FirstOrDefaultAsync();

                if (subjectId != null)
                {
                    subject.IsDeleted = 1;
                    subject.DeletedDate = DateTime.Now;
                    db.Entry(subject).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "Subject Deleted Succesfully.";
                    return RedirectToAction("Subjects");
                }

                TempData["ErrorMessage"] = "Subject Not Deleted";
                return RedirectToAction("Subjects");
            }
        }

        public async Task<ActionResult> Chapters(int? id)
        {
            var Subjects =  db.Subjects.Where(p => p.IsDeleted == 0);
            var chapter = db.Chapters.Include(c=>c.Subject);

            if (id == null)
            {

                ChapterViewModel viewModel = new ChapterViewModel()
                {
                    Subjects = await Subjects.ToListAsync(),
                    Chapters = await chapter.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                if (TempData["StatusMessage"] != null)
                {
                    ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
                }

                if (TempData["ErrorMessage"] != null)
                {
                    ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
                }

                return View(viewModel);
            }
            else
            {
                var data = await db.Chapters.Where(d => d.Id == id).FirstOrDefaultAsync();
                ChapterViewModel viewModel = new ChapterViewModel()
                {
                    Id = data.Id,
                    Name = data.Name,
                    CreatedBy = data.CreatedBy,
                    CreatedDate = data.CreatedDate,
                    IsDeleted = data.IsDeleted,
                    ModifiedBy = data.ModifiedBy,
                    ModifiedTime = data.ModifiedTime,
                    DeletedDate = data.DeletedDate,
                    SubId = data.SubId,
                    Subjects = await Subjects.ToListAsync(),
                    Chapters = await chapter.Where(p => p.IsDeleted == 0).ToListAsync()
                };

                return View(viewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Chapters(ChapterViewModel chapter)
        {
            var userId = db.Users.Where(x => x.UserName == User.Identity.Name).FirstOrDefault().Id;
            var chapterList = db.Chapters.Include(c => c.Subject);

            if (chapter.Id != null)
            {
                if (ModelState.IsValid)
                {
                    Chapter model = await db.Chapters.Where(c => c.Id == chapter.Id).FirstOrDefaultAsync();
                    model.Name = chapter.Name;
                    model.ModifiedTime = DateTime.Now;
                    model.ModifiedBy = userId;
                    model.SubId = chapter.SubId;

                    db.Entry(model).State = EntityState.Modified;
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Chapter Edited Succesfully.";
                    return RedirectToAction("EditCourse");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                chapter.Chapters = await chapterList.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(chapter);
            }
            else
            {
                Chapter model = new Chapter()
                {
                    Name = chapter.Name,
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now,
                    ModifiedTime = DateTime.Now,
                    DeletedDate = DateTime.Now,
                    SubId = chapter.SubId
                };

                if (ModelState.IsValid)
                {
                    db.Chapters.Add(model);
                    await db.SaveChangesAsync();

                    TempData["StatusMessage"] = "Chapter Created Succesfully.";
                    return RedirectToAction("Chapters");
                }

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                chapter.Chapters = await chapterList.Where(p => p.IsDeleted == 0).ToListAsync();
                return View(chapter);
            }
        }

        public ActionResult EditChapter()
        {
            return RedirectToAction("Chapters");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChapterDelete(int? chapterId)
        {
            if (chapterId == null)
            {
                TempData["ErrorMessage"] = "Chapter Not Deleted";
                return RedirectToAction("Chapters");
            }
            else
            {
                Chapter chapter = await db.Chapters.Where(c => c.Id == chapterId).FirstOrDefaultAsync();

                if (chapterId != null)
                {
                    chapter.IsDeleted = 1;
                    db.Entry(chapter).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "Chapter Deleted Succesfully.";
                    return RedirectToAction("Chapters");
                }

                TempData["ErrorMessage"] = "Chapter Not Deleted";
                return RedirectToAction("Chapters");
            }
        }


        public ActionResult StudentRegistrations()
        {
            var studreg = db.AllStudentRegistrationDetails();
            return View(studreg);
        }

        public ActionResult StudentRegView(string regId)
        {
            var data = db.GetAllStudentRegistrationByRegId(regId).FirstOrDefault();
            var data1 = db.Student_AcademicPerformancebyRegid(regId).ToList();
            var data2 = db.Student_PreviousEntrancebyRegid(regId).ToList();


            StudentRegPdfViewModel student = new StudentRegPdfViewModel()
            {
                GetAllStudentRegistrationByRegId = data,
                Student_AcademicPerformancebyRegid = data1,
                Student_PreviousEntrancebyRegid = data2

            };

            return View(student);
        }

        public ActionResult TeacherRegistrations()
        {
            var teacher = db.Teachers_Registration.Where(s => s.IsDeleted == 0).ToList();
            return View(teacher);
        }

        public ActionResult TeacherRegView(string regId)
        {
            var data = db.Teachers_Registration.Where(t=>t.TeachRegId == regId).FirstOrDefault();
            return View(data);
        }

        public ActionResult Groups()
        {
            var data = db.GetAllGroupList().Where(g=>g.IsDeleted==0).ToList();
            return View(data);
        }

        public async Task<ActionResult> Group(int? id)
        {
            if (id == null)
            {
                ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name");
                ViewBag.ClassId = new SelectList(db.Classes.Where(c => c.IsDeleted == 0), "Id", "Name");
                ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name");
                ViewBag.TeacherId = new SelectList(db.Users.Where(p => p.IsDeleted == 0 && p.RoleId == 2), "Id", "UniqueID");
                ViewBag.StudentId = new SelectList(db.Users.Where(p => p.IsDeleted == 0 && p.RoleId == 3), "Id", "UniqueID");
                ViewBag.SubPgmId = new SelectList(Enumerable.Empty<SelectListItem>());
                ViewBag.CourseId = new SelectList(Enumerable.Empty<SelectListItem>());
            }
            else
            {

                var data = await db.Groups.Where(d => d.Id == id).FirstOrDefaultAsync();
                GroupViewModel groupView = new GroupViewModel()
                {
                    Id = data.Id,
                    GroupName = data.GroupName,
                    PgmId = data.PgmId,
                    SubPgmId = data.SubPgmId,
                    ClassId = data.ClassId,
                    CourseId = data.CourseId,
                    SubjectId = data.SubjectId,
                };

                ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", data.PgmId);
                ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", data.SubjectId);
                ViewBag.ClassId = new SelectList(db.Classes.Where(p => p.IsDeleted == 0), "Id", "Name", data.ClassId);
                ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == data.ClassId), "Id", "Name", data.CourseId);
                ViewBag.SubPgmId = new SelectList(db.SubPrograms.Where(p => p.IsDeleted == 0 && p.PgmId == data.PgmId), "Id", "Name", data.SubPgmId);


                var dteach = db.Group_Teacher.Where(d => d.GroupId == id).ToList();
                List<int> tList = new List<int>();
                foreach (var item in dteach)
                {
                    tList.Add(item.TeacherId);
                }
                var teach = new MultiSelectList(db.Users.Where(p => p.IsDeleted == 0 && p.RoleId == 2), "Id", "UniqueID", tList).ToList();
                ViewBag.TeacherId = teach;

                var datastud = db.Group_StudentTable.Where(d => d.GroupId == id).ToList();
                List<int> SList = new List<int>();
                foreach (var item in datastud)
                {
                    SList.Add(item.StudentId);
                }
                var stud = new MultiSelectList(db.Users.Where(p => p.IsDeleted == 0 && p.RoleId == 3), "Id", "UniqueID", SList).ToList();
                ViewBag.StudentId = stud;
                
                return View(groupView);

            }
            return View();
        }

        [HttpGet]
        public JsonResult GetSubProgram(int ID)
        {
            var sub = new SelectList(db.SubPrograms.Where(s => s.PgmId == ID), "Id", "Name");
            return Json(sub, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public JsonResult GetCourse(int ID)
        {
            var chap = new SelectList(db.Courses.Where(c => c.ClassId == ID), "Id", "Name");
            return Json(chap, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Group(GroupViewModel groupView)
        {

            if (groupView.Id != null)
            {
                Group data = db.Groups.Find(groupView.Id);
                if (data != null)
                {
                    if (ModelState.IsValid)
                    {
                        data.GroupName = groupView.GroupName;
                        data.PgmId = groupView.PgmId;
                        data.SubPgmId = groupView.SubPgmId;
                        data.ClassId = groupView.ClassId;
                        data.CourseId = groupView.CourseId;
                        data.SubjectId = groupView.SubjectId;
                        data.ModifiedDateTime = DateTime.Now;
                        data.ModifiedBy = 1;
                        db.Entry(data).State = EntityState.Modified;
                        await db.SaveChangesAsync();

                        int gid = data.Id;

                        var dteach = db.Group_Teacher.Where(d => d.GroupId == gid).ToList();
                        int[] tData = groupView.TeacherId;

                        foreach (var item in tData)
                        {
                            var tGroup = dteach.Where(at => at.TeacherId == item && at.GroupId == gid).FirstOrDefault();
                            if (tGroup == null)
                            {
                                Group_Teacher teacher = new Group_Teacher
                                {
                                    TeacherId = item,
                                    GroupId = gid
                                };
                                db.Group_Teacher.Add(teacher);
                            }
                        }

                        foreach (var item in dteach)
                        {
                            if (!tData.Contains(item.TeacherId))
                            {
                                db.Group_Teacher.Remove(item);
                            }
                        }


                        var Steach = db.Group_StudentTable.Where(d => d.GroupId == gid).ToList();
                        int[] sData = groupView.StudentId;

                        foreach (var item in sData)
                        {
                            var sGroup = Steach.Where(at => at.StudentId == item && at.GroupId == gid).FirstOrDefault();
                            if (sGroup == null)
                            {
                                Group_StudentTable student = new Group_StudentTable
                                {
                                    StudentId = item,
                                    GroupId = gid
                                };

                                db.Group_StudentTable.Add(student);
                            }
                        }

                        foreach (var item in Steach)
                        {
                            if (!sData.Contains(item.StudentId))
                            {
                                db.Group_StudentTable.Remove(item);
                            }
                        }

                        await db.SaveChangesAsync();

                        return RedirectToAction("Groups");
                    }
                }

                ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", groupView.PgmId);
                ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", groupView.SubjectId);
                ViewBag.ClassId = new SelectList(db.Classes.Where(p => p.IsDeleted == 0), "Id", "Name", groupView.ClassId);
                ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == groupView.ClassId), "Id", "Name", groupView.CourseId);
                ViewBag.SubPgmId = new SelectList(db.SubPrograms.Where(p => p.IsDeleted == 0 && p.PgmId == groupView.PgmId), "Id", "Name", groupView.SubPgmId);

                ViewBag.TeacherId = new SelectList(db.Users.Where(p => p.IsDeleted == 0 && p.RoleId == 2), "Id", "FirstName");
                ViewBag.StudentId = new SelectList(db.Users.Where(p => p.IsDeleted == 0 && p.RoleId == 3), "Id", "FirstName");

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                return View(groupView);

            }
            else
            {
                if (ModelState.IsValid)
                {
                    Group group = new Group
                    {
                        GroupName = groupView.GroupName,
                        PgmId = groupView.PgmId,
                        SubPgmId = groupView.SubPgmId,
                        ClassId = groupView.ClassId,
                        CourseId = groupView.CourseId,
                        SubjectId = groupView.SubjectId,
                        CreatedDateTime = DateTime.Now,
                        ModifiedDateTime = DateTime.Now,
                        DeletedDateTime = DateTime.Now
                    };
                    db.Groups.Add(group);
                    await db.SaveChangesAsync();

                    int gid = group.Id;
                    int[] tData = groupView.TeacherId;

                    foreach (var item in tData)
                    {
                        Group_Teacher teacher = new Group_Teacher
                        {
                            TeacherId = item,
                            GroupId = gid
                        };
                        db.Group_Teacher.Add(teacher);
                    }
                    db.SaveChanges();

                    int[] sData = groupView.StudentId;
                    foreach (var items in sData)
                    {
                        Group_StudentTable studentTable = new Group_StudentTable
                        {
                            StudentId = items,
                            GroupId = gid
                        };
                        db.Group_StudentTable.Add(studentTable);
                    }
                    db.SaveChanges();
                    return RedirectToAction("Groups");
                }

                ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", groupView.PgmId);
                ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", groupView.SubjectId);
                ViewBag.ClassId = new SelectList(db.Classes.Where(p => p.IsDeleted == 0), "Id", "Name", groupView.ClassId);
                ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == groupView.ClassId), "Id", "Name", groupView.CourseId);
                ViewBag.SubPgmId = new SelectList(db.SubPrograms.Where(p => p.IsDeleted == 0 && p.PgmId == groupView.PgmId), "Id", "Name", groupView.SubPgmId);

                ViewBag.TeacherId = new SelectList(db.Users.Where(p => p.IsDeleted == 0 && p.RoleId == 2), "Id", "FirstName");
                ViewBag.StudentId = new SelectList(db.Users.Where(p => p.IsDeleted == 0 && p.RoleId == 3), "Id", "FirstName");

                ViewBag.ErrorMessage = "Please fill in all the required fields";
                return View(groupView);
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteGroup(int? groupId)
        {
            if (groupId == null)
            {
                TempData["ErrorMessage"] = "Account Not Deleted";
                return RedirectToAction("Groups");
            }
            else
            {
                Group group = db.Groups.Find(groupId);
                if (group != null)
                {
                    group.IsDeleted = 1;
                    group.DeletedDateTime = DateTime.Now;
                    db.Entry(group).State = EntityState.Modified;
                    db.SaveChanges();
                    TempData["StatusMessage"] = "Account Deleted Succesfully.";
                    return RedirectToAction("Groups");
                }
                TempData["ErrorMessage"] = "Account Not Deleted";
                return RedirectToAction("Groups");
            }
        }

        public ActionResult QsAs()
        {
            if (TempData["StatusMessage"] != null)
            {
                ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            List<User> dtp = db.Users.Where(u => u.RoleId == 4 && u.IsActive == 1 && u.IsDeleted == 0).ToList();
            return View(dtp);
        }

        public ActionResult QaAsList(int? id)
        {
            if (TempData["StatusMessage"] != null)
            {
                ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            var list = db.GetAllDtpQusAnsByUserId(id).ToList();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActiveQsAs(int? activeId)
        {
            if (activeId == null)
            {
                TempData["ErrorMessage"] = "QsAs Not Activated";
                return RedirectToAction("QaAsList");
            }
            else
            {
                DataEntry_QuestionBank programmes = await db.DataEntry_QuestionBank.Where(c => c.Id == activeId).FirstOrDefaultAsync();
                if (programmes != null)
                {
                    programmes.IsActive = 1;
                    db.Entry(programmes).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "QsAs Activated Succesfully.";
                    return RedirectToAction("QaAsList", new { Id = programmes.CreatedBy });
                }
                TempData["ErrorMessage"] = "QsAs Not Activated";
                return RedirectToAction("QsAs");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> InactiveQsAs(int? inactiveId)
        {
            if (inactiveId == null)
            {
                TempData["ErrorMessage"] = "QsAs Not Inactivated";
                return RedirectToAction("QsAs");
            }
            else
            {
                DataEntry_QuestionBank programmes = await db.DataEntry_QuestionBank.Where(c => c.Id == inactiveId).FirstOrDefaultAsync();
                if (programmes != null)
                {
                    programmes.IsActive = 0;
                    db.Entry(programmes).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "QsAs Inactivated Succesfully.";
                    return RedirectToAction("QaAsList", new { Id = programmes.CreatedBy });
                }
                TempData["ErrorMessage"] = "QsAs Not Inactivated";
                return RedirectToAction("QsAs");
            }
        }

        public async Task<ActionResult> QsAsDetails(int? id)
        {
            if(id != null)
            {
                var data = await db.DataEntry_QuestionBank.Where(d => d.Id == id).FirstOrDefaultAsync();
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
                    Photo = data.Photo,
                    CuserId = data.CreatedBy
                };

                ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", data.PgmId);
                ViewBag.ClassId = new SelectList(db.Classes.Where(s => s.IsDeleted == 0), "Id", "Name", data.ClassId);
                ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == data.ClassId), "Id", "Name", data.CourseId);
                ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", data.SubjectId);
                ViewBag.ChapterId = new SelectList(db.Chapters.Where(p => p.IsDeleted == 0 && p.SubId == data.SubjectId), "Id", "Name", data.ChapterId);

                return View(dtpQA);
            }

            TempData["ErrorMessage"] = "Invalid Try";
            return RedirectToAction("QsAs");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> QsAsDetails(QsAsViewModel dtpQAView, HttpPostedFileBase fileQus)
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
                            uploadPath = Path.Combine(Server.MapPath("~/Uploads/QuestionDtp/"), myfile);
                            dtpQAView.Photo = "../../Uploads/QuestionDtp/" + myfile;
                        }
                    }

                    DataEntry_QuestionBank data = db.DataEntry_QuestionBank.Find(dtpQAView.Id);
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
                        return RedirectToAction("QaAsList", new { Id = data.CreatedBy });
                    }
                }               
            }

            ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", dtpQAView.PgmId);
            ViewBag.ClassId = new SelectList(db.Classes.Where(p => p.IsDeleted == 0), "Id", "Name", dtpQAView.ClassId);
            ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == dtpQAView.ClassId), "Id", "Name", dtpQAView.CourseId);
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", dtpQAView.SubjectId);
            ViewBag.ChapterId = new SelectList(db.Chapters.Where(p => p.IsDeleted == 0 && p.SubId == dtpQAView.SubjectId), "Id", "Name", dtpQAView.ChapterId);

            ViewBag.ErrorMessage = "Please fill in all the required fields";
            return View(dtpQAView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteQsASDtp(int? deleteQsAsId)
        {
            if (deleteQsAsId != null)
            {
                DataEntry_QuestionBank dataEntry = db.DataEntry_QuestionBank.Find(deleteQsAsId);
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

        public ActionResult QsAsTeacher()
        {
            if (TempData["StatusMessage"] != null)
            {
                ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            List<User> dtp = db.Users.Where(u => u.RoleId == 2 && u.IsActive == 1 && u.IsDeleted == 0).ToList();
            return View(dtp);
        }

        public ActionResult TeacherQsAsList(int? id)
        {
            if (TempData["StatusMessage"] != null)
            {
                ViewBag.StatusMessage = TempData["StatusMessage"].ToString();
            }

            if (TempData["ErrorMessage"] != null)
            {
                ViewBag.ErrorMessage = TempData["ErrorMessage"].ToString();
            }

            var list = db.GetAllTeacherQusAnsByUserId(id).ToList();
            return View(list);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ActiveQsAsTeacher(int? activeId)
        {
            if (activeId == null)
            {
                TempData["ErrorMessage"] = "QsAs Not Activated";
                return RedirectToAction("QsAsTeacher");
            }
            else
            {
                Teachers_QuestionBank programmes = await db.Teachers_QuestionBank.Where(c => c.Id == activeId).FirstOrDefaultAsync();
                if (programmes != null)
                {
                    programmes.IsActive = 1;
                    db.Entry(programmes).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "QsAs Activated Succesfully.";
                    return RedirectToAction("TeacherQsAsList", new { Id = programmes.CreatedBy });
                }
                TempData["ErrorMessage"] = "QsAs Not Activated";
                return RedirectToAction("QsAsTeacher");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> InactiveQsAsTeacher(int? inactiveId)
        {
            if (inactiveId == null)
            {
                TempData["ErrorMessage"] = "QsAs Not Inactivated";
                return RedirectToAction("QsAsTeacher");
            }
            else
            {
                Teachers_QuestionBank programmes = await db.Teachers_QuestionBank.Where(c => c.Id == inactiveId).FirstOrDefaultAsync();
                if (programmes != null)
                {
                    programmes.IsActive = 0;
                    db.Entry(programmes).State = EntityState.Modified;
                    await db.SaveChangesAsync();
                    TempData["StatusMessage"] = "QsAs Inactivated Succesfully.";
                    return RedirectToAction("TeacherQsAsList", new { Id = programmes.CreatedBy });
                }
                TempData["ErrorMessage"] = "QsAs Not Inactivated";
                return RedirectToAction("QsAsTeacher");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteQsASTeacher(int? deleteQsAsId)
        {
            if (deleteQsAsId != null)
            {
                Teachers_QuestionBank dataEntry = db.Teachers_QuestionBank.Find(deleteQsAsId);
                dataEntry.IsDeleted = 1;
                dataEntry.DeletedDateTime = DateTime.Now;
                db.Entry(dataEntry).State = EntityState.Modified;
                db.SaveChanges();

                TempData["StatusMessage"] = "Questions Answers Deleted Succesfully.";
                return RedirectToAction("TeacherQsAsList", new { Id = dataEntry.CreatedBy });
            }

            TempData["ErrorMessage"] = "Questions Answers Not Deleted.";
            return RedirectToAction("QsAsTeacher");
        }

        public async Task<ActionResult> QsAsDetailsTeacher(int? id)
        {
            if (id != null)
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
                    Photo = data.Photo,
                    CuserId = data.CreatedBy
                };

                ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", data.PgmId);
                ViewBag.ClassId = new SelectList(db.Classes.Where(s => s.IsDeleted == 0), "Id", "Name", data.ClassId);
                ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == data.ClassId), "Id", "Name", data.CourseId);
                ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", data.SubjectId);
                ViewBag.ChapterId = new SelectList(db.Chapters.Where(p => p.IsDeleted == 0 && p.SubId == data.SubjectId), "Id", "Name", data.ChapterId);

                return View(dtpQA);
            }

            TempData["ErrorMessage"] = "Invalid Try";
            return RedirectToAction("QsAsTeacher");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> QsAsDetailsTeacher(QsAsViewModel dtpQAView, HttpPostedFileBase fileQus)
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
                        return RedirectToAction("TeacherQsAsList", new { Id = data.CreatedBy });
                    }
                }
            }

            ViewBag.PgmId = new SelectList(db.Programmes.Where(p => p.IsDeleted == 0), "Id", "Name", dtpQAView.PgmId);
            ViewBag.ClassId = new SelectList(db.Classes.Where(p => p.IsDeleted == 0), "Id", "Name", dtpQAView.ClassId);
            ViewBag.CourseId = new SelectList(db.Courses.Where(c => c.IsDeleted == 0 && c.ClassId == dtpQAView.ClassId), "Id", "Name", dtpQAView.CourseId);
            ViewBag.SubjectId = new SelectList(db.Subjects.Where(s => s.IsDeleted == 0), "Id", "Name", dtpQAView.SubjectId);
            ViewBag.ChapterId = new SelectList(db.Chapters.Where(p => p.IsDeleted == 0 && p.SubId == dtpQAView.SubjectId), "Id", "Name", dtpQAView.ChapterId);

            ViewBag.ErrorMessage = "Please fill in all the required fields";
            return View(dtpQAView);
        }

        public ActionResult ExamDetails(int? examId)
        {
            if (examId != null)
            {
                var data = db.GetAllExamById(examId).FirstOrDefault();
                return View(data);
            }

            TempData["ErrorMessage"] = "The Exam you are looking for could not be found.";
            return RedirectToAction("dashboard");
        }
    }
}