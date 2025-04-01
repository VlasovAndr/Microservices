using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ShoppingCartAPI.Models.Dto;

namespace ShoppingCartAPI.Models;

public class CartDetails
{
    [Key]
    public int CartDetailsId { get; set; }
    [ForeignKey("CartHeaderId")]
    public int CartHeaderId { get; set; }
    public CartHeader CartHeader { get; set; }
    public int ProductId { get; set; }
    [NotMapped]
    public ProductDto Product { get; set; }
    public int Count { get; set; }
}
