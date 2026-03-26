using System;

namespace BookShoppingCartMvcUI.Repositories;

public interface IHomeRepository
{
    Task<IEnumerable<Book>> GetBooksAsync(string sTerm = "", int genreid = 0);
    Task<IEnumerable<Genre>> Genres();
}
