using System;

namespace BackOfficeLibrary
{
    public class TeacherViewModel
    {
        public int Id { get; set; }
        public int? Prefix_Id { get; set; }
        public string? Prefix { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? NickName { get; set; }
    }

    public class TeacherDTOModel
    {
        public int Id { get; set; }
        public int? Prefix_Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? NickName { get; set; }
    }

    public class TeacherViewDetailModel
    {
        public string? Prefix { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? FullName { get; set; }
        public string? NickName { get; set; }
    }
}