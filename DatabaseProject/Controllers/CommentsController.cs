using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;

namespace DatabaseProject.Controllers;

public class CommentsController : Controller
{
    private readonly ICommentRepository _commentRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITaskAssignmentRepository _taskRepository;

    public CommentsController(
        ICommentRepository commentRepository,
        IEmployeeRepository employeeRepository,
        ITaskAssignmentRepository taskRepository)
    {
        _commentRepository = commentRepository;
        _employeeRepository = employeeRepository;
        _taskRepository = taskRepository;
    }

    // GET: Comments
    public async Task<IActionResult> Index()
    {
        var comments = await _commentRepository.GetAllAsync();
        return View(comments);
    }

    // GET: Comments/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            return NotFound();
        }
        return View(comment);
    }

    // GET: Comments/Create
    public async Task<IActionResult> Create(int? assignmentId)
    {
        var comment = new Comment();
        if (assignmentId.HasValue)
        {
            comment.AssignmentID = assignmentId.Value;
        }

        await LoadDropDownsAsync();
        return View(comment);
    }

    // POST: Comments/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Comment comment)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _commentRepository.CreateAsync(comment);
                TempData["Success"] = "Comment created successfully!";

                if (comment.AssignmentID.HasValue)
                {
                    return RedirectToAction("Details", "Tasks", new { id = comment.AssignmentID.Value });
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
        }

        await LoadDropDownsAsync();
        return View(comment);
    }

    // GET: Comments/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        if (comment == null)
        {
            return NotFound();
        }

        await LoadDropDownsAsync();
        return View(comment);
    }

    // POST: Comments/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Comment comment)
    {
        if (id != comment.CommentNumber)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _commentRepository.UpdateAsync(comment);
                TempData["Success"] = "Comment updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
            }
        }

        await LoadDropDownsAsync();
        return View(comment);
    }

    // GET: Comments/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var comment = await _commentRepository.GetByIdAsync(id);
        
        if (comment == null)
        {
            return NotFound();
        }
        var task = await _taskRepository.GetByIdAsync(comment.AssignmentID ?? 0);
        if (task != null)
        {
            ViewBag.Task = task;
        }
        return View(comment);
    }

    // POST: Comments/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _commentRepository.DeleteAsync(id);
            TempData["Success"] = "Comment deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction(nameof(Delete), new { id });
        }
    }

    private async Task LoadDropDownsAsync()
    {
        var employeeList = await _employeeRepository.GetAllAsync();
        var taskList = await _taskRepository.GetAllAsync();

        ViewBag.EmployeeList = new SelectList(employeeList, "WorkerID", "FullName");
        ViewBag.TaskList = new SelectList(taskList, "AssignmentID", "TaskDescription");
    }
}
