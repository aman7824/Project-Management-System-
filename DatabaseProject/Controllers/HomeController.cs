using Microsoft.AspNetCore.Mvc;
using DatabaseProject.Repositories.Interfaces;

namespace DatabaseProject.Controllers;

public class HomeController : Controller
{
    private readonly IProjectRepository _projectRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITaskAssignmentRepository _taskAssignmentRepository;

    public HomeController(IProjectRepository projectRepository, IEmployeeRepository employeeRepository, ITaskAssignmentRepository taskAssignmentRepository)
    {
        _projectRepository = projectRepository;
        _employeeRepository = employeeRepository;
        _taskAssignmentRepository = taskAssignmentRepository;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var projects = await _projectRepository.GetAllAsync();
            var employees = await _employeeRepository.GetAllAsync();
            var tasks = await _taskAssignmentRepository.GetAllAsync();

            ViewBag.TotalProjects = projects.Count();
            ViewBag.ActiveProjects = projects.Count(p => p.ProjectStatus == "Active");
            ViewBag.TotalEmployees = employees.Count();
            ViewBag.TotalTasks = tasks.Count();

            return View();
        }
        catch
        {
            ViewBag.TotalProjects = 0;
            ViewBag.ActiveProjects = 0;
            ViewBag.TotalEmployees = 0;
            return View();
        }
    }
}