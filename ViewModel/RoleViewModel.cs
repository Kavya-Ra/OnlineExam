using OnlineExam.DbContext;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineExam.ViewModel
{
    public class RoleViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string RoleName { get; set; }

        public int? RoleId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public int IsActive { get; set; }
        public int IsDeleted { get; set; }
        public int DeletedBy { get; set; }
        public DateTime DeletedDate { get; set; }

        public int CuserId { get; set; }

        public List<Role> Roles { get; set; }
    }
}