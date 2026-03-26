using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShoppingCartMvcUI.Controllers
{
    [Authorize(Roles = nameof(Roles.Admin))]
    public class ReportsController : Controller
    {
        private readonly IReportRepository _reportRepository;

        public ReportsController(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }
        // GET: ReportsController
        public async Task<IActionResult> TopFiveSellingBooks(DateTime? sDate=null,DateTime? eDate=null)
        {
            try
            {
                DateTime startDate = sDate ?? DateTime.UtcNow.AddDays(-7);
                DateTime endDate = eDate ?? DateTime.UtcNow;

                var TopFiveSellingBooks = await _reportRepository.GetTopNSellingBooksByDate(startDate, endDate);

                var vm = new TopNSoldBooksVm(startDate, endDate, TopFiveSellingBooks);

                return View(vm);
            }
            catch (System.Exception ex)
            {
                TempData["errorMessage"] = "Something get wroung";
                return RedirectToAction("Index", "Home");
                
            }
        }

    }
}
