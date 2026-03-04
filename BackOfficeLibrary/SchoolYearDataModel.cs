using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("school_year")]
    public class SchoolYearDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public int? Status { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }

    [Table("term")]
    public class TermDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public int? SchoolYear_Id { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
