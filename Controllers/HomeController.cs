using Microsoft.AspNetCore.Mvc;
using NewsPortal.Data;
using NewsPortal.ViewModels;

namespace NewsPortal.Controllers;

public sealed class HomeController : Controller
{
    private readonly INewsRepository _repo;

    public HomeController(INewsRepository repo) => _repo = repo;

    public IActionResult Index()
    {
        var vm = new HomePageViewModel
        {
            MainCards = _repo.GetFeaturedForHome(limit: 6),
            Latest = _repo.GetLatest(limit: 12)
        };
        return View(vm);
    }
}
