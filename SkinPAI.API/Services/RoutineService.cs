using Microsoft.EntityFrameworkCore;
using SkinPAI.API.Models.DTOs;
using SkinPAI.API.Models.Entities;
using SkinPAI.API.Repositories;
using System.Text.Json;

namespace SkinPAI.API.Services;

public interface IRoutineService
{
    Task<List<RoutineDto>> GetUserRoutinesAsync(Guid userId);
    Task<RoutineDto?> GetRoutineByIdAsync(Guid routineId, Guid userId);
    Task<RoutineDto> CreateRoutineAsync(Guid userId, CreateRoutineRequest request);
    Task<RoutineDto> UpdateRoutineAsync(Guid routineId, Guid userId, UpdateRoutineRequest request);
    Task<bool> DeleteRoutineAsync(Guid routineId, Guid userId);
    Task<RoutineDto> AddStepAsync(Guid routineId, Guid userId, CreateRoutineStepRequest request);
    Task<bool> RemoveStepAsync(Guid routineId, long stepId, Guid userId);
    Task<RoutineCompletionDto> CompleteRoutineAsync(Guid userId, CompleteRoutineRequest request);
    Task<List<RoutineCompletionDto>> GetCompletionHistoryAsync(Guid userId, int days = 30);
    Task<List<RoutineReminderDto>> GetRemindersAsync(Guid userId);
    Task<RoutineReminderDto> CreateReminderAsync(Guid userId, CreateReminderRequest request);
    Task<RoutineReminderDto> UpdateReminderAsync(Guid reminderId, Guid userId, UpdateReminderRequest request);
    Task<bool> DeleteReminderAsync(Guid reminderId, Guid userId);
}

