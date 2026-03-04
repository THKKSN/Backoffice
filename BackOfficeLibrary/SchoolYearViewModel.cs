using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    public class SchoolYearViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public int? Status { get; set; }
        public int Term_Id { get; set; }

        public string? Term_Name { get; set; }
        public bool? IsActive { get; set; }

        public string? CreateBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? UpdateBy { get; set; }

        public DateTime? UpdateDate { get; set; }
    }
}
