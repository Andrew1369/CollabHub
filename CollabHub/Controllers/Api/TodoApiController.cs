using CollabHub.Data;
using CollabHub.Dtos;
using CollabHub.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollabHub.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;

namespace CollabHub.Controllers.Api;

[Authorize]
[ApiController]
[Route("api/v1/todos")]
public class TodoApiController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly IHubContext<TodoHub> _hub;

    public TodoApiController(ApplicationDbContext db, IHubContext<TodoHub> hub)
    {
        _db = db;
        _hub = hub;
    }

    // GET /api/v1/todos
    [HttpGet]
    public async Task<ActionResult<IEnumerable<TodoItemDto>>> GetAll()
    {
        var items = await _db.TodoItems
            .OrderBy(t => t.Order)
            .ThenBy(t => t.Id)
            .Select(t => new TodoItemDto(
                t.Id,
                t.Text,
                t.Done,
                t.Order,
                t.CreatedAt
            ))
            .ToListAsync();

        return Ok(items);
    }

    public record TodoCreateRequest(string Text);
    public record TodoUpdateRequest(string? Text, bool? Done, int? Order);

    // POST /api/v1/todos
    [HttpPost]
    public async Task<ActionResult<TodoItemDto>> Create([FromBody] TodoCreateRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Text))
            return BadRequest("Text is required.");

        var maxOrder = await _db.TodoItems.MaxAsync(t => (int?)t.Order) ?? 0;

        var item = new TodoItem
        {
            Text = req.Text.Trim(),
            Done = false,
            Order = maxOrder + 1
        };

        _db.TodoItems.Add(item);
        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("TodosChanged");

        var dto = new TodoItemDto(item.Id, item.Text, item.Done, item.Order, item.CreatedAt);
        return CreatedAtAction(nameof(GetAll), new { id = item.Id }, dto);
    }

    // PUT /api/v1/todos/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TodoItemDto>> Update(int id, [FromBody] TodoUpdateRequest req)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return NotFound();

        if (req.Text != null)
            item.Text = req.Text;

        if (req.Done.HasValue)
            item.Done = req.Done.Value;

        if (req.Order.HasValue)
            item.Order = req.Order.Value;

        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("TodosChanged");

        var dto = new TodoItemDto(item.Id, item.Text, item.Done, item.Order, item.CreatedAt);
        return Ok(dto);
    }

    // PUT /api/v1/todos/reorder
    // приймає масив ID у новому порядку і оновлює Order
    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder([FromBody] int[] orderedIds)
    {
        if (orderedIds == null || orderedIds.Length == 0)
            return BadRequest("orderedIds required.");

        var items = await _db.TodoItems
            .Where(t => orderedIds.Contains(t.Id))
            .ToListAsync();

        var orderMap = orderedIds
            .Select((id, index) => new { id, order = index + 1 })
            .ToDictionary(x => x.id, x => x.order);

        foreach (var item in items)
        {
            if (orderMap.TryGetValue(item.Id, out var newOrder))
                item.Order = newOrder;
        }

        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("TodosChanged");
        return NoContent();
    }

    // DELETE /api/v1/todos/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.TodoItems.FindAsync(id);
        if (item == null) return NotFound();

        _db.TodoItems.Remove(item);
        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("TodosChanged");
        return NoContent();
    }
}
