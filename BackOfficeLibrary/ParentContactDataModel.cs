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
    [Table("parent_contact")]
    public class ParentContactDataModel
    {
        [Key]
        public int Id { get; set; }
        public string? Student_Id { get; set; }
        public int? Prefix_Id { get; set; }
        public int? Relationship_Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? IDCard { get; set; }
        public string? DateOfBirth { get; set; }
        public int? Religion_Id { get; set; }
        public string? Nationality { get; set; }
        public string? Occupation { get; set; }
        public int? Status { get; set; }
        public bool? IsActive { get; set; }
        public string? CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
    }

    [Table("parent_status")]
    public class ParentStatusDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? NameEng { get; set; }

    }
}
