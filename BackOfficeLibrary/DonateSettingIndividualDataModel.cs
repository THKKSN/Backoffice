using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("donate_setting_individual")]
    public class DonateSettingIndividualDataModel
    {
        [Key]
        public int Id { get; set; }

        public string Student_Id { get; set; }

        public int? SchoolYear_Id { get; set; }

        public int? Term_Id { get; set; }

        public int? GradeLevel_Id { get; set; }

        public decimal? OverrideAmount { get; set; }

        public bool? IsActive { get; set; }

        public string? Remark { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? UpdateDate { get; set; }

        public string? UpdateBy { get; set; }

    }
}
