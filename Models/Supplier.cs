using System.ComponentModel.DataAnnotations;

namespace OfficeStore.Models
{
    public class Supplier
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string ContactInfo { get; set; }

        public ICollection<Product> Products { get; set; }
    }
}
