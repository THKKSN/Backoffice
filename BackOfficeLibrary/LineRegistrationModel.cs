using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    [Table("line_user_registration")]
    public class LineRegistrationModel
    {
        [Key]
        public string UserId { get; set; }

        public string? LineUserId { get; set; }

        public string? DisplayName { get; set; }

        public string? PictureUrl { get; set; }

        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? PhoneNumber { get; set; }

        public string? Email { get; set; }

        public DateTime? RegisteredDate { get; set; }

        public DateTime? UpdatedDate { get; set; }

        public bool? IsAdmin { get; set; }

        //public string? Property_Id { get; set; }
    }
}
