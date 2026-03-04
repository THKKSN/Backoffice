using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("teacher")]
    public class TeacherDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Code { get; set; }

        public int? Prefix_Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? NickName { get; set; }

        public int? LearningArea_Id { get; set; }

        public int? Department_Id { get; set; }

        public int? Subject_Id { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
