using Microsoft.AspNetCore.Identity;
using BackOfficeLibrary;

namespace BackOfficeManagement.Data
{
    public static class ApplicationSeed
    {
        public static async Task SeedDefaultUserAsync(
            UserManager<ApplicationUser> userManager,
           RoleManager<IdentityRole> roleManager
            )
        {
            try
            {
                if (!await roleManager.RoleExistsAsync("SysAdministrator"))
                {
                    Guid ConcurrencyStamp = Guid.NewGuid();
                    await roleManager.CreateAsync(new IdentityRole() { Name = "SysAdministrator", ConcurrencyStamp = ConcurrencyStamp.ToString() });
                }
                if (!await roleManager.RoleExistsAsync("Administrator"))
                {
                    Guid ConcurrencyStamp = Guid.NewGuid();
                    await roleManager.CreateAsync(new IdentityRole() { Name = "Administrator", ConcurrencyStamp = ConcurrencyStamp.ToString() });
                }
                if (!await roleManager.RoleExistsAsync("Manager"))
                {
                    Guid ConcurrencyStamp = Guid.NewGuid();
                    await roleManager.CreateAsync(new IdentityRole() { Name = "Manager", ConcurrencyStamp = ConcurrencyStamp.ToString() });
                }
                if (!await roleManager.RoleExistsAsync("Accounting"))
                {
                    Guid ConcurrencyStamp = Guid.NewGuid();
                    await roleManager.CreateAsync(new IdentityRole() { Name = "Accounting", ConcurrencyStamp = ConcurrencyStamp.ToString() });
                }
                if (!await roleManager.RoleExistsAsync("Teacher"))
                {
                    Guid ConcurrencyStamp = Guid.NewGuid();
                    await roleManager.CreateAsync(new IdentityRole() { Name = "Teacher", ConcurrencyStamp = ConcurrencyStamp.ToString() });
                }

                string FirstName = "System";
                string LastName = "Administrator";
                string UserName = "SystemAdmin";
                string Email = "systemadmin@gmail.com";

                var defaultSysAdminUser = new ApplicationUser
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    UserName = UserName,
                    Email = Email,
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true
                };

                var findUser = await userManager.FindByNameAsync("SystemAdmin");
                if (findUser is null)
                {
                    IdentityResult result = await userManager.CreateAsync(defaultSysAdminUser, "5WV3gxFHzJ_");
                    if (result.Succeeded)
                    {
                        defaultSysAdminUser = await userManager.FindByEmailAsync("systemadmin@gmail.com");
                        if (defaultSysAdminUser != null)
                        {
                            if (!await userManager.IsInRoleAsync(defaultSysAdminUser, "SysAdministrator"))
                            {
                                await userManager.AddToRoleAsync(defaultSysAdminUser, "SysAdministrator");
                            }
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static async Task SeedSampleDataAsync(ApplicationDbContext dbContext)
        {
            try
            {
                #region "Student Status"
                if (dbContext.StudentStatusData.Count() == 0)
                {
                    List<StudentStatusDataModel> student_status = SeedStudentStatus();
                    await dbContext.StudentStatusData.AddRangeAsync(student_status);
                    await dbContext.SaveChangesAsync();
                }
                #endregion
            }
            catch (Exception ex)
            {

            }
        }


        #region Data Seeding
        #region "Student Status"
        private static List<StudentStatusDataModel> SeedStudentStatus()
        {
            List<StudentStatusDataModel> items = new List<StudentStatusDataModel>();

            return items;
        }
        #endregion
        #endregion
    }
}
