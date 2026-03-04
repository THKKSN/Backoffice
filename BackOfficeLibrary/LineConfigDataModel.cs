using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("line_config")]
    public class LineConfigDataModel
    {
        [Key]
        public int id { get; set; }

        public string? url_line_liff { get; set; }

        public string? url_push_message { get; set; }

        public string? line_liff_id { get; set; }

        public string? line_group_id { get; set; }

        public string? channel_id { get; set; }

        public string? channel_secret { get; set; }

        public string? channel_access_token { get; set; }

        public int? Payment_RouteNo { get; set; }

        public bool? IsActive { get; set; }
    }

    public class LineDonateViewModel
    {
        public string LineUserId { get; set; }
        public string Uuid { get; set; }

        public string Student_Id { get; set; }
        public string? Code { get; set; }
        public string? Prefix { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? NiickName { get; set; }
        public int? GradeLevel_Id { get; set; }
        public int? GradeLevel_History_Id { get; set; }
        public string GradeLevel_Name { get; set; }
        public int? Room_Id { get; set; }
        public string Room_Name { get; set; }
        public int? GradeLevelTeacher_Id { get; set; }
        public string? GradeLevelTeacher_Name { get; set; }
        public int? AssistantTeacher_Id { get; set; }
        public string? AssistantTeacher_Name { get; set; }
        public string? IDCard { get; set; }

        public string? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public string? Address_desc { get; set; }

        public int? District_Id { get; set; }
        public string? District { get; set; }

        public int? Subdistrict_Id { get; set; }
        public string? Subdistrict { get; set; }

        public int? Province_Id { get; set; }
        public string? Province { get; set; }

        public int? ZipCode_Id { get; set; }
        public string? ZipCode { get; set; }

        public string? Latitude { get; set; }

        public string? Longitude { get; set; }

        public int? Relationship_Id { get; set; }

        public string? ContactFirstName { get; set; }

        public string? ContactLastName { get; set; }


        public int? Status { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }

        public string? Donate_Name { get; set; }

        public DateTime? DonateDate { get; set; }
        public string? DonateTime { get; set; } // HH:mm

        public string? EvidenceImage { get; set; }
        public decimal? DonateAmount { get; set; }
        public int? School_Year_Id { get; set; }
        public int? Term_Id { get; set; }
        public string? Term { get; set; }

        public string? School_Year { get; set; }
        public decimal? RequiredAmount { get; set; }
        public string? SearchText { get; set; }

    }

    public class DonateViewFromModel
    {

        public int? Id { get; set; }
        public string Student_Id { get; set; }
        public string? Code { get; set; }
        public string? Prefix { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? NiickName { get; set; }
        public int? GradeLevel_Id { get; set; }
        public int? GradeLevel_History_Id { get; set; }
        public string GradeLevel_Name { get; set; }
        public int? Room_Id { get; set; }
        public string Room_Name { get; set; }
        public int? GradeLevelTeacher_Id { get; set; }
        public string? GradeLevelTeacher_Name { get; set; }
        public int? AssistantTeacher_Id { get; set; }
        public string? AssistantTeacher_Name { get; set; }
        public string? IDCard { get; set; }

        public string? DateOfBirth { get; set; }

        public string? Address { get; set; }

        public string? Address_desc { get; set; }

        public int? District_Id { get; set; }
        public string? District { get; set; }

        public int? Subdistrict_Id { get; set; }
        public string? Subdistrict { get; set; }

        public int? Province_Id { get; set; }
        public string? Province { get; set; }

        public int? ZipCode_Id { get; set; }
        public string? ZipCode { get; set; }

        public string? Latitude { get; set; }

        public string? Longitude { get; set; }

        public int? Relationship_Id { get; set; }

        public string? ContactFirstName { get; set; }

        public string? ContactLastName { get; set; }

        public int? Status { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }

        public string? Donate_Name { get; set; }

        public DateTime? DonateDate { get; set; }

        public string? EvidenceImage { get; set; }
        public decimal? DonateAmount { get; set; }
        public int? School_Year_Id { get; set; }
        public int? Term_Id { get; set; }
        public string? Term { get; set; }

        public string? School_Year { get; set; }
        public decimal? RequiredAmount { get; set; }
        public string? SearchText { get; set; }
        public int? Teacher_Id { get; set; }
        public string? TeacherName { get; set; }



    }

    public class StudentSearchResultViewModel
    {
        public Guid? IdGuid { get; set; }  // สำหรับ internal
        public string Id { get; set; }
        public string Code { get; set; }
        public string Prefix { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string NickName { get; set; }
        public string IDCard { get; set; }
        public string GradeLevel { get; set; }
        public string Room { get; set; }
    }

    public class YearTermViewModel
    {
        public int SchoolYearId { get; set; }
        public string SchoolYearName { get; set; }
        public List<TermItem> Terms { get; set; }
    }

    public class TermItem
    {
        public int TermId { get; set; }
        public string TermName { get; set; }
    }

    public class DailyDonateSummaryDto
    {
        public DateTime Date { get; set; }
        public int TotalTransaction { get; set; }
        public decimal TotalAmount { get; set; }

        public decimal LineAmount { get; set; }
        public decimal WalkInAmount { get; set; }

        public int StudentCount { get; set; }
        public int TeacherCount { get; set; }
    }

}
