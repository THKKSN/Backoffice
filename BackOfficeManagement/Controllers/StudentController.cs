using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using BackOfficeManagement.Data;
using BackOfficeManagement.Services;
using BackOfficeLibrary;
using System.Diagnostics;
using System.Globalization;
using BackOfficeManagement.Interface;
using OfficeOpenXml;

namespace BackOfficeManagement.Controllers
{
    [Authorize]
    public class StudentController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<StudentController> _logger;
        private readonly IFileService _fileService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly LanguageService _lang;
        private string currentLang = "";
        private CultureInfo thTHCulture = new CultureInfo("th-TH");

        public StudentController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            ILogger<StudentController> logger,

            IStringLocalizer<SharedResource> localizer,
            LanguageService lang,
            IFileService fileService
            )
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;

            _localizer = localizer;
            _lang = lang;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user is null || user.Id is null)
                {
                    ViewBag.ErrorMessage = $"User cannot be found!";
                    return View("NotFound");
                }

                var model = new List<StudantViewModel>();

                var result = await (from studentdata in _context.StudentData
                                    join studenthistory in _context.StudentHistoryClassData on studentdata.Id.ToString() equals studenthistory.Student_Id
                                    join gradeleveldata in _context.GradeLevelData on studenthistory.Grade_Level_Id equals gradeleveldata.Id
                                    join roomdata in _context.RoomData on studenthistory.Room_Id equals roomdata.Id
                                    join statusstudentdata in _context.StudentStatusData on studentdata.Status equals statusstudentdata.Id
                                    join prefix in _context.PrefixData on studentdata.Prefix_Id equals prefix.Id
                                    where studentdata.IsActive == true && studenthistory.IsCurrent == true
                                    select new
                                    {
                                        Id = studentdata.Id,
                                        Student_Id = studentdata.Code,
                                        FirstName = studentdata.FirstName,
                                        LastName = studentdata.LastName,
                                        GradeLevel_Name = gradeleveldata.Name,
                                        Room_Name = roomdata.Name,
                                        Student_Status_Id = studentdata.Status,
                                        Student_Status_Name = statusstudentdata.Name,
                                        Abbreviation = prefix.Abbreviation
                                    }).ToListAsync();


                if (result.Any())
                {
                    foreach (var item in result)
                    {
                        var fullName = $"{item.Abbreviation}{item.FirstName} {item.LastName}";

                        model.Add(new StudantViewModel()
                        {
                            Id = item.Id.ToString(),
                            Student_Id = item.Student_Id,
                            Student_Name = fullName,
                            Grade_Level_Name = item.GradeLevel_Name,
                            Room_Name = item.Room_Name,
                            Student_Status_Id = item.Student_Status_Id,
                            Student_Status_Name = item.Student_Status_Name,
                        });
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Update(string Id)
        {
            try
            {
                if (!Guid.TryParse(Id, out var studentId))
                {
                    ViewBag.ErrorMessage = "Invalid student Id";
                    return View("NotFound");
                }

                var _prevschool = await (from prevschool in _context.PreviousSchoolData
                                         where prevschool.Student_Id == Id && prevschool.IsActive == true
                                         select new PreviousSchoolViewModel
                                         {
                                             SchoolName = prevschool.SchoolName,
                                             SchoolProvince_Id = prevschool.SchoolProvince_Id,
                                             GradeLevel_Id = prevschool.GradeLevel_Id
                                         }).FirstOrDefaultAsync();

                var _parentList = await (from parent in _context.ParentContactData
                                         where parent.Student_Id == Id
                                         select new ParentContactViewModel
                                         {
                                             Id = parent.Id,
                                             PrefixParent_Id = parent.Prefix_Id,
                                             ContactFirstName = parent.FirstName,
                                             ContactLastName = parent.LastName,
                                             IDCard = parent.IDCard,
                                             Occupation = parent.Occupation,
                                             ContactPhone = parent.PhoneNumber,
                                             DateOfBirth = parent.DateOfBirth,
                                             Relationship_Id = parent.Relationship_Id,
                                             Religion_Id = parent.Religion_Id,
                                             ParentStatus_Id = parent.Status,
                                             Nationality = parent.Nationality
                                         }).ToListAsync();

                var _studentData = await (from student in _context.StudentData
                                          join historyclass in _context.StudentHistoryClassData on student.Id.ToString() equals historyclass.Student_Id
                                          where student.Id == studentId && historyclass.IsCurrent == true
                                          select new StudentDTOModel
                                          {
                                              Id = student.Id.ToString(),
                                              AttachFile = student.AttachFile,
                                              Code = student.Code,
                                              Prefix_Id = student.Prefix_Id,
                                              FirstName = student.FirstName,
                                              LastName = student.LastName,
                                              NickName = student.NickName,
                                              Height = student.Height,
                                              Weight = student.Weight,
                                              IDCard = student.IDCard,
                                              DateOfBirth = student.DateOfBirth,
                                              DisabilityType_Id = student.DisabilityType_Id,
                                              GradeLevel_Id = historyclass.Grade_Level_Id,
                                              Room_Id = historyclass.Room_Id,
                                              Nationality = student.Nationality,
                                              Religion_Id = student.Religion_Id,
                                              School_Year_Id = historyclass.School_Year_Id,
                                              Term_Id = historyclass.Term_Id,
                                              StartDate = student.Start_Date,
                                              HouseRegNo = student.HouseRegNo,
                                              Address = student.Address,
                                              Address_desc = student.Address_desc,
                                              Soi = student.Soi,
                                              Road = student.Road,
                                              Province_Id = student.Province_Id,
                                              District_Id = student.District_Id,
                                              Subdistrict_Id = student.Subdistrict_Id,
                                              ZipCode_Id = student.ZipCode_Id,
                                              PreviousSchool = _prevschool != null ? _prevschool : new PreviousSchoolViewModel(),
                                              Parents = _parentList.Any() ? _parentList : new List<ParentContactViewModel> { new ParentContactViewModel(), new ParentContactViewModel() }
                                          }).FirstOrDefaultAsync();
                await LoadDropdownsAsync();

                if (_studentData != null)
                {
                    if (_studentData.GradeLevel_Id != null)
                    {
                        var rooms = await _context.RoomData.Where(_ => _.Grade_Level_Id == _studentData.GradeLevel_Id).ToListAsync();
                        var _room = rooms.Select(x => new SelectListItem
                        {
                            Value = x.Id.ToString(),
                            Text = x.Name
                        }).ToList();
                        ViewBag.RoomList = _room;
                    }

                    if (_studentData.School_Year_Id != null)
                    {
                        var term = await _context.TermData.Where(_ => _.SchoolYear_Id == _studentData.School_Year_Id).ToListAsync();
                        var _term = term.Select(x => new SelectListItem
                        {
                            Value = x.Id.ToString(),
                            Text = x.Name
                        }).ToList();
                        ViewBag.TermList = _term;
                    }

                    if (_studentData.Province_Id != null)
                    {
                        var district = await _context.DistrictData.Where(_ => _.Province_Id == _studentData.Province_Id).ToListAsync();
                        var _district = district.Select(x => new SelectListItem
                        {
                            Value = x.Id.ToString(),
                            Text = x.Name
                        }).ToList();
                        ViewBag.District = _district;
                    }

                    if (_studentData.District_Id != null)
                    {
                        var subdistrict = await _context.SubDistrictData.Where(_ => _.District_Id == _studentData.District_Id).ToListAsync();
                        var _subdistrict = subdistrict.Select(x => new SelectListItem
                        {
                            Value = x.Id.ToString(),
                            Text = x.Name
                        }).ToList();
                        ViewBag.SubDistrict = _subdistrict;
                    }

                    if (_studentData.Subdistrict_Id != null)
                    {
                        var zipcode = await _context.ZipCodeData.Where(_ => _.Subdistrict_Id == _studentData.Subdistrict_Id).FirstOrDefaultAsync();
                        ViewBag.Zipcode = zipcode?.Name ?? "";
                    }
                }

                return PartialView("_Update", _studentData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return PartialView("NotFound");
            }
        }

        [HttpPost]
        public async Task<ResponsesModel> Delete(string Id)
        {
            ResponsesModel response = new ResponsesModel();
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user is null || user.Id is null)
                {
                    response.status = false;
                    response.message = "User cannot be found!";
                    return response;
                }


                var student = await _context.StudentData
                    .FirstOrDefaultAsync(x => x.Id.ToString() == Id);

                if (student is null)
                {
                    response.status = false;
                    response.message = "student cannot be found!";
                    return response;
                }


                student.UpdateDate = DateTime.UtcNow;
                student.UpdateBy = user.Id;
                student.IsActive = false;
                _context.StudentData.Update(student);
                await _context.SaveChangesAsync();

                response.status = true;
                response.message = "Success";
                return response;
            }
            catch (Exception ex)
            {
                response.status = false;
                response.message = "Error" + ex.ToString();
                return response;
            }
        }


        [HttpPost]
        public async Task<IActionResult> Update(StudentDTOModel model, IFormFile? StudentFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDropdownsAsync();
                    return View(model);
                }

                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user is null)
                {
                    ViewBag.ErrorMessage = "User not found!";
                    return View("NotFound");
                }

                var student = await _context.StudentData
                    .FirstOrDefaultAsync(x => x.Id.ToString() == model.Id);

                if (student is null)
                    return View("NotFound");

                string? StudentFileName = student.AttachFile; // เก็บไฟล์เดิมไว้ก่อน

                // ----------------------------------
                //    ถ้ามีการเลือกไฟล์ใหม่ → ลบไฟล์เก่า + อัพใหม่
                // ----------------------------------
                if (StudentFile?.Length > 0)
                {
                    // ⛔ ลบไฟล์เก่า
                    if (!string.IsNullOrEmpty(student.AttachFile))
                    {
                        var oldPath = Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "wwwroot",
                            student.AttachFile.Replace("/", Path.DirectorySeparatorChar.ToString())
                        );

                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // -----------------------------
                    //    Upload ไฟล์ใหม่
                    // -----------------------------
                    var studentFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/student");

                    // สร้างโฟลเดอร์ถ้ายังไม่มี
                    Directory.CreateDirectory(studentFolderPath);

                    var newFileName = $"{Guid.NewGuid()}_{model.Code}.webp".Replace(" ", "_");

                    var filePath = Path.Combine(studentFolderPath, newFileName);

                    var compressedBytes = await _fileService.CompressToMaxSizeAsync(StudentFile, maxBytes: 2 * 1024 * 1024);

                    await System.IO.File.WriteAllBytesAsync(filePath, compressedBytes);

                    StudentFileName = Path.Combine("uploads", "student", newFileName).Replace("\\", "/");
                }

                // ----------------------------------
                //    อัปเดตข้อมูลนักเรียน
                // ----------------------------------
                student.AttachFile = StudentFileName;
                student.Prefix_Id = model.Prefix_Id;
                student.FirstName = model.FirstName;
                student.LastName = model.LastName;
                student.NickName = model.NickName;
                student.Height = model.Height;
                student.Weight = model.Weight;
                student.IDCard = model.IDCard;
                student.Religion_Id = model.Religion_Id;
                student.Nationality = model.Nationality;
                student.DateOfBirth = model.DateOfBirth;
                student.DisabilityType_Id = model.DisabilityType_Id;
                student.Code = model.Code;

                student.HouseRegNo = model.HouseRegNo;
                student.Province_Id = model.Province_Id;
                student.District_Id = model.District_Id;
                student.Subdistrict_Id = model.Subdistrict_Id;
                student.ZipCode_Id = model.ZipCode_Id;
                student.Address = model.Address;
                student.Address_desc = model.Address_desc;
                student.Soi = model.Soi;
                student.Road = model.Road;
                student.Start_Date = model.StartDate;

                student.UpdateDate = DateTime.UtcNow;
                student.UpdateBy = user.Id;

                await _context.SaveChangesAsync();

                // ----------------------------------------------------
                //              Update PreviousSchool
                // ----------------------------------------------------

                var oldPreviousSchool = await _context.PreviousSchoolData.FirstOrDefaultAsync(x => x.Student_Id == student.Id.ToString());

                bool hasPreviousSchoolInput =
                    !string.IsNullOrWhiteSpace(model.PreviousSchool.SchoolName) &&
                    model.PreviousSchool.SchoolProvince_Id != null &&
                    model.PreviousSchool.GradeLevel_Id != null;

                if (hasPreviousSchoolInput)
                {
                    if (oldPreviousSchool == null)
                    {
                        // Create
                        var newPreviousSchool = new PreviousSchoolDataModel
                        {
                            Student_Id = student.Id.ToString(),
                            SchoolName = model.PreviousSchool.SchoolName,
                            SchoolProvince_Id = model.PreviousSchool.SchoolProvince_Id,
                            GradeLevel_Id = model.PreviousSchool.GradeLevel_Id,
                            CreateDate = DateTime.UtcNow,
                            CreateBy = user.Id,
                            IsActive = true,
                        };
                        _context.PreviousSchoolData.Add(newPreviousSchool);
                    }
                    else
                    {
                        // Update
                        oldPreviousSchool.SchoolName = model.PreviousSchool.SchoolName;
                        oldPreviousSchool.SchoolProvince_Id = model.PreviousSchool.SchoolProvince_Id;
                        oldPreviousSchool.GradeLevel_Id = model.PreviousSchool.GradeLevel_Id;
                        oldPreviousSchool.UpdateBy = user.Id;
                        oldPreviousSchool.UpdateDate = DateTime.UtcNow;
                        oldPreviousSchool.IsActive = true;
                    }

                    await _context.SaveChangesAsync();
                }
                else
                {
                    // ไม่มีข้อมูล 
                    if (oldPreviousSchool != null)
                    {
                        oldPreviousSchool.UpdateBy = user.Id;
                        oldPreviousSchool.UpdateDate = DateTime.UtcNow;
                        oldPreviousSchool.IsActive = false;
                        await _context.SaveChangesAsync();
                    }
                }

                // ----------------------------------------------------
                //           Update Parents (ลบทั้งหมด → ใส่ใหม่)
                // ----------------------------------------------------
                var existingParents = await _context.ParentContactData.Where(x => x.Student_Id == student.Id.ToString()).ToListAsync();

                // วนลูปรายการที่ส่งมาจากฟอร์ม
                foreach (var p in model.Parents)
                {
                    if (p.Id != null)
                    {
                        // -------------------------------
                        //    UPDATE Parent ที่มีอยู่แล้ว
                        // -------------------------------
                        var parent = existingParents.FirstOrDefault(x => x.Id == p.Id);
                        if (parent != null)
                        {
                            parent.Prefix_Id = p.PrefixParent_Id;
                            parent.Relationship_Id = p.Relationship_Id;
                            parent.FirstName = p.ContactFirstName;
                            parent.LastName = p.ContactLastName;
                            parent.PhoneNumber = p.ContactPhone;
                            parent.Occupation = p.Occupation;
                            parent.Status = p.ParentStatus_Id;
                            parent.IDCard = p.IDCard;
                            parent.DateOfBirth = p.DateOfBirth;
                            parent.Religion_Id = p.Religion_Id;
                            parent.Nationality = p.Nationality;
                            parent.UpdateDate = DateTime.UtcNow;
                            parent.UpdateBy = user.Id;
                        }
                    }
                    else
                    {
                        // -------------------------------
                        //      CREATE Parent ใหม่
                        // -------------------------------
                        var newParent = new ParentContactDataModel
                        {
                            Student_Id = student.Id.ToString(),
                            Prefix_Id = p.PrefixParent_Id,
                            Relationship_Id = p.Relationship_Id,
                            FirstName = p.ContactFirstName,
                            LastName = p.ContactLastName,
                            PhoneNumber = p.ContactPhone,
                            Occupation = p.Occupation,
                            Status = p.ParentStatus_Id,
                            IDCard = p.IDCard,
                            DateOfBirth = p.DateOfBirth,
                            Religion_Id = p.Religion_Id,
                            Nationality = p.Nationality,
                            CreateDate = DateTime.UtcNow,
                            CreateBy = user.Id,
                            IsActive = true,
                        };

                        _context.ParentContactData.Add(newParent);
                    }
                }

                await _context.SaveChangesAsync();

                // ----------------------------------------------------
                //         HistoryClass ปัจจุบัน Log
                // ----------------------------------------------------
                var historyCurrent = await _context.StudentHistoryClassData.Where(x => x.Student_Id == student.Id.ToString() && x.IsCurrent == true).FirstOrDefaultAsync();

                bool isChanged =
                    historyCurrent == null ||
                    historyCurrent.School_Year_Id != model.School_Year_Id ||
                    historyCurrent.Term_Id != model.Term_Id ||
                    historyCurrent.Grade_Level_Id != model.GradeLevel_Id;

                var existedRecord = await _context.StudentHistoryClassData.FirstOrDefaultAsync(x =>
                            x.Student_Id == student.Id.ToString() &&
                            x.School_Year_Id == model.School_Year_Id &&
                            x.Term_Id == model.Term_Id &&
                            x.Grade_Level_Id == model.GradeLevel_Id);

                if (existedRecord != null)
                {
                    // ปิด Log เดิม
                    if (historyCurrent != null && historyCurrent.Id != existedRecord.Id)
                    {
                        historyCurrent.IsCurrent = false;
                        await _context.SaveChangesAsync();
                    }

                    existedRecord.Room_Id = model.Room_Id;
                    existedRecord.IsCurrent = true;
                }
                else
                {
                    if (isChanged)
                    {
                        // ปิด Log เดิม
                        if (historyCurrent != null)
                        {
                            historyCurrent.IsCurrent = false;
                            await _context.SaveChangesAsync();
                        }

                        // เพิ่ม Log ใหม่
                        var newHistory = new StudentHistoryClassDataModel
                        {
                            Student_Id = student.Id.ToString(),
                            School_Year_Id = model.School_Year_Id,
                            Term_Id = model.Term_Id,
                            Grade_Level_Id = model.GradeLevel_Id,
                            Room_Id = model.Room_Id,
                            IsCurrent = true,
                        };

                        _context.StudentHistoryClassData.Add(newHistory);
                    }
                    else
                    {
                        historyCurrent.Room_Id = model.Room_Id;
                    }

                }


                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Detail(string Id)
        {
            try
            {
                if (!Guid.TryParse(Id, out var studentId))
                {
                    ViewBag.ErrorMessage = "Invalid student Id";
                    return View("NotFound");
                }

                // ===============================
                // 1️⃣ Student + Current Class
                // ===============================
                var student = await
                    (from s in _context.StudentData
                     join h in _context.StudentHistoryClassData
                        on s.Id.ToString() equals h.Student_Id
                     where s.Id == studentId && h.IsCurrent == true

                     join g in _context.GradeLevelData
                        on h.Grade_Level_Id equals g.Id into gGroup
                     from g in gGroup.DefaultIfEmpty()

                     join r in _context.RoomData
                        on h.Room_Id equals r.Id into rGroup
                     from r in rGroup.DefaultIfEmpty()

                     select new
                     {
                         Student = s,
                         Grade = g,
                         Room = r,

                         Prefix = _context.PrefixData.FirstOrDefault(p => p.Id == s.Prefix_Id),
                         DisabilityType = _context.DisabilityTypeData.FirstOrDefault(d => d.Id == s.DisabilityType_Id),
                         Province = _context.ProvincesData.FirstOrDefault(pr => pr.Id == s.Province_Id),
                         District = _context.DistrictData.FirstOrDefault(d => d.Id == s.District_Id),
                         SubDistrict = _context.SubDistrictData.FirstOrDefault(sd => sd.Id == s.Subdistrict_Id),
                         ZipCode = _context.ZipCodeData.FirstOrDefault(z => z.Id == s.ZipCode_Id),
                         Religion = _context.ReligionData.FirstOrDefault(rg => rg.Id == s.Religion_Id),

                         PreviousSchoolData =
                             (from ps in _context.PreviousSchoolData
                              where ps.Student_Id == s.Id.ToString()

                              join pr in _context.ProvincesData
                                on ps.SchoolProvince_Id equals pr.Id into prGroup
                              from pr in prGroup.DefaultIfEmpty()

                              join gl in _context.GradeLevelData
                                on ps.GradeLevel_Id equals gl.Id into glGroup
                              from gl in glGroup.DefaultIfEmpty()

                              select new PreviousSchoolViewModel
                              {
                                  SchoolName = ps.SchoolName,
                                  SchoolProvince = pr != null ? pr.Name : null,
                                  GradeLevel = gl != null ? gl.Name : null
                              }).FirstOrDefault(),

                         Parents =
                             (from p in _context.ParentContactData
                              join rel in _context.RelationshipData
                                on p.Relationship_Id equals rel.Id
                              where p.Student_Id == s.Id.ToString()
                              select new ParentContactViewModel
                              {
                                  PrefixParent_Id = p.Prefix_Id,
                                  PrefixParent = _context.PrefixData
                                      .Where(x => x.Id == p.Prefix_Id)
                                      .Select(x => x.Name)
                                      .FirstOrDefault(),

                                  Relationship_Id = p.Relationship_Id,
                                  Relationship = rel.Name,

                                  Religion = _context.ReligionData
                                      .Where(x => x.Id == p.Religion_Id)
                                      .Select(x => this.currentLang.ToString() == "en-US" ? x.NameEn : x.Name)
                                      .FirstOrDefault(),

                                  ContactFirstName = p.FirstName,
                                  ContactLastName = p.LastName,
                                  ContactPhone = p.PhoneNumber,
                                  ParentStatus_Id = p.Status,
                                  IDCard = p.IDCard,
                                  DateOfBirth = p.DateOfBirth,
                                  Religion_Id = p.Religion_Id,
                                  Occupation = p.Occupation,

                                  ParentStatus = _context.ParentStatusData
                                      .Where(x => x.Id == p.Status)
                                      .Select(x => this.currentLang.ToString() == "en-US" ? x.NameEng : x.Name)
                                      .FirstOrDefault()
                              }).ToList()
                     }).FirstOrDefaultAsync();

                if (student == null)
                {
                    ViewBag.ErrorMessage = "Student cannot be found!";
                    return View("NotFound");
                }

                // ===============================
                // 2️⃣ Grade / SchoolYear History
                // ===============================
                var gradeHistoryList =
                    await (from h in _context.StudentHistoryClassData.AsNoTracking()
                           where h.Student_Id == studentId.ToString()

                           join sy in _context.SchoolYearData
                                on h.School_Year_Id equals sy.Id into syGroup
                           from sy in syGroup.DefaultIfEmpty()

                           join t in _context.TermData
                                on h.Term_Id equals t.Id into tGroup
                           from t in tGroup.DefaultIfEmpty()

                           join g in _context.GradeLevelData
                                on h.Grade_Level_Id equals g.Id into gGroup
                           from g in gGroup.DefaultIfEmpty()

                           join r in _context.RoomData
                                on h.Room_Id equals r.Id into rGroup
                           from r in rGroup.DefaultIfEmpty()

                           orderby sy.Id descending, t.Id descending

                           select new GradeHistoryViewModel
                           {
                               SchoolYear = sy != null ? sy.Name : string.Empty,
                               Term = t != null ? t.Name : string.Empty,
                               GradeName = g != null ? g.Name : string.Empty,
                               RoomName = r != null ? r.Name : string.Empty,
                               IsCurrent = h.IsCurrent ?? false
                           }).ToListAsync();

                // ===============================
                // 3️⃣ อายุผู้ปกครอง
                // ===============================
                if (student.Parents != null)
                {
                    foreach (var parent in student.Parents)
                    {
                        if (!string.IsNullOrWhiteSpace(parent.DateOfBirth) &&
                            DateTime.TryParseExact(parent.DateOfBirth, "yyyy-MM-dd",
                                CultureInfo.InvariantCulture, DateTimeStyles.None, out var dob))
                        {
                            int age = DateTime.Today.Year - dob.Year;
                            if (dob > DateTime.Today.AddYears(-age)) age--;
                            parent.Age = age;
                            parent.DateOfBirth = dob.ToString("dd/MMM/yyyy");
                        }
                        else
                        {
                            parent.Age = null;
                            parent.DateOfBirth = string.Empty;
                        }
                    }
                }

                // ===============================
                // 4️⃣ ViewModel
                // ===============================
                var fullName = $"{student.Prefix?.Abbreviation}{student.Student.FirstName} {student.Student.LastName}";

                var model = new StudantViewModel
                {
                    Id = student.Student.Id.ToString(),
                    Student_Id = student.Student.Code,
                    Student_Name = fullName,
                    NickName = student.Student.NickName,

                    Grade_Level_Name = student.Grade?.Name ?? string.Empty,
                    Room_Name = student.Room?.Name ?? string.Empty,
                    GradeLevel_Room = student.Grade != null && student.Room != null
                        ? $"{student.Grade.Name} / {student.Room.Name}"
                        : string.Empty,

                    Student_Status_Id = student.Student.Status,
                    AttachFile = student.Student.AttachFile,

                    IDCard = student.Student.IDCard,
                    DateOfBirth = student.Student.DateOfBirth,

                    DisabilityType = student.DisabilityType?.DisabilityTypeName ?? string.Empty,
                    Height = student.Student.Height,
                    Weight = student.Student.Weight,

                    StartDate = student.Student.Start_Date.HasValue
                        ? student.Student.Start_Date.Value.ToString("dd/MMM/yyyy", new CultureInfo("th-TH"))
                        : string.Empty,

                    Address = student.Student.Address,
                    Address_desc = student.Student.Address_desc,
                    Soi = student.Student.Soi,
                    Road = student.Student.Road,

                    Provinces = student.Province != null
                        ? (this.currentLang.ToString() == "en-US" ? student.Province.NameEn : student.Province.Name)
                        : string.Empty,

                    Districts = student.District != null
                        ? (this.currentLang.ToString() == "en-US" ? student.District.NameEn : student.District.Name)
                        : string.Empty,

                    Subdistricts = student.SubDistrict != null
                        ? (this.currentLang.ToString() == "en-US" ? student.SubDistrict.NameEn : student.SubDistrict.Name)
                        : string.Empty,

                    ZipCode = student.ZipCode?.Name ?? string.Empty,
                    Religion = student.Religion?.Name ?? string.Empty,
                    Nationality = student.Student.Nationality,

                    PreviousSchool = student.PreviousSchoolData,
                    Parents = student.Parents,

                    // ⭐ สำคัญ
                    GradeHistories = gradeHistoryList
                };

                return PartialView("_Detail", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return PartialView("NotFound");
            }
        }

        private async Task LoadDropdownsAsync()
        {
            this.currentLang = this._lang.CurrentLang(HttpContext.Request);

            var prefix = await _context.PrefixData.ToListAsync();
            var disabilitytype = await _context.DisabilityTypeData.ToListAsync();
            var religion = await _context.ReligionData.ToListAsync();
            var provinces = await _context.ProvincesData.ToListAsync();
            var grandLevel = await _context.GradeLevelData.ToListAsync();
            var relationship = await _context.RelationshipData.ToListAsync();
            var parentStatus = await _context.ParentStatusData.ToListAsync();
            var schoolYear = await _context.SchoolYearData.OrderByDescending(_ => _.Name).ToListAsync();
            var status = await _context.StudentStatusData.ToListAsync();

            // ส่วนนี้เหมือนเดิม …
            ViewBag.PrefixList = prefix
                .Where(x => x.Abbreviation == "ด.ช." || x.Abbreviation == "ด.ญ.")
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Abbreviation,
                }).ToList();

            ViewBag.DisabilityTypeList = disabilitytype
                .Where(x => x.IsActive == true)
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.DisabilityTypeName,
                }).ToList();

            ViewBag.ParentPrefixList = prefix
                .Where(x => x.Abbreviation != "ด.ช." && x.Abbreviation != "ด.ญ.")
                .Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name,
                }).ToList();

            ViewBag.ReligionList = religion.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = this.currentLang.ToString() == "en-US" ? x.NameEn : x.Name
            }).ToList();

            ViewBag.ProvincesList = provinces.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = this.currentLang.ToString() == "en-US" ? x.NameEn : x.Name
            }).ToList();

            ViewBag.GrandLevelList = grandLevel.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList();

            ViewBag.RelationshipList = relationship.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = this.currentLang.ToString() == "en-US" ? x.NameEng : x.Name
            }).ToList();

            ViewBag.ParentStatusList = parentStatus.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = this.currentLang.ToString() == "en-US" ? x.NameEng : x.Name
            }).ToList();

            ViewBag.SchoolYearList = schoolYear.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList();

            ViewBag.TermList = new List<SelectListItem>();

            ViewBag.StatusList = status.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = this.currentLang.ToString() == "en-US" ? x.NameEng : x.Name
            }).ToList();
        }


        public async Task<IActionResult> Create()
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User cannot be found!";
                    return View("NotFound");
                }
                var model = new StudentDTOModel
                {
                    PreviousSchool = new PreviousSchoolViewModel(),
                    Parents = new List<ParentContactViewModel>
                    {
                        new ParentContactViewModel(),
                        new ParentContactViewModel()
                    }
                };

                await LoadDropdownsAsync();
                return PartialView(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return PartialView("NotFound");
            }
        }

        public async Task<IActionResult> UpdateStatus()
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User cannot be found!";
                    return View("NotFound");
                }
                var model = new StudentDTOModel
                {
                    PreviousSchool = new PreviousSchoolViewModel(),
                    Parents = new List<ParentContactViewModel>
                    {
                        new ParentContactViewModel(),
                        new ParentContactViewModel()
                    }
                };

                await LoadDropdownsAsync();
                return PartialView("_UpdateStatus", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return PartialView("NotFound");
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStudentStatusDTO model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                    return Unauthorized();

                if (model.StudentIds == null || !model.StudentIds.Any())
                    return BadRequest("Student Id is required");

                var students = _context.StudentData
                    .Where(s => s.Id.HasValue && model.StudentIds.Contains(s.Id.Value))
                    .ToList();

                if (!students.Any())
                    return NotFound("Students not found");

                foreach (var student in students)
                {
                    student.Status = model.Status;
                    student.UpdateDate = DateTime.Now;
                    student.UpdateBy = user.Id;
                }

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> UpgradeStudent()
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User cannot be found!";
                    return View("NotFound");
                }
                var model = new StudentDTOModel
                {
                    PreviousSchool = new PreviousSchoolViewModel(),
                    Parents = new List<ParentContactViewModel>
                    {
                        new ParentContactViewModel(),
                        new ParentContactViewModel()
                    }
                };

                await LoadDropdownsAsync();
                return PartialView("_UpgradeStudent", model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return PartialView("NotFound");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetStudentsByGrade(int gradeLevelId)
        {
            var students = await (
                from student in _context.StudentData
                join prefix in _context.PrefixData
                    on student.Prefix_Id equals prefix.Id
                join history in _context.StudentHistoryClassData
                    on student.Id.ToString() equals history.Student_Id
                join status in _context.StudentStatusData
                    on student.Status equals status.Id
                join room in _context.RoomData
                    on history.Room_Id equals room.Id into roomJoin
                from room in roomJoin.DefaultIfEmpty()
                where history.Grade_Level_Id == gradeLevelId
                    && (student.Status == 1 || student.Status == 2)
                      && history.IsCurrent == true
                      && student.IsActive == true
                select new
                {
                    StudentId = student.Id,
                    Code = student.Code,
                    FullName = prefix.Abbreviation + student.FirstName + " " + student.LastName,
                    StudentStatusId = status.Id,
                    StatusName = status.Name,
                    RoomName = room != null ? room.Name : "-"
                }
            ).ToListAsync();

            return Json(students);
        }


        [HttpPost]
        public async Task<IActionResult> UpgradeStudents([FromBody] UpgradeStudentRequest model)
        {
            try
            {
                var user = await _userManager.GetUserAsync(HttpContext.User);
                if (user == null)
                {
                    ViewBag.ErrorMessage = "User cannot be found!";
                    return View("NotFound");
                }

                var currentHistories = await _context.StudentHistoryClassData
                    .Where(x => model.StudentIds.Contains(x.Student_Id) && x.IsCurrent == true)
                    .ToListAsync();

                foreach (var studentId in model.StudentIds)
                {
                    var current = currentHistories.FirstOrDefault(x => x.Student_Id == studentId && x.IsCurrent == true);

                    bool isChanged =
                        current == null ||
                        current.School_Year_Id != model.New_SchoolYear_Id ||
                        current.Term_Id != model.NewTerm_Id ||
                        current.Grade_Level_Id != model.NewGradeLevel_Id ||
                        current.Room_Id != model.NewRoom_Id;

                    if (!isChanged)
                        continue;

                    if (current != null)
                        current.IsCurrent = false;

                    var existed = await _context.StudentHistoryClassData.FirstOrDefaultAsync(x =>
                        x.Student_Id == studentId &&
                        x.School_Year_Id == model.New_SchoolYear_Id &&
                        x.Term_Id == model.NewTerm_Id &&
                        x.Grade_Level_Id == model.NewGradeLevel_Id
                    );

                    if (existed != null)
                    {
                        existed.IsCurrent = true;
                        existed.Room_Id = model.NewRoom_Id;
                    }
                    else
                    {
                        _context.StudentHistoryClassData.Add(new StudentHistoryClassDataModel
                        {
                            Student_Id = studentId,
                            School_Year_Id = model.New_SchoolYear_Id,
                            Term_Id = model.NewTerm_Id,
                            Grade_Level_Id = model.NewGradeLevel_Id,
                            Room_Id = model.NewRoom_Id,
                            IsCurrent = true
                        });

                        var student = await _context.StudentData
                                .FirstOrDefaultAsync(x => x.Id.ToString() == studentId);

                        if (student != null && student.Status == 1)
                        {
                            student.Status = 2; // กำลังศึกษา
                        }
                    }
                }
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StudentDTOModel model, IFormFile? StudentFile)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    await LoadDropdownsAsync();
                    return View(model);
                }
                var user = await _userManager.GetUserAsync(HttpContext.User);

                if (user is null || user.Id is null)
                {
                    ViewBag.ErrorMessage = $"User cannot be found!";
                    return View("NotFound");
                }

                string? StudentFileName = null;

                if (StudentFile?.Length > 0)
                {
                    var studentFolderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/student");
                    // สร้างโฟลเดอร์ถ้ายังไม่มี
                    Directory.CreateDirectory(studentFolderPath);

                    // ตั้งชื่อไฟล์ใหม่แบบหล่อ ๆ กันชนกัน
                    var newFileName = $"{Guid.NewGuid()}_{model.Code}.webp".Replace(" ", "_");

                    var filePath = Path.Combine(studentFolderPath, newFileName);

                    var compressedBytes = await _fileService.CompressToMaxSizeAsync(StudentFile, maxBytes: 2 * 1024 * 1024);

                    await System.IO.File.WriteAllBytesAsync(filePath, compressedBytes);

                    // สร้าง path แบบ relative สำหรับเก็บใน DB
                    // เช่น "uploads/M1/Room1/xxxx.jpg"
                    StudentFileName = Path.Combine("uploads", "student", newFileName)
                                        .Replace("\\", "/"); // เปลี่ยน \ เป็น / ให้ compatible เว็บ
                }


                var student = new StudentDataModel
                {
                    Id = Guid.NewGuid(),
                    AttachFile = StudentFileName,
                    Prefix_Id = model.Prefix_Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    NickName = model.NickName,
                    Height = model.Height,
                    Weight = model.Weight,
                    IDCard = model.IDCard,
                    Religion_Id = model.Religion_Id,
                    Nationality = model.Nationality,
                    DateOfBirth = model.DateOfBirth,
                    DisabilityType_Id = model.DisabilityType_Id,

                    Code = model.Code,
                    Status = 2,

                    HouseRegNo = model.HouseRegNo,
                    Province_Id = model.Province_Id,
                    District_Id = model.District_Id,
                    Subdistrict_Id = model.Subdistrict_Id,
                    ZipCode_Id = model.ZipCode_Id,
                    Address = model.Address,
                    Address_desc = model.Address_desc,
                    Soi = model.Soi,
                    Road = model.Road,
                    Start_Date = model.StartDate,

                    CreateDate = DateTime.UtcNow,
                    CreateBy = user.Id,
                    IsActive = true,
                };
                _context.StudentData.Add(student);
                await _context.SaveChangesAsync();

                var historyClass = new StudentHistoryClassDataModel
                {
                    Student_Id = student.Id.ToString(),
                    School_Year_Id = model.School_Year_Id,
                    Term_Id = model.Term_Id,
                    Grade_Level_Id = model.GradeLevel_Id,
                    Room_Id = model.Room_Id,
                    IsCurrent = true,
                };
                _context.StudentHistoryClassData.Add(historyClass);
                await _context.SaveChangesAsync();

                if (model.PreviousSchool.SchoolName != null && model.PreviousSchool.SchoolProvince_Id != null && model.PreviousSchool.GradeLevel_Id != null)
                {
                    var previousSchool = new PreviousSchoolDataModel
                    {
                        Student_Id = student.Id.ToString(),
                        SchoolName = model.PreviousSchool.SchoolName,
                        SchoolProvince_Id = model.PreviousSchool.SchoolProvince_Id,
                        GradeLevel_Id = model.PreviousSchool.GradeLevel_Id,
                        CreateDate = DateTime.UtcNow,
                        CreateBy = user.Id,
                        IsActive = true,
                    };
                    _context.PreviousSchoolData.Add(previousSchool);
                    await _context.SaveChangesAsync();
                }


                foreach (var p in model.Parents)
                {
                    // if (string.IsNullOrWhiteSpace(p.ContactFirstName))
                    //     continue;
                    var parent = new ParentContactDataModel
                    {
                        Student_Id = student.Id.ToString(),
                        Prefix_Id = p.PrefixParent_Id,
                        Relationship_Id = p.Relationship_Id,
                        FirstName = p.ContactFirstName,
                        LastName = p.ContactLastName,
                        PhoneNumber = p.ContactPhone,
                        Status = p.ParentStatus_Id,
                        IDCard = p.IDCard,
                        DateOfBirth = p.DateOfBirth,
                        Religion_Id = p.Religion_Id,
                        Nationality = p.Nationality,
                        CreateDate = DateTime.UtcNow,
                        CreateBy = user.Id,
                        IsActive = true,
                    };

                    _context.ParentContactData.Add(parent);
                }
                await _context.SaveChangesAsync();



                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                return View("NotFound");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTermsBySchoolYear(int schoolYearId)
        {
            var currentLang = this._lang.CurrentLang(HttpContext.Request).ToString();

            var terms = await _context.TermData
                .Where(x => x.SchoolYear_Id == schoolYearId && x.IsActive == true)
                .Select(x => new
                {
                    value = x.Id,
                    text = x.Name
                })
                .ToListAsync();

            return Json(new { status = 200, termsList = terms });
        }



        [HttpGet]
        public async Task<IActionResult> GetDistrict(int province)
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);

                var districts = await _context.DistrictData.Where(x => x.Province_Id == province).ToListAsync();

                var districtsList = districts.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = this.currentLang.ToString() == "en-US" ? x.NameEn : x.Name
                }).ToList();

                return Json(new { status = 200, districtslist = districtsList });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetRoom(int grade_level)
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);

                var room = await _context.RoomData.Where(x => x.Grade_Level_Id == grade_level).ToListAsync();

                var roomList = room.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = this.currentLang.ToString() == "en-US" ? x.NameEng : x.Name
                }).ToList();

                return Json(new { status = 200, roomList = roomList });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetSubDistrict(int province, int district)
        {
            try
            {
                this.currentLang = this._lang.CurrentLang(HttpContext.Request);

                var subDistricts = await _context.SubDistrictData.Where(x => x.Province_Id == province && x.District_Id == district).ToListAsync();


                var subDistrictsList = subDistricts.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = this.currentLang.ToString() == "en-US" ? x.NameEn : x.Name
                }).ToList();

                return Json(new { status = 200, subdistrictslist = subDistrictsList });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetZipCode(int province, int district, int subdistrict)
        {
            try
            {
                var zipCode = await _context.ZipCodeData
                    .Where(x => x.Province_Id == province
                             && x.District_Id == district
                             && x.Subdistrict_Id == subdistrict)
                    .ToListAsync();


                var zipCodeList = zipCode.Select(x => new SelectListItem
                {
                    Value = x.Id.ToString(),
                    Text = x.Name
                }).ToList();

                return Json(new { status = 200, zipcodelist = zipCodeList });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        #region AddHistoryStudent
        // [HttpPost]
        // public async Task<IActionResult> UploadExcel(IFormFile file)
        // {
        //     if (file == null || file.Length == 0)
        //         return BadRequest("No file uploaded");

        //     int importedCount = 0;
        //     var userId = User?.Identity?.Name ?? "System";

        //     using (var stream = file.OpenReadStream())
        //     {
        //         var students = ReadStudentsFromExcel(stream);

        //         foreach (var model in students)
        //         {
        //             await InsertStudentAsync(model);
        //             importedCount++;
        //         }
        //     }

        //     return Ok(new { importedCount });
        // }

        // public List<StudentExcelHistoryModel> ReadStudentsFromExcel(Stream excelStream)
        // {
        //     var students = new List<StudentExcelHistoryModel>();
        //     ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //     using (var package = new ExcelPackage(excelStream))
        //     {
        //         var worksheet = package.Workbook.Worksheets["Student List"];
        //         if (worksheet == null) throw new Exception("ไม่พบ sheet 'Student List'");
        //         if (worksheet.Dimension == null) throw new Exception("Sheet 'Student List' ไม่มีข้อมูล");

        //         int startRow = 2; // header
        //         int endRow = worksheet.Dimension.End.Row;

        //         for (int row = startRow; row < endRow; row++)
        //         {
        //             var gradeStartText = worksheet.Cells[row, 11].Text;

        //             int? gradeStart = null;
        //             if (int.TryParse(gradeStartText, out var value))
        //             {
        //                 gradeStart = value;
        //             }

        //             var student = new StudentExcelHistoryModel
        //             {
        //                 ID = worksheet.Cells[row, 1].Text,
        //                 GradeLevel = Int32.Parse(worksheet.Cells[row, 6].Text),
        //                 Room_Id = Int32.Parse(worksheet.Cells[row, 7].Text),
        //                 Grade_Start = gradeStart,
        //                 StartDate = DateOnly.Parse(worksheet.Cells[row, 5].Text)
        //             };

        //             students.Add(student);
        //         }
        //     }

        //     return students;
        // }


        // public async Task<ResponsesModel> InsertStudentAsync(StudentExcelHistoryModel model)
        // {
        //     ResponsesModel response = new ResponsesModel();
        //     try
        //     {

        //         var user = await _userManager.GetUserAsync(HttpContext.User);

        //         int curentyear = 5; // 2565
        //         int curentterm = 9; // เทอม 1

        //         int gradelevel = model.GradeLevel - 1;
        //         int roomlevel = model.Room_Id - 1;

        //         int startYear = model.StartDate.Year;
        //         int endYear = 2023;
        //         int yearDiff = endYear - startYear;

        //         for (var i = 0; i < yearDiff; i++)
        //         {
        //             var roopt = model.StartDate.Month >= 11 && model.StartDate.Year == endYear ? 1 : 2;
        //             for (var x = 0; x < roopt; x++)
        //             {
        //                 var _historyClass = new StudentHistoryClassDataModel
        //                 {
        //                     Student_Id = model.ID,
        //                     School_Year_Id = curentyear,
        //                     Term_Id = curentterm,
        //                     Grade_Level_Id = gradelevel,
        //                     Room_Id = roomlevel,
        //                     IsCurrent = false,
        //                 };
        //                 _context.StudentHistoryClassData.Add(_historyClass);
        //                 await _context.SaveChangesAsync();
        //                 curentterm++;
        //             }
        //             curentyear++;
        //             gradelevel--;
        //             roomlevel--;
        //         }


        //         response.status = true;
        //         response.message = "Success";
        //         return response;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, ex.Message);
        //         response.status = false;
        //         response.message = "Error" + ex.ToString();
        //         return response;
        //     }
        // }

        #endregion


        #region AddStudentInfo


        // [HttpPost]
        // public async Task<IActionResult> UploadExcel(IFormFile file)
        // {
        //     if (file == null || file.Length == 0)
        //         return BadRequest("No file uploaded");

        //     int importedCount = 0;
        //     var userId = User?.Identity?.Name ?? "System";

        //     using (var stream = file.OpenReadStream())
        //     {
        //         var students = ReadStudentsFromExcel(stream);

        //         foreach (var model in students)
        //         {
        //             await InsertStudentAsync(model);
        //             importedCount++;
        //         }
        //     }

        //     return Ok(new { importedCount });
        // }

        // public List<StudentExcelModel> ReadStudentsFromExcel(Stream excelStream)
        // {
        //     var students = new List<StudentExcelModel>();
        //     ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        //     using (var package = new ExcelPackage(excelStream))
        //     {
        //         var worksheet = package.Workbook.Worksheets["Student List"];
        //         if (worksheet == null) throw new Exception("ไม่พบ sheet 'Student List'");
        //         if (worksheet.Dimension == null) throw new Exception("Sheet 'Student List' ไม่มีข้อมูล");

        //         int startRow = 4; // header
        //         int endRow = worksheet.Dimension.End.Row;

        //         for (int row = startRow; row < endRow; row++)
        //         {

        //             var student = new StudentExcelModel
        //             {
        //                 IDCard = worksheet.Cells[row, 2].Text,
        //                 Code = worksheet.Cells[row, 3].Text,
        //                 GradeLevel = Int32.Parse(worksheet.Cells[row, 4].Text),
        //                 Room_Id = Int32.Parse(worksheet.Cells[row, 5].Text),
        //                 PrefixId = Int32.Parse(worksheet.Cells[row, 7].Text),
        //                 FirstName = worksheet.Cells[row, 9].Text,
        //                 LastName = worksheet.Cells[row, 10].Text,
        //                 DateOfBirth = worksheet.Cells[row, 15].Text,
        //                 GenderId = Int32.Parse(worksheet.Cells[row, 17].Text),
        //                 Nationality = worksheet.Cells[row, 19].Text,
        //                 ReligionId = Int32.Parse(worksheet.Cells[row, 20].Text),
        //                 Status = Int32.Parse(worksheet.Cells[row, 22].Text),
        //                 HouseRegNo = worksheet.Cells[row, 27].Text,
        //                 Address = worksheet.Cells[row, 28].Text,
        //                 Moo = worksheet.Cells[row, 29].Text,
        //                 Soi = worksheet.Cells[row, 30].Text,
        //                 Road = worksheet.Cells[row, 31].Text,
        //                 StartDate = DateOnly.Parse(worksheet.Cells[row, 37].Text).AddYears(543),
        //                 Heigh = Decimal.Parse(worksheet.Cells[row, 61].Text),
        //                 Weight = Decimal.Parse(worksheet.Cells[row, 62].Text),

        //                 PreviousSchool = new PreviousSchoolExcelModel
        //                 {
        //                     SchoolName = worksheet.Cells[row, 63].Text,
        //                 },
        //                 Parents = new List<ParentExcelModel>
        //         {
        //             new ParentExcelModel
        //             {
        //                 IDCard = worksheet.Cells[row, 39].Text,
        //                 Prefix = worksheet.Cells[row, 40].Text == "#N/A" ? null : Int32.Parse(worksheet.Cells[row, 40].Text),
        //                 ContactFirstName = worksheet.Cells[row, 42].Text,
        //                 ContactLastName = worksheet.Cells[row, 43].Text,
        //                 Status = Int32.Parse(worksheet.Cells[row, 44].Text),
        //                 Nationality = worksheet.Cells[row, 46].Text,
        //                 RelationshipId = 1
        //             },
        //             new ParentExcelModel
        //             {
        //                 IDCard = worksheet.Cells[row, 47].Text,
        //                 Prefix = worksheet.Cells[row, 48].Text == "#N/A" ? null : Int32.Parse(worksheet.Cells[row, 48].Text),
        //                 ContactFirstName = worksheet.Cells[row, 50].Text,
        //                 ContactLastName = worksheet.Cells[row, 51].Text,
        //                 Status = Int32.Parse(worksheet.Cells[row, 52].Text),
        //                 Nationality = worksheet.Cells[row, 54].Text,
        //                 RelationshipId = 2
        //             }
        //         }
        //             };

        //             students.Add(student);
        //         }
        //     }

        //     return students;
        // }


        // public async Task<ResponsesModel> InsertStudentAsync(StudentExcelModel model)
        // {
        //     ResponsesModel response = new ResponsesModel();
        //     try
        //     {

        //         var user = await _userManager.GetUserAsync(HttpContext.User);

        //         var student = new StudentDataModel
        //         {
        //             Id = Guid.NewGuid(),
        //             Prefix_Id = model.PrefixId,
        //             FirstName = model.FirstName,
        //             LastName = model.LastName,
        //             Height = model.Heigh,
        //             Weight = model.Weight,
        //             IDCard = model.IDCard,
        //             Religion_Id = model.ReligionId,
        //             Nationality = model.Nationality,
        //             DateOfBirth = model.DateOfBirth,
        //             Code = model.Code,
        //             Status = model.Status,

        //             HouseRegNo = model.HouseRegNo,
        //             Address = model.Address,
        //             Address_desc = model.Moo,
        //             Soi = model.Soi,
        //             Road = model.Road,
        //             Start_Date = model.StartDate,

        //             CreateDate = DateTime.UtcNow,
        //             CreateBy = user.Id,
        //             IsActive = true,
        //         };
        //         _context.StudentData.Add(student);
        //         await _context.SaveChangesAsync();

        //         var historyClass2 = new StudentHistoryClassDataModel
        //         {
        //             Student_Id = student.Id.ToString(),
        //             School_Year_Id = 3,
        //             Term_Id = 6,
        //             Grade_Level_Id = model.GradeLevel,
        //             Room_Id = model.Room_Id,
        //             IsCurrent = true,
        //         };
        //         _context.StudentHistoryClassData.Add(historyClass2);
        //         await _context.SaveChangesAsync();

        //         if (model.StartDate.Month < 11)
        //         {
        //             var historyClass1 = new StudentHistoryClassDataModel
        //             {
        //                 Student_Id = student.Id.ToString(),
        //                 School_Year_Id = 3,
        //                 Term_Id = 5,
        //                 Grade_Level_Id = model.GradeLevel,
        //                 Room_Id = model.Room_Id,
        //                 IsCurrent = false,
        //             };
        //             _context.StudentHistoryClassData.Add(historyClass1);
        //             await _context.SaveChangesAsync();
        //         }

        //         int curentyear = 1;
        //         int curentterm = 1;
        //         int gradelevel = 1;
        //         int room = 7;
        //         int roomlevel = 0;
        //         if (model.Status == 2 && model.GradeLevel != 1)
        //         {
        //             int loopy = model.GradeLevel == 2 ? 1 : 2;

        //             for (var i = 0; i < loopy; i++)
        //             {
        //                 for (var x = 0; x < 2; x++)
        //                 {
        //                     var _historyClass = new StudentHistoryClassDataModel
        //                     {
        //                         Student_Id = student.Id.ToString(),
        //                         School_Year_Id = 3 - curentyear,
        //                         Term_Id = 5 - curentterm,
        //                         Grade_Level_Id = model.GradeLevel - gradelevel,
        //                         Room_Id = (model.GradeLevel - gradelevel) == 8 ? model.Room_Id - 2 : (model.GradeLevel - gradelevel) < 8 ? room - roomlevel : null,
        //                         IsCurrent = false,
        //                     };
        //                     _context.StudentHistoryClassData.Add(_historyClass);
        //                     await _context.SaveChangesAsync();
        //                     curentterm++;
        //                 }
        //                 curentyear++;
        //                 gradelevel++;
        //                 roomlevel++;
        //             }
        //         }


        //         if (!String.IsNullOrEmpty(model.PreviousSchool.SchoolName))
        //         {
        //             var previousSchool = new PreviousSchoolDataModel
        //             {
        //                 Student_Id = student.Id.ToString(),
        //                 SchoolName = model.PreviousSchool.SchoolName,
        //                 CreateDate = DateTime.UtcNow,
        //                 CreateBy = user.Id,
        //                 IsActive = true,
        //             };
        //             _context.PreviousSchoolData.Add(previousSchool);
        //             await _context.SaveChangesAsync();
        //         }


        //         foreach (var p in model.Parents)
        //         {
        //             // if (string.IsNullOrWhiteSpace(p.ContactFirstName))
        //             //     continue;
        //             var parent = new ParentContactDataModel
        //             {
        //                 Student_Id = student.Id.ToString(),
        //                 Prefix_Id = p.Prefix,
        //                 Relationship_Id = p.RelationshipId,
        //                 FirstName = p.ContactFirstName,
        //                 LastName = p.ContactLastName,
        //                 Status = p.Status,
        //                 IDCard = p.IDCard,
        //                 Nationality = p.Nationality,
        //                 CreateDate = DateTime.UtcNow,
        //                 CreateBy = user.Id,
        //                 IsActive = true,
        //             };

        //             _context.ParentContactData.Add(parent);
        //         }
        //         await _context.SaveChangesAsync();

        //         response.status = true;
        //         response.message = "Success";
        //         return response;
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, ex.Message);
        //         response.status = false;
        //         response.message = "Error" + ex.ToString();
        //         return response;
        //     }
        // }

        #endregion

    }
}
