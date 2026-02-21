namespace SkinPAI.API.Models.DTOs;

// ==================== Routine DTOs ====================
public record RoutineDto(
    Guid RoutineId,
    string RoutineName,
    string RoutineType,
    bool IsActive,
    List<RoutineStepDto> Steps,
    DateTime CreatedAt
);

public record RoutineStepDto(
    long StepId,
    int StepOrder,
    string StepName,
    string? Instructions,
    int? DurationMinutes,
    bool IsCompleted,
    ProductSummaryDto? Product
);

public record CreateRoutineRequest(
    string RoutineName,
    string RoutineType = "Morning",
    List<CreateRoutineStepRequest>? Steps = null
);

public record RoutineReminderDto(
    Guid ReminderId,
    Guid RoutineId,
    string RoutineName,
    TimeOnly ReminderTime,
    int[] DaysOfWeek,
    bool IsEnabled,
    bool SoundEnabled,
    bool VibrationEnabled
);
