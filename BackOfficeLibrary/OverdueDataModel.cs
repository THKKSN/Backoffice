using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("overdue")]
    public class OverdueDataModel
    {
        [Key]
        public int Id { get; set; }

        public int? SchoolYear_Id { get; set; }

        public string? Student_Id { get; set; }

        public string? Description { get; set; }

        public string? OverDueDate { get; set; }

        public int? Status { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }

    [Table("overdue_status")]
    public class OverdueStatusDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? NameEn { get; set; }
    }

    public class OverdueViewModel
    {
        public int? Id { get; set; }

        public int? SchoolYear_Id { get; set; }

        public string? SchoolYear { get; set; }

        public string? Student_Id { get; set; }

        public string? Code { get; set; }

        public string? StudentName { get; set; }

        public string? GradeLevel { get; set; }

        public string? Description { get; set; }

        public string? OverDueDate { get; set; }

        public int? Status { get; set; }

        public string? StatusName { get; set; }

        public decimal? TotalAmount { get; set; }   // DonateSetting
        public decimal? PaidAmount { get; set; }    // SUM DonateDetail
        public decimal? Balance { get; set; }       // คงเหลือ
    }

    public class OverdueDTOModel
    {
        public int Id { get; set; }

        public int? SchoolYear_Id { get; set; }

        public string? Student_Id { get; set; }
        public string? Student_Name { get; set; }

        public string? Description { get; set; }

        public string? OverDueDate { get; set; }

        public int? Status { get; set; }
        public int? GradeLevel_Id { get; set; }
        
    }
}
