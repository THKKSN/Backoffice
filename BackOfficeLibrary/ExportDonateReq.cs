
using System;

namespace BackOfficeLibrary
{
    public class ExportDonateReq
    {
        public List<int> SchoolYearIds { get; set; }   // [1,2,3]
        public int? GradeLevelId { get; set; }
    }
}