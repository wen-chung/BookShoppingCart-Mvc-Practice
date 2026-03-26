using System;
using Microsoft.EntityFrameworkCore;


namespace BookShoppingCartMvcUI.Repositories;

public class HomeRepository : IHomeRepository
{
    private readonly ApplicationDbContext _db;

    // private readonly ApplicationDbContext _dbContext;

    public HomeRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<Genre>> Genres()
    {
        return await _db.Genres.ToListAsync();
    }

    public async Task<IEnumerable<Book>> GetBooksAsync(string sTerm = "", int genreid = 0)
    {

        IQueryable<Book> books = (from book in _db.Books
                                  join genre in _db.Genres
                                  on book.GenreId equals genre.Id
                                  join stock in _db.Stocks
                                  on book.Id equals stock.BookId
                                  into book_stocks
                                  from bookWithStock in book_stocks.DefaultIfEmpty()
                                  select new Book
                                  {
                                      Id = book.Id,
                                      Image = book.Image,
                                      AuthorName = book.AuthorName,
                                      BookName = book.BookName,
                                      GenreId = book.GenreId,
                                      GenreName = genre.GenreName,
                                      Price = book.Price,
                                      Quantity = bookWithStock.Quantity == null ? 0 : bookWithStock.Quantity
                                  }
                                );

        if (!string.IsNullOrEmpty(sTerm))
        {
            sTerm = sTerm.ToLower();
            books = books.Where(a => a.BookName.ToLower().StartsWith(sTerm));
        }

        if (genreid > 0)
        {
            books = books.Where(a => a.GenreId == genreid);
        }

        return await books.ToListAsync();
    }
}