using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackOfficeLibrary
{
    public class DonateViweModel
    {
        public int Id { get; set; }

        public string Student_Id { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public decimal Donate_Term_1 { get; set; } = 0;

        public decimal Limit_Donate_Term_1 { get; set; }

        public decimal Donate_Term_2 { get; set; } = 0;

        public decimal Limit_Donate_Term_2 { get; set; }

        public decimal Total { get; set; } = 0;

        public decimal Outstanding_Balance { get; set; } = 0;
    }

    public class DonateViweDataTableModel
    {
        public int Id { get; set; }
        public DateTime DonateDate { get; set; }
        public string UUID { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string GradeLevel { get; set; }
        public string Term { get; set; }
        public string StudyYear { get; set; }
        public string DonateChannel { get; set; }
        public decimal Amount { get; set; } = 0;
    }


    public class DonateInfoTableModel
    {
        public int Id { get; set; }
        public DateTime DonateDate { get; set; }
        public string Code { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string GradeLevel { get; set; }
        public string Term { get; set; }
        public string StudyYear { get; set; }
        public string DonateChannel { get; set; }
        public decimal Amount { get; set; } = 0;
    }

    public class DonateInfoViewModel
    {
        public int Id { get; set; }
        public string UUID { get; set; }
        public string Code { get; set; }
        public string Prefix { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName { get; set; }
        public string GradeLevel { get; set; }
        public string StartDate { get; set; }


    }
    public class OutstandingDonationViewModel
    {
        public string StudentYear { get; set; }
        public string Term { get; set; }
        public int Term_Id { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal DonateAmount { get; set; }
        public string Grade_Level { get; set; }
        public string Room { get; set; }


    }

    public class OutstandingsByUuidVM
    {
        public DonateInfoViewModel Student { get; set; }
        public List<OutstandingDonationViewModel> Outstandings { get; set; }
    }

    public class DonateDetailsViewModel
    {
        public int Id { get; set; }

        public int SchoolYear_Id { get; set; }
        public string SchoolYear { get; set; }

        public int Term_Id { get; set; }
        public string Term { get; set; }

        public int Donate_Id { get; set; }

        public int GradeLevel_Id { get; set; }
        public string GradeLevel { get; set; }
        public string? Code { get; set; }
        public string? FirstName { get; set; }
        public string? FullName { get; set; }
        public string? LastName { get; set; }
        public decimal? Donate { get; set; }

        public string? Donate_Name { get; set; }


        public DateTime? DonateDate { get; set; }

        public string? EvidenceImage { get; set; }

        public int? Status { get; set; }

        public int? Channel_Id { get; set; }
        public string? Channel_Name { get; set; }
        public int? Teacher_Id { get; set; }
        public string? Teacher_Name { get; set; }
        public string CreatedBy { get; set; }
    }

    public class DailySummaryDto
    {
        public DateTime Date { get; set; }
        public decimal TodayDonation { get; set; }
        public int TodayDonatorCount { get; set; }
    }
    public class AnnualSummaryDto
    {
        public string SchoolYearName { get; set; }

        public int? TotalStudent { get; set; }
        public int? TotalStudentDonate { get; set; }
        public int? TotalStudentNotDonate { get; set; }

        public decimal? TotalDonationYear { get; set; }
        public decimal? TotalExpectedAmount { get; set; }
        public decimal TotalOutstandingAmount { get; set; }
    }

}
