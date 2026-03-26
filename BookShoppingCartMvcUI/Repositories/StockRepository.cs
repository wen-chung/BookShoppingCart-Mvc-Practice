using System;
using Microsoft.EntityFrameworkCore;
using NuGet.Frameworks;

namespace BookShoppingCartMvcUI.Repositories;

public class StockRepository : IStockRepository
{
    private readonly ApplicationDbContext _context;

    public StockRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Stock?> GetStockByBookId(int bookId) => await _context.Stocks.FirstOrDefaultAsync(s => s.BookId == bookId);

    public async Task ManageStock(StockDTO stockToMagage)
    {
        var existingStock = await GetStockByBookId(stockToMagage.BookId);
        if (existingStock is null)
        {
            var stock = new Stock { BookId = stockToMagage.BookId, Quantity = stockToMagage.Quantity };
            _context.Stocks.Add(stock);
        }
        else
        {
            existingStock.Quantity = stockToMagage.Quantity;
        }

        await _context.SaveChangesAsync();

    }

    public async Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerms = "")
    {
        var stocks = await (from book in _context.Books
                            join stock in _context.Stocks
                            on book.Id equals stock.BookId
                            into book_stock
                            from bookStock in book_stock.DefaultIfEmpty()
                            where string.IsNullOrEmpty(sTerms) || book.BookName.ToLower().Contains(sTerms.ToLower())
                            select new StockDisplayModel
                            {
                                BookId = book.Id,
                                BookName = book.BookName,
                                Quantity = bookStock == null ? 0 : bookStock.Quantity
                            }

        ).ToListAsync();

        return stocks;
    }

}


    public interface IStockRepository
    {
        public  Task<IEnumerable<StockDisplayModel>> GetStocks(string sTerms = "");
        public  Task<Stock?> GetStockByBookId(int bookId);
        public Task ManageStock(StockDTO stockToMagage);
    }
