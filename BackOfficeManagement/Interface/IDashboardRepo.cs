using BackOfficeLibrary;

namespace BackOfficeManagement.Interface
{
    public interface IDashboardRepo
    {
        Task<List<TopNotDonateVM>> GetTopNotDonateAsync(DataTableReq input);
        Task<List<TopDonateVM>> GetTopDonateAsync(DataTableReq input);

        Task<List<StudentDonationSummaryVM>> GetStudentDonationSummaryAsync(DataTableReq input);
    }
}