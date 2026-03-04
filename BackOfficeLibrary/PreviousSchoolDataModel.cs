using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("previousSchool")]
    public class PreviousSchoolDataModel
    {
        [Key]
        public int Id { get; set; }
        public string? Student_Id { get; set; }
        public string? SchoolName { get; set; }
        public int? SchoolProvince_Id { get; set; }
        public int? GradeLevel_Id { get; set; }
        public bool? IsActive { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
    }
}
