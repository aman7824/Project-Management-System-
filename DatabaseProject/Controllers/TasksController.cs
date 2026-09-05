using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;

namespace DatabaseProject.Controllers;

public class TasksController : Controller
{
    private readonly ITaskAssignmentRepository _taskRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly ITeamRepository _teamRepository;
    private readonly ICommentRepository _commentRepository;

    public TasksController(
        ITaskAssignmentRepository taskRepository,
        IProjectRepository projectRepository,
        ITeamRepository teamRepository,
        ICommentRepository commentRepository)
    {
        _taskRepository = taskRepository;
        _projectRepository = projectRepository;
        _teamRepository = teamRepository;
        _commentRepository = commentRepository;
    }

    // GET: Tasks
    public async Task<IActionResult> Index(string searchTerm)
    {
        var tasks = await _taskRepository.GetAllAsync();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            tasks = tasks.Where(t =>
                (t.TaskDescription != null && t.TaskDescription.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
            );
        }

        ViewBag.SearchTerm = searchTerm;

        return View(tasks.ToList());
    }

    // GET: Tasks/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        IEnumerable<Comment> comments = await _commentRepository.GetByAssignmentIdAsync(id);
        ViewBag.Comments = comments;

        return View(task);
    }

    // GET: Tasks/Create
    public async Task<IActionResult> Create(int? projectId)
    {
        var projects = await _projectRepository.GetAllAsync();
        var teams = await _teamRepository.GetAllAsync();

        ViewBag.Projects = new SelectList(projects, "ProjectID", "ProjectName", projectId);
        ViewBag.Teams = new SelectList(teams, "TeamID", "TeamName");

        var task = new TaskAssignment();
        if (projectId.HasValue)
        {
            task.ProjectID = projectId.Value;
        }

        return View(task);
    }

    // POST: Tasks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskAssignment task)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _taskRepository.CreateAsync(task);
                TempData["Success"] = "Task created successfully!";
                
                return RedirectToAction("Index", "Tasks");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
        }

        var projects = await _projectRepository.GetAllAsync();
        var teams = await _teamRepository.GetAllAsync();
        ViewBag.Projects = new SelectList(projects, "ProjectID", "ProjectName", task.ProjectID);
        ViewBag.Teams = new SelectList(teams, "TeamID", "TeamName", task.TeamID);

        return View(task);
    }

    // GET: Tasks/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }

        var projects = await _projectRepository.GetAllAsync();
        var teams = await _teamRepository.GetAllAsync();

        ViewBag.Projects = new SelectList(projects, "ProjectID", "ProjectName", task.ProjectID);
        ViewBag.Teams = new SelectList(teams, "TeamID", "TeamName", task.TeamID);

        return View(task);
    }

    // POST: Tasks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, TaskAssignment task)
    {
        if (id != task.AssignmentID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _taskRepository.UpdateAsync(task);
                TempData["Success"] = "Task updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
        }

        var projects = await _projectRepository.GetAllAsync();
        var teams = await _teamRepository.GetAllAsync();
        ViewBag.Projects = new SelectList(projects, "ProjectID", "ProjectName", task.ProjectID);
        ViewBag.Teams = new SelectList(teams, "TeamID", "TeamName", task.TeamID);

        return View(task);
    }

    // GET: Tasks/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var task = await _taskRepository.GetByIdAsync(id);
        if (task == null)
        {
            return NotFound();
        }
        return View(task);
    }

    // POST: Tasks/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _taskRepository.DeleteAsync(id);
            TempData["Success"] = "Task deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction(nameof(Delete), new { id });
        }
    }
}
