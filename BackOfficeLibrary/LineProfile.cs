using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    public class LineProfile
    {
        public string LineUserId { get; set; }

        public string? DisplayName { get; set; }

        public string? PictureUrl { get; set; }
    }
}
