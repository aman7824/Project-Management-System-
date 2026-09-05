using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;

namespace DatabaseProject.Controllers;

public class EmployeesController : Controller
{
    private readonly IEmployeeRepository _repository;

    public EmployeesController(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    // GET: Employees
    public async Task<IActionResult> Index(string searchTerm, string filterType)
    {
        var employees = await _repository.GetAllAsync();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            employees = employees.Where(e =>
                (e.FirstName != null && e.FirstName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (e.LastName != null && e.LastName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) ||
                (e.Email != null && e.Email.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }

        if (!string.IsNullOrEmpty(filterType))
        {
            employees = employees.Where(e => e.EmployeeType?.ToString() == filterType);
        }

        ViewBag.SearchTerm = searchTerm;
        ViewBag.FilterType = filterType;

        return View(employees.ToList());
    }

    // GET: Employees/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        return View(employee);
    }

    // GET: Employees/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Employees/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employee employee)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _repository.CreateAsync(employee);
                TempData["Success"] = "Employee created successfully using stored procedures!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error creating employee: {ex.Message}";
                return View(employee);
            }
        }
        return View(employee);
    }

    // GET: Employees/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        return View(employee);
    }

    // POST: Employees/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Employee employee)
    {
        if (id != employee.WorkerID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _repository.UpdateAsync(employee);
                TempData["Success"] = "Employee updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error updating employee: {ex.Message}";
                return View(employee);
            }
        }
        return View(employee);
    }

    // GET: Employees/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var employee = await _repository.GetByIdAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        return View(employee);
    }

    // POST: Employees/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _repository.DeleteAsync(id);
            TempData["Success"] = "Employee deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error deleting employee: {ex.Message}";
            return RedirectToAction(nameof(Delete), new { id });
        }
    }
}
