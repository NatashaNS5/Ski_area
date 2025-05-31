using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Горнолыжный_комплекс__Благодать_.Models
{
    public class Order
    {
        [Key] public int OrderID { get; set; }
        public string OrderCode { get; set; }
        public DateTime CreationDate { get; set; }
        public TimeSpan CreationTime { get; set; }
        public long ClientID { get; set; }
        public string Status { get; set; }
        public DateTime? ClosingDate { get; set; }
        public int RentalDurationMinutes { get; set; }
        [ForeignKey("ClientID")] public Client Client { get; set; }
        public List<OrderService> OrderServices { get; set; }
    }
}
