using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Горнолыжный_комплекс__Благодать_.Models
{
    [Table("LoginHistory")] 
    public class LoginHistory
    {
        [Key] public int HistoryID { get; set; }
        public int EmployeeID { get; set; }
        public DateTime LoginTime { get; set; }
        public string LoginStatus { get; set; }
        [ForeignKey("EmployeeID")] public Employee Employee { get; set; }
    }
}