using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DatabaseProject.Models;
using DatabaseProject.Repositories.Interfaces;

namespace DatabaseProject.Controllers;

public class TeamsController : Controller
{
    private readonly ITeamRepository _teamRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ITeamMemberRepository _teamMemberRepository;

    public TeamsController(ITeamRepository teamRepository, IEmployeeRepository employeeRepository, ITeamMemberRepository teamMemberRepository)
    {
        _teamRepository = teamRepository;
        _employeeRepository = employeeRepository;
        _teamMemberRepository = teamMemberRepository;
    }

    // GET: Teams
    public async Task<IActionResult> Index()
    {
        var teams = await _teamRepository.GetAllAsync();
        return View(teams);
    }

    // GET: Teams/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        var members = await _teamMemberRepository.GetTeamMembersAsync(id);
        var allEmployees = await _employeeRepository.GetAllAsync();
        
        ViewBag.Members = members;
        ViewBag.AvailableEmployees = new SelectList(
            allEmployees.Where(e => !members.Any(m => m.WorkerID == e.WorkerID)),
            "WorkerID",
            "FullName"
        );
        ViewBag.EmployeeList = new SelectList(allEmployees, "WorkerID", "FullName", team.ManagerID);

        return View(team);
    }

    // GET: Teams/Create
    public async Task<IActionResult> Create()
    {
        var employees = await _employeeRepository.GetAllAsync();
        ViewBag.EmployeeList = new SelectList(employees, "WorkerID", "FullName");
        return View();
    }

    // POST: Teams/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Team team)
    {
        if (ModelState.IsValid)
        {
            try
            {
                team.TeamID = await _teamRepository.CreateAsync(team);
                TempData["Success"] = "Team created successfully using sp_Team_CreateNew stored procedure!";

                if (team.ManagerID != null)
                {
                    await _teamMemberRepository.AddMemberToTeamAsync(team.TeamID, team.ManagerID ?? 0);
                }
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                var employees = await _employeeRepository.GetAllAsync();
                ViewBag.EmployeeList = new SelectList(employees, "WorkerID", "FullName");
                return View(team);
            }
        }

        var allEmployees = await _employeeRepository.GetAllAsync();
        ViewBag.Employees = new SelectList(allEmployees, "WorkerID", "FullName");
        return View(team);
    }

    // GET: Teams/Edit/5
    public async Task<IActionResult> Edit(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
        {
            return NotFound();
        }

        var employees = await _employeeRepository.GetAllAsync();
        ViewBag.EmployeeList = new SelectList(employees, "WorkerID", "FullName", team.ManagerID);
        return View(team);
    }

    // POST: Teams/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Team team)
    {
        if (id != team.TeamID)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _teamRepository.UpdateAsync(team);
                TempData["Success"] = "Team updated successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error: {ex.Message}";
                var employees = await _employeeRepository.GetAllAsync();
                ViewBag.EmployeeList = new SelectList(employees, "WorkerID", "FullName", team.ManagerID);
                return View(team);
            }
        }

        var allEmployees = await _employeeRepository.GetAllAsync();
        ViewBag.EmployeeList = new SelectList(allEmployees, "WorkerID", "FullName", team.ManagerID);
        return View(team);
    }

    // GET: Teams/Delete/5
    public async Task<IActionResult> Delete(int id)
    {
        var team = await _teamRepository.GetByIdAsync(id);
        if (team == null)
        {
            return NotFound();
        }
        return View(team);
    }

    // POST: Teams/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _teamRepository.DeleteAsync(id);
            TempData["Success"] = "Team deleted successfully!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
            return RedirectToAction(nameof(Delete), new { id });
        }
    }

    // POST: Teams/UpdateManager
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateManager(int id, int? managerId)
    {
        try
        {
            var team = await _teamRepository.GetByIdAsync(id);
            if (team == null)
            {
                return NotFound();
            }

            team.ManagerID = managerId;
            team.ManagerID = await _teamRepository.UpdateAsync(team);
            
            TempData["Success"] = team.ManagerID.HasValue 
                ? "Team manager updated successfully!" 
                : "Team manager removed successfully!";

            if (team.ManagerID.HasValue)
            {
                await _teamMemberRepository.AddMemberToTeamAsync(team.TeamID, team.ManagerID ?? 0);
            }


        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Teams/AddMember/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddMember(int id, int workerId)
    {
        try
        {
            await _teamMemberRepository.AddMemberToTeamAsync(id, workerId);
            TempData["Success"] = "Member added to team successfully using sp_Team_AddMember stored procedure!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // POST: Teams/RemoveMember
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveMember(int teamId, int workerId)
    {
        try
        {
            await _teamMemberRepository.RemoveMemberFromTeamAsync(teamId, workerId);
            TempData["Success"] = "Member removed from team successfully!";
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Error: {ex.Message}";
        }

        return RedirectToAction(nameof(Details), new { id = teamId });
    }
}
