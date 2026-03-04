using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("donate_setting")]
    public class DonateSettingDataModel
    {
        [Key]
        public int Id { get; set; }

        public decimal? Donate { get; set; }

        public int? SchoolYear_Id { get; set; }

        public int? Term_Id { get; set; }

        public int? Grade_Level_Id { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }

    public class DonateSettingViewwModel
    {
        [Key]
        public int Id { get; set; }

        public decimal? Donate { get; set; }

        public int? SchoolYear_Id { get; set; }
        public string? SchoolYear { get; set; }

        public int? Term_Id { get; set; }
        public string? Term { get; set; }

        public int? Grade_Level_Id { get; set; }
        public string? Grade_Level { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
        public List<SchoolYearTermViewModel> SchoolYears { get; set; } = new();

    }

    public class DonateIndividualViewModel
    {
        [Key]
        public int Id { get; set; }
        public string? Student_Id { get; set; }
        public string? Student_Name { get; set; }

        public decimal? OverrideAmount { get; set; }

        public int? SchoolYear_Id { get; set; }
        public string? SchoolYear { get; set; }

        public int? Term_Id { get; set; }
        public string? Term { get; set; }

        public int? GradeLevel_Id { get; set; }
        public string? Grade_Level { get; set; }

        public bool? IsActive { get; set; }

        public string? Remark { get; set; }
        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
        public List<SchoolYearTermViewModel> SchoolYears { get; set; } = new();

    }

    public class SchoolYearTermViewModel
    {
        public int SchoolYearId { get; set; }
        public string SchoolYear { get; set; }
        public List<TermViewModel> Terms { get; set; } = new();
    }

    public class TermViewModel
    {
        public int TermId { get; set; }
        public string Term { get; set; }
        public bool? IsActive { get; set; }
        public int? SchoolYearId { get; set; }
    }
    public class DonateSettingsFilterModel
    {
        public string SchoolYear { get; set; }
    }
}
