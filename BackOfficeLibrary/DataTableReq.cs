using System;

namespace BackOfficeLibrary
{

    public class DataTableReq
    {
        public int? Draw { get; set; }
        public int? Start { get; set; }
        public int? Length { get; set; }
        public string? Name { get; set; }
        public string? Search { get; set; }
        public string? SearchDate { get; set; }
        public string? StartDate { get; set; }
        public string? EndDate { get; set; }
        public string? SortColumn { get; set; }
        public string? SortOrder { get; set; }
        public string? FilterBy { get; set; }
        public string? schoolyear { get; set; }
        public string? uuid { get; set; }
        public int? schoolYearId { get; set; }
        public int? gradeLevelId { get; set; }
    }
}
