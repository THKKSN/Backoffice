
using BackOfficeLibrary;

namespace BackOfficeManagement.Interface
{
    public interface ILineDailySummaryRepository
    {
        public Task<bool> SendDailySummary(string schoolYearName, DateTime date, string groupId);
        public Task<DailySummaryDto> GetDailySummary(int schoolYear, DateTime date);

    }
}
