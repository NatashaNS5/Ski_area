using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Горнолыжный_комплекс__Благодать_.Models
{
    public class Client
    {
        [Key] public long ClientID { get; set; }
        public string FullName { get; set; }
        public string PassportData { get; set; }
        public DateTime BirthDate { get; set; }
        public string Address { get; set; }
    }
}
