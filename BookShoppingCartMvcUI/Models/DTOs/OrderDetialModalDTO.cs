using System;

namespace BookShoppingCartMvcUI.Models.DTOs;

public class OrderDetialModalDTO
{
public string Divid { get; set; }
public IEnumerable<OrderDetail> OrderDetail { get; set; }
}
