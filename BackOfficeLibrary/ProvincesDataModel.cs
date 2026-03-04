using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("provinces")]
    public class ProvincesDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? NameEn { get; set; }
    }

    [Table("district")]
    public class DistrictDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? NameEn { get; set; }

        public int Province_Id { get; set; }
    }

    [Table("subdistrict")]
    public class SubDistrictDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public string? NameEn { get; set; }

        public int District_Id { get; set; }

        public int Province_Id { get; set; }
    }

    [Table("zip_code")]
    public class ZipCodeDataModel
    {
        [Key]
        public int Id { get; set; }

        public string? Code { get; set; }

        public string? Name { get; set; }

        public int Province_Id { get; set; }

        public int District_Id { get; set; }

        public int Subdistrict_Id { get; set; }
    }
}
