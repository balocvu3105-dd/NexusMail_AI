# ADR-0007: Shared Domain Foundation

## Trạng thái
**Được duyệt (Accepted)** - 2026-08-03

## Ngữ cảnh (Context)
Dự án NexusMail-AI được xây dựng theo kiến trúc Clean Architecture kết hợp DDD (Domain-Driven Design). Khi phát triển Sprint 1 (Workspace Module), chúng ta nhận thấy cần một "xương sống" (Base Domain) vững chắc dùng chung cho toàn hệ thống (Shared.Domain và Shared.Common), bao gồm các khái niệm cốt lõi như `IDomainEvent`, `ValueObject`, `Guard`, `Result`, và `Error`.

Mục tiêu là tạo ra một nền tảng ổn định cho các Bounded Context tiếp theo (Email, AI, Automation, Identity) mà không bị phụ thuộc vòng (circular dependencies) hoặc ràng buộc (coupling) với các thư viện infrastructure.

## Quyết định (Decision)

1. **Phân tách `Shared.Common` và `Shared.Domain`**:
   - `Result` và `Error` được đặt trong `Shared.Common` vì chúng là kiểu trả về cross-cutting, được dùng ở cả tầng API, Application và Infrastructure.
   - Các thành phần thuần túy DDD (`ValueObject`, `IDomainEvent`, `Guard`) được đặt ở `Shared.Domain`.

2. **`IDomainEvent` KHÔNG phụ thuộc vào MediatR**:
   - `IDomainEvent` được thiết kế thuần túy với `EventId` và `OccurredOnUtc`. 
   - Không sử dụng `INotification` của MediatR ở tầng Domain để đảm bảo Domain layer không bị ô nhiễm (polluted) bởi thư viện bên thứ ba. Việc map từ Domain Event sang MediatR sẽ do Infrastructure (hoặc Application) đảm nhận.

3. **Tự viết `Guard` Clause thay vì dùng thư viện**:
   - Quyết định tự viết class `Guard` nhỏ gọn với `[CallerArgumentExpression]` thay vì cài `Ardalis.GuardClauses`.
   - Lý do: Hạn chế thêm dependency không cần thiết ở giai đoạn đầu khi nhu cầu chỉ là các phép kiểm tra cơ bản (Null, Empty, OutOfRange).

4. **Trì hoãn áp dụng `Specification` và `SoftDelete`**:
   - Ở giai đoạn hiện tại (Sprint 1.x), các query còn đơn giản. Việc áp dụng Specification Pattern hoặc SoftDelete (kèm Interceptors) sẽ tạo ra quá nhiều boilerplate code không cần thiết.
   - Chúng ta sẽ đưa vào khi hệ thống thực sự có nhu cầu query phức tạp (như Module Email) và sau khi đã hoàn thiện phần Identity (để xác định CreatedBy, UpdatedBy).

## Hệ quả (Consequences)
- **Tích cực**: Nền tảng Shared nhẹ, không phụ thuộc thư viện, không có dependency vòng, có thể dễ dàng kiểm thử bằng NetArchTest. Dễ dàng mở rộng trong các Sprint sau.
- **Tiêu cực**: Việc không dùng thư viện ngoài đòi hỏi chúng ta phải tự bảo trì các thành phần core (như Guard, ValueObject). Tuy nhiên, chi phí bảo trì này rất thấp.
