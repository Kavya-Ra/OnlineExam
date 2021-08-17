using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class StudentRegistrationViewModel
    {
        public StudentRegistrationViewModel()
        {
            ApplnDate = Convert.ToString(DateTime.Now.Date);
        }

        public string RegId { get; set; }        
        [Required][Display(Name = "Year")]
        public string ExamAttendingYear { get; set; }
        [Required][Display(Name = "Preferred Day")]
        public string PreferredDay { get; set; }
        [Required][Display(Name = "Application Date")]
        public string ApplnDate { get; set; }
        [Required][Display(Name = "Academic Year")]
        public string AcademicYear { get; set; }
        [Required][Display(Name = "Admission Test Date")]
        public string AdmissionTestDate { get; set; }
        [Required][Display(Name = "Preferred Time")]
        public string PreferredTime { get; set; }
        [Required][Display(Name = "Student Name")]
        public string StudentName { get; set; }
        [Required][Display(Name = "WhatsappNo")]
        public string WhatsappNo { get; set; }
        [Required][Display(Name = "DOB")]
        public string DOB { get; set; }
        [Required][Display(Name = "Gender")]
        public string Gender { get; set; }
        [Required][Display(Name = "Religion")]
        public string Religion { get; set; }

        [Required][Display(Name = "Caste")]
        public string Caste { get; set; }
        [Required][Display(Name = "Community")]
        public string Community { get; set; }
        [Required][Display(Name = "BloodGroup")]
        public string BloodGroup { get; set; }
        [Required][Display(Name = "Nationalty")]
        public string Nationalty { get; set; }
        [Required][Display(Name = "Present Address")]
        public string PresentAddress { get; set; }
        [Required][Display(Name = "Area")]
        public string Area { get; set; }
        [Required][Display(Name = "Location")]
        public string Location { get; set; }
        [Required][Display(Name = "State")]
        public string State { get; set; }
        [Required][Display(Name = "District")]
        public string District { get; set; }
        [Required][Display(Name = "Pincode")]
        public string Pincode { get; set; }
        [Required][Display(Name = "Quick ContactNo")]
        public string QuickContNo { get; set; }
        [Display(Name = "Photo")]
        public string Photo { get; set; }
        [Required][Display(Name = "Quick WhatsaAppNo")]
        public string QuickWhatsApp { get; set; }
        [Required][Display(Name = "Program")]
        public int PgmId { get; set; }
        [Required][Display(Name = "Class")]
        public int ClassId { get; set; }
        [Required][Display(Name = "Course")]
        public int CourseId { get; set; }
        [Required][Display(Name = "Sub Program")]
        public int SubPgmId { get; set; }

        //homedetails//
        [Required][Display(Name = "Address1")]
        public string homeAddress { get; set; }
        [Display(Name = "Address2")]
        public string Address2 { get; set; }
        [Required][Display(Name = "Home Area")]
        public string AreaHome { get; set; }
        [Required][Display(Name = "Pincode")]
        public string PincodeHome { get; set; }
        [Required][Display(Name = "Quick ContactNo")]
        public string homeContact { get; set; }
        [Required][Display(Name = "Location")]
        public string LocationHome { get; set; }
        [Required][Display(Name = "State")]
        public string StateHome { get; set; }
        [Required][Display(Name = "Email Id")]
        public string emailHome { get; set; }

        [Required][Display(Name = "Quick WhatsappNo")]
        public string QuickHomeWhatsapp { get; set; }
        [Required]
        [Display(Name = "District")]
        public string DistHome { get; set; }

        //Parentdetails//

        [Required][Display(Name = "Father Name")]
        public string FrName { get; set; }
        [Required][Display(Name = "Father Occupation")]
        public string FrOcc { get; set; }
        [Required][Display(Name = "Father MobileNo")]
        public string FrMobNo { get; set; }
        [Required][Display(Name = "Father MailId")]
        public string MailidFr { get; set; }
        [Required][Display(Name = "Father District")]
        public string FrDistrict { get; set; }
        [Display(Name = "Father Sign")]
        public string FrSign { get; set; }
        [Required][Display(Name = "Father State")]
        public string FrState { get; set; }
        [Required][Display(Name = "Father WhatsAppNo")]
        public string WhatsappFr { get; set; }
        [Required][Display(Name = "Mother Name")]
        public string MrName { get; set; }
        [Required][Display(Name = "Mother Occupation")]
        public string MrOcc { get; set; }
        [Required][Display(Name = "Mother MobileNo")]
        public string MrMobNo { get; set; }
        [Required][Display(Name = "Mother MailId")]
        public string MailidMr { get; set; }
        [Required][Display(Name = "Mother District")]
        public string MrDistrict { get; set; }
        [Display(Name = "Mother Sign")]
        public string MrSign { get; set; }
        [Required][Display(Name = "Mother State")]
        public string MrState { get; set; }
        [Required][Display(Name = "Mother WhatsAppNo")]
        public string WhatsappMr { get; set; }
    }
}