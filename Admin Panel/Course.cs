using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Admin_Panel
{
    public class Course
    {
        public int Id { get; set; }
        public string CourseName { get; set; }
        public string Description { get; set; }
        public string VideoUrl { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsPaid { get; set; }
    }
}
