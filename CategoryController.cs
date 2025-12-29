using Microsoft.AspNetCore.Mvc;
using NewsPortal.Data;
using NewsPortal.ViewModels;

namespace NewsPortal.Controllers;

public sealed class CategoryController : Controller
{
    private readonly INewsRepository _repo;

    public CategoryController(INewsRepository repo) => _repo = repo;

    [HttpGet]
    public IActionResult Index(string slug)
    {
        var category = _repo.GetCategoryBySlug(slug);
        if (category is null) return NotFound();

        var vm = new CategoryPageViewModel
        {
            Category = category,
            Items = _repo.GetByCategorySlug(slug, limit: 30)
        };

        return View(vm);
    }
}
