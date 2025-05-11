using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BookProject.Model
{
    public class OrderItem
    {
        [Key]
        public int OrderItemId { get; set; }



        [Required]
        public int OrderId { get; set; }

        [ForeignKey(nameof(OrderId))]
        public virtual Order? Order { get; set; }

        [Required]
        public long BookId { get; set; }

        [ForeignKey(nameof(BookId))]
        public virtual Book? Book { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal Discount { get; set; }
    }
}
