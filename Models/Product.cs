using System.ComponentModel.DataAnnotations;

namespace OfficeStore.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Название товара обязательно")]
        [StringLength(100, ErrorMessage = "Название не может быть длиннее 100 символов")]
        [Display(Name = "Название")]
        public string Name { get; set; }

        [Display(Name = "Описание")]
        [StringLength(500, ErrorMessage = "Описание не может быть длиннее 500 символов")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Цена обязательна")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Цена должна быть больше 0")]
        [Display(Name = "Цена")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Количество на складе обязательно")]
        [Range(0, int.MaxValue, ErrorMessage = "Количество не может быть отрицательным")]
        [Display(Name = "Количество на складе")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Поставщик обязателен")]
        [Display(Name = "Поставщик")]
        public int SupplierId { get; set; }

        public Supplier Supplier { get; set; }
    }
}
