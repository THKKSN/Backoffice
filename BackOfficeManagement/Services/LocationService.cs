using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using BackOfficeManagement.Data;
using BackOfficeLibrary;
using System.Globalization;

namespace BackOfficeManagement.Services
{
    public class LocationService 
    {
        private ILogger<LocationService> _logger;
        private  ApplicationDbContext _context;

        public LocationService(
            ILogger<LocationService> logger,
            ApplicationDbContext context
            )
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<ProvincesDataModel>> GetProvinceAsync()
        {
            try
            {
                var province = await _context.ProvincesData
                .Select(p => new ProvincesDataModel { Id = p.Id, Name = p.Name }).ToListAsync();


                if (province.Any())
                {
                    return province;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }

            return new List<ProvincesDataModel>();
        }

        public async Task<List<DistrictDataModel>> GetDistactAsync(int provinceId)
        {
            return await _context.DistrictData
                .Where(d => d.Province_Id == provinceId)
                .ToListAsync();
        }

        public async Task<List<SubDistrictDataModel>> GetSubDistactAsync(int districtId)
        {
            return await _context.SubDistrictData
                .Where(s => s.District_Id == districtId)
                .ToListAsync();
        }

        public async Task<string?> GetZipCodeAsync(int subDistrictId)
        {
            var zip = await _context.ZipCodeData
                .FirstOrDefaultAsync(z => z.Subdistrict_Id == subDistrictId);

            return zip?.Code;
        }

    }
}
