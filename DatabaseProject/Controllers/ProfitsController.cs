using Microsoft.AspNetCore.Mvc;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;

namespace DatabaseProject.Controllers;

public class ProfitsController : Controller
{
    private readonly IProfitRepository _profitRepository;
    private readonly IProjectRepository _projectRepository;

    public ProfitsController(IProfitRepository profitRepository, IProjectRepository projectRepository)
    {
        _profitRepository = profitRepository;
        _projectRepository = projectRepository;
    }

    // GET: Profits
    public async Task<IActionResult> Index()
    {
        var profits = await _profitRepository.GetAllAsync();
        return View(profits);
    }

    // GET: Profits/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var profit = await _profitRepository.GetByIdAsync(id);
        if (profit == null)
        {
            return NotFound();
        }

        var project = await _projectRepository.GetByIdAsync(profit.ProjectID);
        ViewBag.Project = project;

        return View(profit);
    }
}
