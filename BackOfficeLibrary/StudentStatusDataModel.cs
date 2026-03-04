using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("student_status")]
    public class StudentStatusDataModel
    {
        [Key]
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? NameEng {  get; set; }

        public bool? IsActive { get; set; }
    }
}
