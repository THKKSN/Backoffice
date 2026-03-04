using System;

namespace BackOfficeLibrary
{
    public class ResponsesModel
    {
        public bool status { get; set; }
        public string? message { get; set; }
        public int? id { get; set; }
    }
}