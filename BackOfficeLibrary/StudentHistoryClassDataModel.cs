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
    [Table("student_history_class")]
    public class StudentHistoryClassDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Student_Id { get; set; }

        public int? School_Year_Id { get; set; }

        public int? Term_Id { get; set; }
        
        public int? Grade_Level_Id { get; set; }
        
        public int? Room_Id { get; set; }

        public bool? IsCurrent { get; set; }

    }
}
