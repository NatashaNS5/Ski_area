using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Горнолыжный_комплекс__Благодать_.Models
{
    public class OrderService
    {
        public int OrderID { get; set; }
        public int ServiceID { get; set; }
        [ForeignKey("OrderID")] public Order Order { get; set; }
        [ForeignKey("ServiceID")] public Service Service { get; set; }
    }
}
