using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using BackOfficeLibrary;

namespace BackOfficeManagement.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) :
            base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationUser>(b =>
            {
                b.ToTable("users");
            });
            modelBuilder.Entity<IdentityUserClaim<string>>(b =>
            {
                b.ToTable("userclaims");
            });
            modelBuilder.Entity<IdentityUserLogin<string>>(b =>
            {
                b.ToTable("userlogins");
            });
            modelBuilder.Entity<IdentityUserToken<string>>(b =>
            {
                b.ToTable("usertokens");
            });
            modelBuilder.Entity<IdentityRole>(b =>
            {
                b.ToTable("roles");
            });
            modelBuilder.Entity<IdentityRoleClaim<string>>(b =>
            {
                b.ToTable("roleclaims");
            });
            modelBuilder.Entity<IdentityUserRole<string>>(b =>
            {
                b.ToTable("userroles");
            });
        }

        public DbSet<ProvincesDataModel> ProvincesData { get; set; }

        public DbSet<DistrictDataModel> DistrictData { get; set; }

        public DbSet<SubDistrictDataModel> SubDistrictData { get; set; }

        public DbSet<ZipCodeDataModel> ZipCodeData { get; set; }

        public DbSet<DonateDataModel> DonateData { get; set; }

        public DbSet<DonateChannelDataModel> DonateChannelData { get; set; }

        public DbSet<DonateDetailsDataModel> DonateDetailsData { get; set; }

        public DbSet<DonateSettingDataModel> DonateSettingData { get; set; }

        public DbSet<GradeLevelDataModel> GradeLevelData { get; set; }

        public DbSet<RoomDataModel> RoomData { get; set; }

        public DbSet<LearningAreaDataModel> LearningAreaData { get; set; }

        public DbSet<DepartmentDataModel> DepartmentData { get; set; }

        public DbSet<SubjectDataModel> SubjectData { get; set; }

        public DbSet<RelationshipDataModel> RelationshipData { get; set; }

        public DbSet<SchoolYearDataModel> SchoolYearData { get; set; }

        public DbSet<TermDataModel> TermData { get; set; }

        public DbSet<PrefixDataModel> PrefixData { get; set; }

        public DbSet<ReligionDataModel> ReligionData { get; set; }

        public DbSet<StudentDataModel> StudentData { get; set; }

        public DbSet<StudentHistoryClassDataModel> StudentHistoryClassData { get; set; }

        public DbSet<StudentStatusDataModel> StudentStatusData { get; set; }

        public DbSet<TeacherDataModel> TeacherData { get; set; }

        public DbSet<LineRegistrationModel> LineRegistration { get; set; }

        public DbSet<LineConfigDataModel> LineConfig { get; set; }

        public DbSet<ParentContactDataModel> ParentContactData { get; set; }

        public DbSet<ParentStatusDataModel> ParentStatusData { get; set; }

        public DbSet<DisabilityTypeDataModel> DisabilityTypeData { get; set; }

        public DbSet<PreviousSchoolDataModel> PreviousSchoolData { get; set; }

        public DbSet<OverdueDataModel> OverdueData { get; set; }

        public DbSet<OverdueStatusDataModel> OverdueStatusData { get; set; }

        public DbSet<DonateSettingIndividualDataModel> DonateSettingIndividualData { get; set; }

    }
}
