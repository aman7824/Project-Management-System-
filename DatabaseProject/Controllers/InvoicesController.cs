using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;

namespace DatabaseProject.Controllers;

public class InvoicesController : Controller
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IEmployeeRepository _employeeRepository;

    public InvoicesController(IInvoiceRepository invoiceRepository, IEmployeeRepository employeeRepository)
    {
        _invoiceRepository = invoiceRepository;
        _employeeRepository = employeeRepository;
    }

    // GET: Invoices
    public async Task<IActionResult> Index()
    {
        var invoices = await _invoiceRepository.GetAllAsync();
        return View(invoices);
    }

    // GET: Invoices/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var invoice = await _invoiceRepository.GetByIdAsync(id);
        if (invoice == null)
        {
            return NotFound();
        }
        return View(invoice);
    }

    // GET: Invoices/Create
    public async Task<IActionResult> Create()
    {
        var employees = await _employeeRepository.GetAllAsync();
        var hourlyEmployees = employees.Where(e => e.EmployeeType == 'H');
        
        ViewBag.HourlyEmployeeList = new SelectList(hourlyEmployees, "WorkerID", "FullName");
        return View();
    }

    // POST: Invoices/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int workerId, decimal hoursWorked)
    {
        if (hoursWorked <= 0)
        {
            TempData["Error"] = "Hours worked must be greater than zero.";
            
            var employees = await _employeeRepository.GetAllAsync();
            var hourlyEmployees = employees.Where(e => e.EmployeeType == 'H');
            ViewBag.HourlyEmployeeList = new SelectList(hourlyEmployees, "WorkerID", "FullName");
            
            return View();
        }

        try
        {
            await _invoiceRepository.GenerateInvoiceAsync(workerId, hoursWorked);
            TempData["Success"] = $"Invoice generated successfully for {hoursWorked} hours using sp_Accounting_GenerateInvoice stored procedure!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            
            var employees = await _employeeRepository.GetAllAsync();
            var hourlyEmployees = employees.Where(e => e.EmployeeType == 'H');
            ViewBag.HourlyEmployeeList = new SelectList(hourlyEmployees, "WorkerID", "FullName");
            
            return View();
        }
    }
}
