using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    public class StudantViewModel
    {
        public string? Id { get; set; }

        public string? Student_Id { get; set; }

        public string? FirstName { get; set; }
        public string? Student_Name { get; set; }

        public string? LastName { get; set; }
        public string? NickName { get; set; }

        public string? Grade_Level_Name { get; set; }

        public string? Room_Name { get; set; }

        public int? Student_Status_Id { get; set; }
        public string? Religion { get; set; }
        public string? Nationality { get; set; }

        public string? Student_Status_Name { get; set; }
        public string? AttachFile { get; set; }
        public string? GradeLevel_Room { get; set; }
        public string? IDCard { get; set; }
        public string? DateOfBirth { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? StartDate { get; set; }
        public int? Age { get; set; }
        public bool? IsActive { get; set; }
        public int? Status { get; set; }
        public string? DisabilityType { get; set; }
        public string? HouseRegNo { get; set; }
        public string? Address { get; set; }
        public string? Address_desc { get; set; }
        public string? Soi { get; set; }
        public string? Road { get; set; }
        public string? Provinces { get; set; }
        public string? Districts { get; set; }
        public string? Subdistricts { get; set; }
        public string? ZipCode { get; set; }
        public PreviousSchoolViewModel? PreviousSchool { get; set; }
        public List<ParentContactViewModel> Parents { get; set; } = new();
        public List<GradeHistoryViewModel> GradeHistories { get; set; }

    }

    public class PreviousSchoolViewModel
    {
        public string? SchoolName { get; set; }
        public int? SchoolProvince_Id { get; set; }
        public string? SchoolProvince { get; set; }
        public int? GradeLevel_Id { get; set; }
        public string? GradeLevel { get; set; }
    }

    public class ParentContactViewModel
    {
        public int? Id { get; set; }
        public int? PrefixParent_Id { get; set; }
        public string? PrefixParent { get; set; }
        public string? ContactFirstName { get; set; }
        public string? ContactLastName { get; set; }
        public string? IDCard { get; set; }
        public string? Occupation { get; set; }
        public string? ContactPhone { get; set; }
        public string? DateOfBirth { get; set; }
        public int? Age { get; set; }
        public int? Relationship_Id { get; set; }
        public string? Relationship { get; set; }
        public int? Religion_Id { get; set; }
        public string? Religion { get; set; }
        public int? ParentStatus_Id { get; set; }
        public string? ParentStatus { get; set; }
        public string? Nationality { get; set; }

    }
    public class UpgradeStudentRequest
    {
        public List<string> StudentIds { get; set; } = new();
        public int New_SchoolYear_Id { get; set; }
        public int NewTerm_Id { get; set; }
        public int NewGradeLevel_Id { get; set; }
        public int? NewRoom_Id { get; set; }
    }

    public class UpdateStudentStatusDTO
    {
        public List<Guid> StudentIds { get; set; } = new();
        public int? Status { get; set; }
    }



    public class StudentDTOModel
    {
        public string? Id { get; set; }
        public string? Code { get; set; }
        public int? Prefix_Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? NickName { get; set; }
        public int? Room_Id { get; set; }
        public int? School_Year_Id { get; set; }
        public DateOnly? StartDate { get; set; }
        public int? Term_Id { get; set; }
        public int? GradeLevel_Id { get; set; }
        public int? NewGradeLevel_Id { get; set; }
        public int? NewRoom_Id { get; set; }
        public int? GradeLevelTeacher_Id { get; set; }
        public int? AssistantTeacher_Id { get; set; }
        public string? IDCard { get; set; }
        public string? DateOfBirth { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public int? DisabilityType_Id { get; set; }
        public int? Religion_Id { get; set; }
        public string? Nationality { get; set; }
        public string? HouseRegNo { get; set; }
        public string? Address { get; set; }
        public string? Soi { get; set; }
        public string? Road { get; set; }
        public string? Address_desc { get; set; }
        public int? District_Id { get; set; }
        public int? Subdistrict_Id { get; set; }
        public int? Province_Id { get; set; }
        public int? ZipCode_Id { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? AttachFile { get; set; }
        public int? Relationship_Id { get; set; }
        public int? PrefixParent_Id { get; set; }
        public string? ContactFirstName { get; set; }
        public string? ContactLastName { get; set; }
        public string? ContactPhone { get; set; }
        public int? ParentStatus_Id { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }

        public PreviousSchoolViewModel? PreviousSchool { get; set; }
        public List<ParentContactViewModel> Parents { get; set; } = new();
    }
    public class GradeHistoryViewModel
    {
        public string GradeName { get; set; }
        public string RoomName { get; set; }
        public string Term { get; set; }
        public string SchoolYear { get; set; }
        public bool IsCurrent { get; set; }
    }
}
