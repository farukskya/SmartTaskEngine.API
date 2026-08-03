using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartTaskEngine.Application.Services;
using SmartTaskEngine.Domain.Entities;
using SmartTaskEngine.Infrastructure.Persistence;

namespace SmartTaskEngine.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly SmartTaskDbContext _context;
    private readonly ISmartPriorityCalculator _priorityCalculator;

    public TasksController(SmartTaskDbContext context, ISmartPriorityCalculator priorityCalculator)
    {
        _context = context;
        _priorityCalculator = priorityCalculator;
    }

    // 1. Tüm Görevleri Öncelik Skoruna Göre Sıralı Getir (GET api/tasks)
    [HttpGet]
    public async Task<IActionResult> GetAllTasks()
    {
        var tasks = await _context.Tasks
            .Include(t => t.Category)
            .OrderByDescending(t => t.PriorityScore) // En yüksek skor en üstte!
            .ToListAsync();

        return Ok(tasks);
    }

    // 2. Yeni Görev Ekle (POST api/tasks)
    [HttpPost]
    // 2. Yeni Görev Ekle (POST api/tasks)
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] TaskItem task)
    {
        try
        {
            // 1. Kategoriyi güvenli bir şekilde kontrol et ve yoksa oluştur
            var defaultCategory = await _context.Categories.FirstOrDefaultAsync();
            if (defaultCategory == null)
            {
                defaultCategory = new Category
                {
                    Name = "Genel",
                    ColorCode = "#FF5733"
                };
                _context.Categories.Add(defaultCategory);
                await _context.SaveChangesAsync();
            }

            // Kategori ID'sini atıyoruz
            task.CategoryId = defaultCategory.Id;
            task.Category = null; // Navigation property'yi null yapıyoruz ki EF Core ilişki çakışması yaşamasın

            // 2. Akıllı algoritma ile öncelik skoru hesaplanıyor
            task.PriorityScore = _priorityCalculator.CalculatePriorityScore(task);
            task.CreatedAt = DateTime.UtcNow;

            // 3. Veritabanına ekleme
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAllTasks), new { id = task.Id }, task);
        }
        catch (Exception ex)
        {
            // Hata detayını Swagger'da görebilmek için yakalıyoruz
            return StatusCode(500, new { Error = ex.Message, InnerError = ex.InnerException?.Message });
        }
    }

    // 3. Görevi Tamamlandı Olarak İşaretle (PUT api/tasks/{id}/complete)
    [HttpPut("{id}/complete")]
    public async Task<IActionResult> CompleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound("Görev bulunamadı.");

        task.IsCompleted = true;
        await _context.SaveChangesAsync();

        return Ok(new { Message = "Görev başarıyla tamamlandı!", Task = task });
    }
    // Görev Güncelleme
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(int id, [FromBody] TaskItem updatedTask)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        task.Title = updatedTask.Title;
        task.Description = updatedTask.Description;
        task.Priority = updatedTask.Priority;
        task.Difficulty = updatedTask.Difficulty;
        task.Deadline = updatedTask.Deadline;

        // Öncelik Skorunu Yeniden Hesapla
        task.PriorityScore = _priorityCalculator.CalculatePriorityScore(task);

        await _context.SaveChangesAsync();
        return Ok(task);
    }

    // Görev Silme
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}