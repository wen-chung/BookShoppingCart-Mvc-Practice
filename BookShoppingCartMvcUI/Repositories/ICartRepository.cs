using System;

namespace BookShoppingCartMvcUI.Repositories;

public interface ICartRepository
{
    Task<int> AddItem(int bookId, int qty);
    Task<int> RemoveItem(int bookId);
    Task<ShoppingCart> GetCart(string userId);
    Task<ShoppingCart> GetUserCart();
    Task<int> GetCartItemCount(string userId = "");
    string GetUserId();
    Task<bool> DoCheckOut(CheckOutModel model);
}
