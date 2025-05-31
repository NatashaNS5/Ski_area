    using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Горнолыжный_комплекс__Благодать_.Models
{
    public class Employee
    {
        [Key] public int EmployeeID { get; set; }
        public string Position { get; set; }
        public string FullName { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public List<LoginHistory> LoginHistories { get; set; }
    }
}
