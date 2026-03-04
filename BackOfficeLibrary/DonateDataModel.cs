using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("donate")]
    public class DonateDataModel
    {
        [Key]
        public int Id { get; set; }

        public int? School_Year_Id { get; set; }

        public string? Student_Id { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }
    }

    [Table("donate_details")]
    public class DonateDetailsDataModel
    {
        [Key]
        public int Id { get; set; }

        public int SchoolYear_Id { get; set; }

        public int Term_Id { get; set; }

        public int Donate_Id { get; set; }

        public int GradeLevel_Id { get; set; }

        public decimal? Donate { get; set; }

        public string? Donate_Name { get; set; }

        public DateTime? DonateDate { get; set; }

        public string? EvidenceImage { get; set; }

        public int? Status { get; set; }

        public int? Channel_Id { get; set; }
        public int? Teacher_Id { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public bool? SendLine { get; set; }

        public DateTime? SendLineDate { get; set; }
    }

    [Table("donate_channel")]
    public class DonateChannelDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Donate_channel { get; set; }

        public bool? IsActive { get; set; }


    }
}
