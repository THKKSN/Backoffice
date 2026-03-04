using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("learning_area")]
    public class LearningAreaDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? NameEng {  get; set; }

        public string? Description { get; set; }

        public string? DescriptionEng { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }

    [Table("department")]
    public class DepartmentDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? NameEng { get; set; }

        public string? Description { get; set; }

        public string? DescriptionEng { get; set; }

        public int? LearningArea_Id { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }

    [Table("subject")]
    public class SubjectDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? NameEng { get; set; }

        public string? Description { get; set; }

        public string? DescriptionEng { get; set; }

        public int? LearningArea_Id { get; set; }

        public int? Department_Id { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
