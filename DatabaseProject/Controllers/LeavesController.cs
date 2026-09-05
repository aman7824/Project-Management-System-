using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;

namespace DatabaseProject.Controllers;

public class LeavesController : Controller
{
    private readonly ILeaveRepository _leaveRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public LeavesController(ILeaveRepository leaveRepository, IEmployeeRepository employeeRepository)
    {
        _leaveRepository = leaveRepository;
        _employeeRepository = employeeRepository;
    }

    // GET: Leaves
    public async Task<IActionResult> Index()
    {
        var leaves = await _leaveRepository.GetAllAsync();
        return View(leaves);
    }

    // GET: Leaves/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var leave = await _leaveRepository.GetByIdAsync(id);
        if (leave == null)
        {
            return NotFound();
        }
        return View(leave);
    }

    // GET: Leaves/Create
    public async Task<IActionResult> Create()
    {
        var employees = await _employeeRepository.GetAllAsync();
        var fullTimeEmployees = employees.Where(e => e.EmployeeType == 'F');
        
        ViewBag.EmployeeList = new SelectList(fullTimeEmployees, "WorkerID", "FullName");
        return View();
    }

    // POST: Leaves/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Leave leave)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _leaveRepository.RequestLeaveAsync(leave);
                TempData["Success"] = "Leave request submitted successfully using sp_HR_RequestLeave stored procedure!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
        }

        var employees = await _employeeRepository.GetAllAsync();
        var fullTimeEmployees = employees.Where(e => e.EmployeeType == 'F');
        ViewBag.Employees = new SelectList(fullTimeEmployees, "WorkerID", "FullName", leave.WorkerID);
        
        return View(leave);
    }

    // GET: Leaves/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var leave = await _leaveRepository.GetByIdAsync(id);
        if (leave == null)
        {
            return NotFound();
        }
        return View(leave);
    }

    // POST: Leaves/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _leaveRepository.DeleteAsync(id);
            TempData["Success"] = "Leave request deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction(nameof(Delete), new { id });
        }
    }
}
