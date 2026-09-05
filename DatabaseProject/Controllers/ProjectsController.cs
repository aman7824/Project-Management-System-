using Microsoft.AspNetCore.Mvc;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;

namespace DatabaseProject.Controllers;

public class ProjectsController : Controller
{
    private readonly IProjectRepository _projectRepository;
    private readonly IProfitRepository _profitRepository;

    public ProjectsController(IProjectRepository projectRepository, IProfitRepository profitRepository)
    {
        _projectRepository = projectRepository;
        _profitRepository = profitRepository;
    }

    // GET: Projects
    public async Task<IActionResult> Index(string searchTerm, string filterStatus)
    {
        var projects = await _projectRepository.GetAllAsync();

        // Apply search filter
        if (!string.IsNullOrEmpty(searchTerm))
        {
            projects = projects.Where(p =>
                (p.ProjectName != null && p.ProjectName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (p.ProjectDescription != null && p.ProjectDescription.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }

        // Apply status filter
        if (!string.IsNullOrEmpty(filterStatus))
        {
            projects = projects.Where(p => p.ProjectStatus == filterStatus);
        }

        ViewBag.SearchTerm = searchTerm;
        ViewBag.FilterStatus = filterStatus;

        return View(projects.ToList());
    }

    // GET: Projects/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }

        // Try to get profit data for this project
        var projectProfit = await _profitRepository.GetByProjectIdAsync(id);
        
        ViewBag.ProjectProfit = projectProfit;

        return View(project);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // POST: Projects/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project project)
    {
        if (ModelState.IsValid)
        {
            await _projectRepository.CreateAsync(project);
            TempData["Success"] = "Project created successfully!";
            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }

    // GET: Projects/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }

    // POST: Projects/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Project project)
    {
        if (id != project.ProjectID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _projectRepository.UpdateAsync(project);
                TempData["Success"] = "Project updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating project: {ex.Message}";
                return View(project);
            }
        }
        return View(project);
    }

    // GET: Projects/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var project = await _projectRepository.GetByIdAsync(id);
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }

    // POST: Projects/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            int status = await _projectRepository.DeleteAsync(id);

            if (status == 1)
                TempData["Error"] = "Error encountered while deleting Comments related to TaskAssignments for this project.";
            else if (status == 2)
                TempData["Error"] = "Error encountered while deleting TaskAssignments for this project.";
            else if (status == 3)
                TempData["Error"] = "Completed projects can not be deleted.";
            else
                TempData["Success"] = "Project deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error deleting project: {ex.Message}";
            return RedirectToAction(nameof(Delete), new { id });
        }
    }

    // POST: Projects/Complete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(int id)
    {
        try
        {
            var success = await _projectRepository.CompleteProjectAsync(id);
            if (success)
            {
                TempData["Success"] = "Project completed successfully using sp_Project_Complete stored procedure!";
            }
            else
            {
                TempData["Error"] = "Failed to complete project. Please try again.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Projects/AddExpense/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddExpense(int id, decimal expenseAmount)
    {
        if (expenseAmount <= 0)
        {
            TempData["Error"] = "Expense amount must be greater than zero.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            var success = await _projectRepository.AddExpensesAsync(id, expenseAmount);
            if (success)
            {
                TempData["Success"] = $"Added ${expenseAmount:N2} to project expenses using sp_Project_AddExpenses stored procedure!";
            }
            else
            {
                TempData["Error"] = "Failed to add expenses. Please try again.";
            }
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Projects/AddEarnings/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddEarnings(int id, decimal earningsAmount)
    {
        if (earningsAmount <= 0)
        {
            TempData["Error"] = "Earnings amount must be greater than zero.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            await _projectRepository.AddEarningsAsync(id, earningsAmount);
            TempData["Success"] = $"Added {earningsAmount:C} to project earnings!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}
