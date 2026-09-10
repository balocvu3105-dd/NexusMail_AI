namespace NexusMail.Shared.Common;

/// <summary>
/// Marker interface cho phép Pipeline Behaviors kiểm tra trạng thái của một Result
/// mà không cần biết kiểu generic cụ thể.
/// Đặt tên IOperationResult để tránh conflict với Microsoft.AspNetCore.Http.IResult.
/// </summary>
public interface IOperationResult
{
    bool IsSuccess { get; }
    bool IsFailure { get; }
}
