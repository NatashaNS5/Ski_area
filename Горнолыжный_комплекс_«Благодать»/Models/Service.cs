using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Горнолыжный_комплекс__Благодать_.Models
{
    public class Service
    {
        [Key] public int ServiceID { get; set; }
        public string ServiceName { get; set; }
        public string ServiceCode { get; set; }
        public decimal CostPerHour { get; set; }
    }
}
