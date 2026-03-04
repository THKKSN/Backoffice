using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("grade_level")]
    public class GradeLevelDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? NameEng { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }

    public class GradeLevelViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? NameEng { get; set; }
        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
        public int Room_Count { get; set; }

        public List<RoomViewModel> Rooms { get; set; } = new();

    }

    [Table("room")]
    public class RoomDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? NameEng { get; set; }

        public int Grade_Level_Id { get; set; }

        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }

    }

    public class RoomViewModel
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Code { get; set; }
        public bool? IsActive { get; set; }
    }

}
