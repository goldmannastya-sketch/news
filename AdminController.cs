using Microsoft.AspNetCore.Mvc;
using NewsPortal.Data;
using NewsPortal.Models;

namespace NewsPortal.Controllers;

/// <summary>
/// Simple CRUD to show DB interaction.
/// In a real app you'd protect this with auth; for a student task it's enough.
/// </summary>
public sealed class AdminController : Controller
{
    private readonly INewsRepository _repo;

    public AdminController(INewsRepository repo) => _repo = repo;

    [HttpGet]
    public IActionResult Index()
    {
        var items = _repo.GetLatest(limit: 200);
        ViewBag.Categories = _repo.GetCategories();
        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = _repo.GetCategories();
        return View(new NewsItem { PublishedAt = DateTime.Now, Color = "#2D6CDF" });
    }

    [HttpPost]
    public IActionResult Create(NewsItem model)
    {
        ViewBag.Categories = _repo.GetCategories();

        if (!ModelState.IsValid)
            return View(model);

        if (model.PublishedAt == default)
            model.PublishedAt = DateTime.Now;

        var id = _repo.Create(model);
        return RedirectToAction(nameof(Edit), new { id });
    }

    [HttpGet]
    public IActionResult Edit(int id)
    {
        var item = _repo.GetById(id);
        if (item is null) return NotFound();

        ViewBag.Categories = _repo.GetCategories();
        return View(item);
    }

    [HttpPost]
    public IActionResult Edit(NewsItem model)
    {
        ViewBag.Categories = _repo.GetCategories();

        if (!ModelState.IsValid)
            return View(model);

        _repo.Update(model);
        TempData["Saved"] = "Сохранено";
        return RedirectToAction(nameof(Edit), new { id = model.Id });
    }

    [HttpPost]
    public IActionResult Delete(int id)
    {
        _repo.Delete(id);
        return RedirectToAction(nameof(Index));
    }
}
