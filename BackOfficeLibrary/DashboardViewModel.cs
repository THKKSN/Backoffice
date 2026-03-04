using System;

namespace BackOfficeLibrary
{
    public class SchoolYearOption
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    public class DashboardViewModel
    {
        public int SelectedSchoolYear { get; set; }
        public List<SchoolYearOption> SchoolYears { get; set; }
        public int TotalStudent { get; set; }
        public int TotalStudentDonate { get; set; }
        public int TotalStudentNotDonate { get; set; }
        public decimal TotalDonationYear { get; set; }
        public decimal Term1Donation { get; set; }
        public decimal Term2Donation { get; set; }

        public int Year { get; set; }
        public string School_Year { get; set; }
        public decimal? TotalOutstandingAmount { get; set; }
        public decimal? TotalExpectedAmount { get; set; }
    }
    public class DonutChartVM
    {
        public int TotalStudents1 { get; set; }
        public int TotalStudents2 { get; set; }

        // จำนวนคน
        public int Term1Paid { get; set; }
        public int Term1Unpaid { get; set; }

        public int Term2Paid { get; set; }
        public int Term2Unpaid { get; set; }

        // เกณฑ์ต่อคน
        public decimal Term1ExpectedAmount { get; set; }
        public decimal Term2ExpectedAmount { get; set; }

        // เงินรวมที่ควรได้
        public decimal Term1TotalExpected => TotalStudents1 * Term1ExpectedAmount;
        public decimal Term2TotalExpected => TotalStudents2 * Term2ExpectedAmount;

        // เงินที่รับจริง
        public decimal? Term1TotalPaid { get; set; }
        public decimal? Term2TotalPaid { get; set; }

        // % (ใช้โชว์กลาง Donut)
        public decimal Term1Percent =>
            TotalStudents1 == 0 ? 0m : Math.Round((decimal)Term1Paid * 100 / TotalStudents1, 2);

        public decimal Term2Percent =>
            Term2TotalExpected == 0 ? 0m : Math.Round((decimal)Term2Paid * 100 / TotalStudents2, 2);
    }


    public class GradeLevelOption
    {
        public int Value { get; set; }
        public string Text { get; set; }
    }

    public class TopDonateVM
    {
        public string StudentId { get; set; }
        public string Code { get; set; }
        public string StudentName { get; set; }
        public string GradeLevel_Room { get; set; }
        public decimal? TotalDonate { get; set; }
    }

    public class TopNotDonateVM
    {
        public string StudentId { get; set; }
        public string Code { get; set; }
        public string StudentName { get; set; }
        public string GradeLevel_Room { get; set; }
        public decimal? TotalDonate { get; set; }
        public decimal? Paid { get; set; }
        public decimal? Required { get; set; }
        public decimal? Remaining { get; set; }
    }

    public class StudentDonationSummaryVM
    {
        public string StudentId { get; set; }
        public string Code { get; set; }
        public string StudentName { get; set; }
        public string GradeLevelRoom { get; set; }

        public decimal? Required { get; set; }
        public decimal? Paid { get; set; }
        public decimal? Remaining { get; set; }
    }

    public class RoomDonateViewModel
    {
        public int SchoolYearId { get; set; }
        public int? GradeLevelId { get; set; }
        public int RoomId { get; set; }
        public int TermId { get; set; }
    }


}
