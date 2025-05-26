using System.ComponentModel.DataAnnotations;

namespace OfficeStore.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public User User { get; set; }

        public DateTime OrderDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
