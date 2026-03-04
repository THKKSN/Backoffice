using System;

namespace BackOfficeLibrary
{
    public class StudentExcelModel
    {
        public string IDCard { get; set; }
        public string Code { get; set; }
        public int GradeLevel { get; set; }
        public int Room_Id { get; set; }
        public int PrefixId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string DateOfBirth { get; set; }
        public int GenderId { get; set; }
        public string Nationality { get; set; }
        public int ReligionId { get; set; }
        public int Status { get; set; }
        public string HouseRegNo { get; set; }
        public string Address { get; set; }
        public string Moo { get; set; }
        public string Soi { get; set; }
        public string Road { get; set; }
        public DateOnly StartDate { get; set; }
        public int? StartTrem { get; set; }
        public decimal? Heigh { get; set; }
        public decimal? Weight { get; set; }
        public PreviousSchoolExcelModel PreviousSchool { get; set; }
        public List<ParentExcelModel> Parents { get; set; } = new List<ParentExcelModel>();
    }

    public class PreviousSchoolExcelModel
    {
        public string SchoolName { get; set; }
    }

    public class ParentExcelModel
    {
        public int? Prefix { get; set; }
        public string? IDCard { get; set; }
        public string? ContactFirstName { get; set; }
        public string? ContactLastName { get; set; }
        public int? Status { get; set; }
        public string? Nationality { get; set; }
        public int? RelationshipId { get; set; }
    }



    public class StudentExcelHistoryModel
    {
        public string ID { get; set; }
        public int GradeLevel { get; set; }
        public int Room_Id { get; set; }
        public int? Grade_Start { get; set; }
        public DateOnly StartDate { get; set; }
    }


}
