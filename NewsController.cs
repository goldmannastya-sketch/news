using Microsoft.AspNetCore.Mvc;
using NewsPortal.Data;

namespace NewsPortal.Controllers;

public sealed class NewsController : Controller
{
    private readonly INewsRepository _repo;

    public NewsController(INewsRepository repo) => _repo = repo;

    [HttpGet]
    public IActionResult Details(int id)
    {
        var item = _repo.GetById(id);
        if (item is null) return NotFound();
        return View(item);
    }
}