public class RoutineService : IRoutineService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RoutineService> _logger;

    public RoutineService(IUnitOfWork unitOfWork, ILogger<RoutineService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<List<RoutineDto>> GetUserRoutinesAsync(Guid userId)
    {
        var routines = await _unitOfWork.UserRoutines.Query()
            .Include(r => r.RoutineSteps)
                .ThenInclude(s => s.Product)
                    .ThenInclude(p => p!.Brand)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.RoutineType)
            .ThenBy(r => r.RoutineName)
            .ToListAsync();

        return routines.Select(MapToRoutineDto).ToList();
    }

    public async Task<RoutineDto?> GetRoutineByIdAsync(Guid routineId, Guid userId)
    {
        var routine = await _unitOfWork.UserRoutines.Query()
            .Include(r => r.RoutineSteps)
                .ThenInclude(s => s.Product)
                    .ThenInclude(p => p!.Brand)
            .FirstOrDefaultAsync(r => r.RoutineId == routineId && r.UserId == userId);

        return routine != null ? MapToRoutineDto(routine) : null;
    }

    public async Task<RoutineDto> CreateRoutineAsync(Guid userId, CreateRoutineRequest request)
    {
        var routine = new UserRoutine
        {
            UserId = userId,
            RoutineName = request.RoutineName,
            RoutineType = request.RoutineType
        };

        await _unitOfWork.UserRoutines.AddAsync(routine);

        if (request.Steps != null)
        {
            foreach (var stepRequest in request.Steps)
            {
                var step = new RoutineStep
                {
                    RoutineId = routine.RoutineId,
                    StepOrder = stepRequest.StepOrder,
                    StepName = stepRequest.StepName,
                    Instructions = stepRequest.Instructions,
                    DurationMinutes = stepRequest.DurationMinutes,
                    ProductId = stepRequest.ProductId
                };
                await _unitOfWork.RoutineSteps.AddAsync(step);
            }
        }

        await _unitOfWork.SaveChangesAsync();

        return await GetRoutineByIdAsync(routine.RoutineId, userId) ?? throw new Exception("Failed to create routine");
    }

    public async Task<RoutineDto> UpdateRoutineAsync(Guid routineId, Guid userId, UpdateRoutineRequest request)
    {
        var routine = await _unitOfWork.UserRoutines.FirstOrDefaultAsync(r => r.RoutineId == routineId && r.UserId == userId);
        if (routine == null)
            throw new KeyNotFoundException("Routine not found");

        if (!string.IsNullOrEmpty(request.RoutineName)) routine.RoutineName = request.RoutineName;
        if (!string.IsNullOrEmpty(request.RoutineType)) routine.RoutineType = request.RoutineType;
        if (request.IsActive.HasValue) routine.IsActive = request.IsActive.Value;

        routine.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.UserRoutines.Update(routine);
        await _unitOfWork.SaveChangesAsync();

        return await GetRoutineByIdAsync(routineId, userId) ?? throw new Exception("Failed to update routine");
    }

    public async Task<bool> DeleteRoutineAsync(Guid routineId, Guid userId)
    {
        var routine = await _unitOfWork.UserRoutines.FirstOrDefaultAsync(r => r.RoutineId == routineId && r.UserId == userId);
        if (routine == null) return false;

        _unitOfWork.UserRoutines.Remove(routine);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<RoutineDto> AddStepAsync(Guid routineId, Guid userId, CreateRoutineStepRequest request)
    {
        var routine = await _unitOfWork.UserRoutines.FirstOrDefaultAsync(r => r.RoutineId == routineId && r.UserId == userId);
        if (routine == null)
            throw new KeyNotFoundException("Routine not found");

        var step = new RoutineStep
        {
            RoutineId = routineId,
            StepOrder = request.StepOrder,
            StepName = request.StepName,
            Instructions = request.Instructions,
            DurationMinutes = request.DurationMinutes,
            ProductId = request.ProductId
        };

        await _unitOfWork.RoutineSteps.AddAsync(step);
        await _unitOfWork.SaveChangesAsync();

        return await GetRoutineByIdAsync(routineId, userId) ?? throw new Exception("Failed to add step");
    }

    public async Task<bool> RemoveStepAsync(Guid routineId, long stepId, Guid userId)
    {
        var routine = await _unitOfWork.UserRoutines.FirstOrDefaultAsync(r => r.RoutineId == routineId && r.UserId == userId);
        if (routine == null) return false;

        var step = await _unitOfWork.RoutineSteps.FirstOrDefaultAsync(s => s.StepId == stepId && s.RoutineId == routineId);
        if (step == null) return false;

        _unitOfWork.RoutineSteps.Remove(step);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task<RoutineCompletionDto> CompleteRoutineAsync(Guid userId, CompleteRoutineRequest request)
    {
        var routine = await _unitOfWork.UserRoutines.FirstOrDefaultAsync(r => r.RoutineId == request.RoutineId && r.UserId == userId);
        if (routine == null)
            throw new KeyNotFoundException("Routine not found");

        var completion = new RoutineCompletion
        {
            RoutineId = request.RoutineId,
            UserId = userId,
            CompletionDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Notes = request.Notes
        };

        await _unitOfWork.RoutineCompletions.AddAsync(completion);
        await _unitOfWork.SaveChangesAsync();

        return new RoutineCompletionDto(
            completion.CompletionId,
            completion.RoutineId,
            routine.RoutineName,
            completion.CompletionDate,
            completion.CompletedAt,
            completion.Notes
        );
    }

    public async Task<List<RoutineCompletionDto>> GetCompletionHistoryAsync(Guid userId, int days = 30)
    {
        var startDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-days));

        var completions = await _unitOfWork.RoutineCompletions.Query()
            .Include(c => c.Routine)
            .Where(c => c.UserId == userId && c.CompletionDate >= startDate)
            .OrderByDescending(c => c.CompletedAt)
            .ToListAsync();

        return completions.Select(c => new RoutineCompletionDto(
            c.CompletionId,
            c.RoutineId,
            c.Routine.RoutineName,
            c.CompletionDate,
            c.CompletedAt,
            c.Notes
        )).ToList();
    }

    public async Task<List<RoutineReminderDto>> GetRemindersAsync(Guid userId)
    {
        var reminders = await _unitOfWork.RoutineReminders.Query()
            .Include(r => r.Routine)
            .Where(r => r.UserId == userId)
            .OrderBy(r => r.ReminderTime)
            .ToListAsync();

        return reminders.Select(r => new RoutineReminderDto(
            r.ReminderId,
            r.RoutineId,
            r.Routine.RoutineName,
            r.ReminderTime,
            JsonSerializer.Deserialize<int[]>(r.DaysOfWeek) ?? new[] { 0, 1, 2, 3, 4, 5, 6 },
            r.IsEnabled,
            r.SoundEnabled,
            r.VibrationEnabled
        )).ToList();
    }

    public async Task<RoutineReminderDto> CreateReminderAsync(Guid userId, CreateReminderRequest request)
    {
        var routine = await _unitOfWork.UserRoutines.FirstOrDefaultAsync(r => r.RoutineId == request.RoutineId && r.UserId == userId);
        if (routine == null)
            throw new KeyNotFoundException("Routine not found");

        var reminder = new RoutineReminder
        {
            UserId = userId,
            RoutineId = request.RoutineId,
            ReminderTime = request.ReminderTime,
            DaysOfWeek = JsonSerializer.Serialize(request.DaysOfWeek),
            SoundEnabled = request.SoundEnabled,
            VibrationEnabled = request.VibrationEnabled
        };

        await _unitOfWork.RoutineReminders.AddAsync(reminder);
        await _unitOfWork.SaveChangesAsync();

        return new RoutineReminderDto(
            reminder.ReminderId,
            reminder.RoutineId,
            routine.RoutineName,
            reminder.ReminderTime,
            request.DaysOfWeek,
            reminder.IsEnabled,
            reminder.SoundEnabled,
            reminder.VibrationEnabled
        );
    }

    public async Task<RoutineReminderDto> UpdateReminderAsync(Guid reminderId, Guid userId, UpdateReminderRequest request)
    {
        var reminder = await _unitOfWork.RoutineReminders.Query()
            .Include(r => r.Routine)
            .FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.UserId == userId);

        if (reminder == null)
            throw new KeyNotFoundException("Reminder not found");

        if (request.ReminderTime.HasValue) reminder.ReminderTime = request.ReminderTime.Value;
        if (request.DaysOfWeek != null) reminder.DaysOfWeek = JsonSerializer.Serialize(request.DaysOfWeek);
        if (request.IsEnabled.HasValue) reminder.IsEnabled = request.IsEnabled.Value;
        if (request.SoundEnabled.HasValue) reminder.SoundEnabled = request.SoundEnabled.Value;
        if (request.VibrationEnabled.HasValue) reminder.VibrationEnabled = request.VibrationEnabled.Value;

        _unitOfWork.RoutineReminders.Update(reminder);
        await _unitOfWork.SaveChangesAsync();

        return new RoutineReminderDto(
            reminder.ReminderId,
            reminder.RoutineId,
            reminder.Routine.RoutineName,
            reminder.ReminderTime,
            JsonSerializer.Deserialize<int[]>(reminder.DaysOfWeek) ?? new[] { 0, 1, 2, 3, 4, 5, 6 },
            reminder.IsEnabled,
            reminder.SoundEnabled,
            reminder.VibrationEnabled
        );
    }

    public async Task<bool> DeleteReminderAsync(Guid reminderId, Guid userId)
    {
        var reminder = await _unitOfWork.RoutineReminders.FirstOrDefaultAsync(r => r.ReminderId == reminderId && r.UserId == userId);
        if (reminder == null) return false;

        _unitOfWork.RoutineReminders.Remove(reminder);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private RoutineDto MapToRoutineDto(UserRoutine routine)
    {
        return new RoutineDto(
            routine.RoutineId,
            routine.RoutineName,
            routine.RoutineType,
            routine.IsActive,
            routine.RoutineSteps.OrderBy(s => s.StepOrder).Select(s => new RoutineStepDto(
                s.StepId,
                s.StepOrder,
                s.StepName,
                s.Instructions,
                s.DurationMinutes,
                s.IsCompleted,
                s.Product != null ? new ProductSummaryDto(
                    s.Product.ProductId,
                    s.Product.ProductName,
                    s.Product.ProductImageUrl,
                    s.Product.Price,
                    s.Product.OriginalPrice,
                    s.Product.DiscountPercent ?? 0m,
                    s.Product.AverageRating ?? 0m,
                    s.Product.TotalReviews,
                    s.Product.InStock,
                    s.Product.Brand?.BrandName ?? ""
                ) : null
            )).ToList(),
            routine.CreatedAt
        );
    }
}
