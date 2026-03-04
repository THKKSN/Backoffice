using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("student")]
    public class StudentDataModel
    {
        [Key]
        [MaxLength(255)]
        public Guid? Id { get; set; }

        public string? Code { get; set; }
        public int? Prefix_Id { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? NickName { get; set; }

        public decimal? Height { get; set; }

        public decimal? Weight { get; set; }

        public int? GradeLevelTeacher_Id { get; set; }

        public int? AssistantTeacher_Id { get; set; }

        public string? IDCard { get; set; }

        public string? DateOfBirth { get; set; }

        public int? DisabilityType_Id { get; set; }

        public int? Religion_Id { get; set; }

        public string? Nationality { get; set; }

        public string? HouseRegNo { get; set; }

        public string? Address { get; set; }

        public string? Group { get; set; }

        public string? Address_desc { get; set; }

        public string? Soi { get; set; }
        
        public string? Road { get; set; }

        public int? District_Id { get; set; }

        public int? Subdistrict_Id { get; set; }

        public int? Province_Id { get; set; }

        public int? ZipCode_Id { get; set; }

        public string? Latitude { get; set; }

        public string? Longitude { get; set; }
        
        public string? AttachFile { get; set; }

        public int? Status { get; set; }

        public bool? IsActive { get; set; }

        public DateOnly? Start_Date { get; set; }

        public DateOnly? End_Date { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy {  get; set; }

        public DateTime? UpdateDate { get; set; }
    }

    [Table("prefix")]
    public class PrefixDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Abbreviation { get; set; }
    }

    [Table("religion")]
    public class ReligionDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? NameEn { get; set; }
    }
}
