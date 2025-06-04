using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.API.Models;
using TaskManager.API.Repositories;

namespace TaskManager.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _repository;

        public TasksController(ITaskRepository repository)
        {
            _repository = repository;
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskItem>> GetById(int id)
        {
            var task = await _repository.GetByIdAsync(id);
            if (task == null) return NotFound();
            return task;
        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItemDto>>> GetAll()
        {
            var items = await _repository.GetAllAsync();
            return Ok(items.Select(t => new TaskItemDto { Id = t.Id, Title = t.Title, IsCompleted = t.IsCompleted }));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, TaskItemDto dto)
        {
            var existing = await _repository.GetByIdAsync(id);
            if (existing == null) return NotFound();

            existing.Title = dto.Title;
            existing.IsCompleted = dto.IsCompleted;
            await _repository.UpdateAsync(existing);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteAsync(id);
            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> Create(TaskItem task)
        {
            await _repository.AddAsync(task);
            return CreatedAtAction(nameof(GetById), new { id = task.Id }, task);
        }


    }
}