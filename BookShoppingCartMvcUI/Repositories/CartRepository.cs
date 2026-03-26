using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCartMvcUI.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _db;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<IdentityUser> _userManager;

    public CartRepository(ApplicationDbContext db, IHttpContextAccessor httpContextAccessor, UserManager<IdentityUser> userManager)
    {
        _db = db;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
    }

    public async Task<int> AddItem(int bookId, int qty)
    {
        string userId = GetUserId();
        using var transaction = _db.Database.BeginTransaction();

        try
        {

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("user is not logged in");

            var cart = await GetCart(userId);
            if (cart is null)
            {
                cart = new ShoppingCart
                {
                    UserId = userId
                };
                _db.ShoppingCarts.Add(cart);
            }
            _db.SaveChanges();

            //cart Detail section
            var cartItem = await _db.CartDetails.FirstOrDefaultAsync(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
            if (cartItem is not null)
            {
                cartItem.Quantity += qty;
            }
            else
            {
                var book = _db.Books.Find(bookId);
                cartItem = new CartDetail
                {
                    BookId = bookId,
                    ShoppingCartId = cart.Id,
                    Quantity = qty,
                    UnitPrice = book.Price
                };
                _db.CartDetails.Add(cartItem);
            }
            _db.SaveChanges();
            transaction.Commit();

        }
        catch (Exception ex)
        {
            transaction.Rollback();
            throw;
        }

        var cartItemCount = await GetCartItemCount(userId);
        return cartItemCount;

    }

    public async Task<int> RemoveItem(int bookId)
    {
        //using var transaction = _db.Database.BeginTransaction();
        string userId = GetUserId();
        try
        {

            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("user is not logged in");

            var cart = await GetCart(userId);
            if (cart is null)
            {
                throw new InvalidOperationException("Invaild Cart");
            }

            //cart Detail section
            var cartItem = await _db.CartDetails.FirstOrDefaultAsync(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
            if (cartItem is null)
            {
                throw new InvalidOperationException("No items in Cart");
            }
            else if (cartItem.Quantity == 1)
            {
                _db.CartDetails.Remove(cartItem);
            }
            else
            {
                cartItem.Quantity = cartItem.Quantity - 1;
            }
            _db.SaveChanges();
            //transaction.Commit();


        }
        catch (Exception ex)
        {

        }

        var cartItemCount = await GetCartItemCount(userId);
        return cartItemCount;

    }

    public async Task<ShoppingCart> GetCart(string userId)
    {
        var cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
        return cart;
    }

    public async Task<ShoppingCart> GetUserCart()
    {
        var userId = GetUserId();
        if (userId == null) throw new InvalidOperationException("Invalid user");

        var shoppingCart = await _db.ShoppingCarts
                                .Include(a => a.CartDetails)
                                .ThenInclude(a => a.Book)
                                .ThenInclude(a => a.Stock)
                                .Include(a => a.CartDetails)
                                .ThenInclude(a => a.Book)
                                .ThenInclude(a => a.Genre)
                                .Where(a => a.UserId == userId).FirstOrDefaultAsync();

        return shoppingCart;
    }

    public async Task<int> GetCartItemCount(string userId = "")
    {
        if (string.IsNullOrEmpty(userId))
        {
            userId = GetUserId();
        }

        var data = await (from cart in _db.ShoppingCarts
                          join cartDetail in _db.CartDetails
                          on cart.Id equals cartDetail.ShoppingCartId
                          where cart.UserId == userId
                          select new { cartDetail.Id }
                          ).ToListAsync();

        return data.Count();
    }

    public async Task<bool> DoCheckOut(CheckOutModel model)
    {
        //move data from cartdetial to order and orderdetial then we will remove cartdetial
        //entry->order,orderdetial
        //remove data->cartdetial

        using var transaction = _db.Database.BeginTransaction();

        try
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) throw new UnauthorizedAccessException("User is not logged-in");

            var cart = await GetCart(userId);
            if (cart is null) throw new InvalidOperationException("Invalid Cart");

            var cartDetail = _db.CartDetails.Where(a => a.ShoppingCartId == cart.Id).ToList();
            if (cartDetail.Count == 0) throw new InvalidOperationException("Cart is empty ");

            var pendingRecord = _db.OrderStatuses.FirstOrDefault(s => s.StatusName == "Pending");
            if (pendingRecord is null) throw new InvalidOperationException("Order Status dose not have Pending status.");

            var order = new Order
            {
                UserId = userId,
                CreateDate = DateTime.UtcNow,
                Name = model.Name,
                Email = model.Email,
                MobileNumber = model.MobileNumber,
                PaymentMethod = model.PaymentMethod,
                Address = model.Address,
                IsPaid = false,
                OrderStatusId = (int)Models.Enum.OrderStatus.Pending //pending
            };

            _db.Orders.Add(order);
             _db.SaveChanges();

            foreach (var item in cartDetail)
            {
                var orderdetial = new OrderDetail
                {
                    BookId = item.BookId,
                    OrderId = order.Id,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice
                };
                _db.OrderDetails.Add(orderdetial);

                //update stock here
                var stock = await _db.Stocks.FirstOrDefaultAsync(s => s.BookId == item.BookId);
                if (stock is null) throw new InvalidOperationException("Stock is null");

                if (item.Quantity > stock.Quantity)
                {
                    throw new InvalidOperationException($"Only {stock.Quantity} are avaliable in stock");
                }
                stock.Quantity -= item.Quantity;
            }
            //_db.SaveChanges();

            //removing the CartDetials
            _db.CartDetails.RemoveRange(cartDetail);

            _db.SaveChanges();
            transaction.Commit();
            return true;

        }
        catch (System.Exception e)
        {
            transaction.Rollback();

            //return false;
            throw;
        }

    }

    public string GetUserId()
    {
        var principal = _httpContextAccessor.HttpContext.User;
        var userId = _userManager.GetUserId(principal);

        return userId;
    }
}
