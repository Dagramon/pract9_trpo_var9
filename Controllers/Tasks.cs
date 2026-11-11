using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using pract8_trpo.Models;
using System.Collections.ObjectModel;

using Task = pract8_trpo.Models.Task;

namespace trpo8.Controllers
{
    [Route("api/v1/tasks")]
    [ApiController]
    [Authorize]
    public class Tasks: ControllerBase
    {
        private readonly KaramovDbContext _db;
        public Tasks(KaramovDbContext db) => _db = db;

        private string CurrentLogin => User.Identity?.Name ?? string.Empty;
        private bool IsAdmin => User.IsInRole("Admin");

        public record TaskCreateRequest(string Title, string Description);
        public record TaskUpdateRequest(string Title, string Description, bool IsCompleted);
        public record TaskResponse(int Id, string Title, string Description,
                              string OwnerLogin, bool IsCompleted);
        private static TaskResponse Response(Task t) =>
            new(t.Id, t.Title, t.Description ?? "", t.OwnerLogin, t.IsCompleted);
        [HttpGet]
        public IActionResult GetOwn()
        {
            var list = _db.Tasks
                .Where(t => t.OwnerLogin == CurrentLogin)
                .Select(t => new TaskResponse(t.Id, t.Title, t.Description ?? "",
                                         t.OwnerLogin, t.IsCompleted))
                .ToList();
            return Ok(list);
        }
        [HttpGet("all")]
        [Authorize(Roles = "Admin")]
        public IActionResult GetAll()
        {
            var list = _db.Tasks
                .Select(t => new TaskResponse(t.Id, t.Title, t.Description ?? "",
                                         t.OwnerLogin, t.IsCompleted))
                .ToList();
            return Ok(list);
        }
        [HttpGet("{id:int}")]
        public IActionResult GetById(int id)
        {
            var t = _db.Tasks.FirstOrDefault(x => x.Id == id);
            if (t == null) return NotFound();
            if (!IsAdmin && t.OwnerLogin != CurrentLogin) return Forbid();
            return Ok(Response(t));
        }
        [HttpPost]
        public IActionResult Create(TaskCreateRequest req)
        {
            var t = new Task
            {
                Title = req.Title,
                Description = req.Description,
                OwnerLogin = CurrentLogin,
                IsCompleted = false
            };
            _db.Tasks.Add(t);
            _db.SaveChanges();
            return CreatedAtAction(nameof(GetById), new { id = t.Id }, Response(t));
        }
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, TaskUpdateRequest req)
        {
            var t = _db.Tasks.FirstOrDefault(x => x.Id == id);
            if (t == null) return NotFound();
            if (!IsAdmin && t.OwnerLogin != CurrentLogin) return Forbid();

            t.Title = req.Title;
            t.Description = req.Description;
            t.IsCompleted = req.IsCompleted;

            _db.SaveChanges();
            return Ok(Response(t));
        }
        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            var t = _db.Tasks.FirstOrDefault(x => x.Id == id);
            if (t == null) return NotFound();
            if (!IsAdmin && t.OwnerLogin != CurrentLogin) return Forbid();

            _db.Tasks.Remove(t);
            _db.SaveChanges();
            return NoContent();
        }
    }
}
