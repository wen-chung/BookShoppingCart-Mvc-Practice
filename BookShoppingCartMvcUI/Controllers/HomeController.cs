using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BookShoppingCartMvcUI.Models;
using BookShoppingCartMvcUI.Models.DTOs;

namespace BookShoppingCartMvcUI.Controllers;

public class HomeController : Controller
{

    private readonly ILogger<HomeController> _logger;
    private readonly IHomeRepository _homeRepository;


    public HomeController(ILogger<HomeController> logger, IHomeRepository homeRepository)
    {
        _logger = logger;
        _homeRepository = homeRepository;

    }

    public async Task<IActionResult> Index(string sTerm = "", int genreid = 0)
    {
        IEnumerable<Book> books = await _homeRepository.GetBooksAsync(sTerm, genreid);
        IEnumerable<Genre> genres = await _homeRepository.Genres();

        BookDisplayModel bookDisplayModel = new BookDisplayModel
        {
            Books = books,
            Genres = genres,
            STerm = sTerm,
            GenreId = genreid
        };

        return View(bookDisplayModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
