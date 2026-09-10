# AI_ENTRYPOINT.md

> **NexusMail AI - Artificial Intelligence Coding Agent Entrypoint**
> 
> Version: 1.0.0
> Status: Production
> Priority: CRITICAL (READ FIRST)
> Target Audience: AI Coding Agents, LLMs, AI Assistants

---

## 1. Mục Đích (Purpose)

Tài liệu này là **điểm bắt đầu (entrypoint)** bắt buộc dành cho mọi AI Coding Agent, LLM, hoặc AI Assistant khi tương tác với repository `NexusMail-AI`. 

Mục tiêu của tài liệu là:
- Thiết lập ngay lập tức bối cảnh (context) và giới hạn (boundaries) cho AI.
- Cung cấp "bản đồ tư duy" để AI định hướng cách đọc các tài liệu tiếp theo.
- Đảm bảo tính nhất quán tuyệt đối về mặt kiến trúc, naming conventions, và coding standards.
- Ngăn chặn việc AI tự "ảo giác" (hallucinate) ra các giải pháp không phù hợp với định hướng Enterprise.

Mọi AI khi được cấp quyền truy cập vào repository này **phải đọc và tuân thủ tuyệt đối** các quy tắc trong tài liệu này trước khi thực hiện bất kỳ thay đổi mã nguồn nào.

---

## 2. Thông Tin Dự Án (Project Identity)

- **Tên dự án**: NexusMail AI
- **Loại hình**: Nền tảng hệ điều hành Email tích hợp AI (AI Email Operating System).
- **Mô hình triển khai**: Microservices / Modular Monolith (Sẽ được định nghĩa rõ trong `04_Architecture`).
- **Ngôn ngữ & Nền tảng cốt lõi**:
  - Backend: C# / .NET 8 (hoặc mới nhất).
  - Cơ sở dữ liệu: PostgreSQL, Redis, Qdrant (Vector DB).
  - Frontend: (Được xác định trong `07_Frontend`).
  - Containerization: Docker & Kubernetes.
- **Tư tưởng thiết kế**: Domain-Driven Design (DDD), Clean Architecture, CQRS.

---

## 3. Thứ Tự Nạp Ngữ Cảnh (Context Loading Priority)

Để tránh việc nạp quá nhiều thông tin không cần thiết làm tràn Context Window, AI Agent cần nạp tài liệu theo thứ tự ưu tiên sau tùy thuộc vào loại tác vụ:

### 3.1. Tác vụ Khám phá & Đọc hiểu (Discovery)
1. Đọc `AI_ENTRYPOINT.md` (Tài liệu này).
2. Đọc `DOCUMENT_INDEX.md` để biết vị trí các tài liệu.
3. Đọc `DEPENDENCY_GRAPH.md` để hiểu sự phụ thuộc giữa các module.
4. Đọc `PROJECT_CONTEXT.md` để hiểu bối cảnh dự án.

### 3.2. Tác vụ Phân tích Nghiệp vụ (Business Analysis)
1. Đọc thư mục `02_Business` (`BUSINESS_REQUIREMENTS.md`, `USE_CASES.md`).
2. Đọc `01_Product\VISION.md` để nắm được tầm nhìn.
3. Đọc `03_Domain` tương ứng với phân hệ đang phân tích (VD: `EMAIL_DOMAIN.md`).

### 3.3. Tác vụ Code / Kiến trúc (Engineering)
1. Đọc `04_Architecture\CLEAN_ARCHITECTURE_GUIDE.md`.
2. Đọc `04_Architecture\CQRS_GUIDE.md`.
3. Đọc `00_Project\CODING_STANDARDS.md`.
4. Đọc `00_Project\NAMING_CONVENTIONS.md`.
5. Tham chiếu `03_Domain` liên quan để lấy Domain Model.

---

## 4. Hành Vi Bắt Buộc Của AI (Mandatory AI Behaviors)

### 4.1. Quy tắc "Think Before Act"
- AI **không bao giờ** được sinh mã nguồn ngay lập tức.
- Trước khi thực hiện thay đổi, AI phải:
  1. Phân tích yêu cầu.
  2. Truy xuất tài liệu tương ứng trong `docs/`.
  3. Lập Implementation Plan (Kế hoạch triển khai).
  4. Đợi người dùng (Human) phê duyệt (nếu có yêu cầu).

### 4.2. Quy tắc "No Hallucination"
- Nếu không chắc chắn về một kiến trúc, thư viện, hoặc quy trình: **HỎI**. 
- Không tự ý thêm thư viện bên thứ ba (3rd-party libraries) trừ khi nó được liệt kê trong `ARCHITECTURE_DECISION_RECORDS.md` hoặc được người dùng chỉ định.

### 4.3. Quy tắc "Clean Code & Clean Architecture"
- Mọi logic nghiệp vụ (Business Logic) **phải** nằm ở tầng Domain (`src/NexusMail.Domain`).
- Controller/API endpoints chỉ đóng vai trò điều phối (Orchestration) và gọi Command/Query thông qua MediatR.
- Giao tiếp giữa các Aggregate hoặc Module **phải** thông qua Domain Events hoặc Integration Events, không gọi trực tiếp Repository của Aggregate khác.

### 4.4. Quy tắc "Mã Nguồn Có Thể Thực Thi" (Executable Code)
- Bất kỳ mã nguồn nào AI sinh ra đều phải hoàn chỉnh, không dùng placeholder như `// TODO: Implement logic here` trừ khi được yêu cầu tạo khung (scaffolding).
- Phải luôn có bẫy lỗi (try/catch global) và Logging theo chuẩn cấu trúc.

---

## 5. Giới Hạn Công Nghệ (Technology Boundaries)

AI chỉ được phép sử dụng các công nghệ sau (trừ khi có lệnh ghi đè từ người dùng):

- **ORM**: Entity Framework Core. Không dùng Dapper trừ phi có yêu cầu tối ưu hiệu năng đặc thù (Performance.Tests chứng minh EF Core chậm).
- **Validation**: FluentValidation. Không dùng Data Annotations trong Entity.
- **Mapping**: Mapster hoặc AutoMapper.
- **Testing**: xUnit, Moq, FluentAssertions, Testcontainers.
- **Logging**: Serilog (với Elasticsearch/Logstash/Kibana hoặc Seq).
- **In-process Messaging**: MediatR.
- **Message Broker**: RabbitMQ hoặc Kafka (TBD trong ADR).

---

## 6. Hướng Dẫn Tương Tác Cụ Thể (Specific Interaction Guidelines)

### 6.1. Xử lý Lỗi (Error Handling)
Khi gặp lỗi biên dịch hoặc runtime, AI phải:
1. Đọc nội dung lỗi kỹ lưỡng.
2. Không đoán mò. Sử dụng `grep_search` hoặc `view_file` để kiểm tra trực tiếp mã nguồn.
3. Đề xuất nguyên nhân gốc rễ (Root Cause) trước khi đưa ra đoạn code sửa lỗi.

### 6.2. Tạo mới Domain Model
Khi được yêu cầu tạo một Domain mới:
1. Luôn kế thừa từ `Entity` hoặc `AggregateRoot` base classes.
2. Thuộc tính (Properties) phải có `private set` (hoặc `init`).
3. Mọi thay đổi trạng thái phải đi qua các phương thức (Methods) có ý nghĩa nghiệp vụ (Ví dụ: thay vì `email.Status = Read`, dùng `email.MarkAsRead()`).
4. Nếu trạng thái thay đổi quan trọng, sinh ra Domain Event (Ví dụ: `AddDomainEvent(new EmailMarkedAsReadEvent(Id))`).

### 6.3. Tương tác với File System
- Khi cần tìm kiếm, AI ưu tiên sử dụng `grep_search`.
- Khi cần chỉnh sửa file, sử dụng `write_to_file` hoặc các công cụ sửa đổi theo block (multi_replace_file_content). Không dùng regex phức tạp qua bash script.
- Mọi tài liệu thiết kế đều được lưu tập trung tại `NexusMail-AI/docs`.

---

## 7. Quy Trình Cập Nhật Tài Liệu (Documentation Update Protocol)

Là một AI Agent, bạn có trách nhiệm giữ cho tài liệu luôn cập nhật.
Nếu quá trình lập trình (coding) phát sinh một quyết định kiến trúc mới:
1. Nhắc nhở người dùng tạo mới một ADR (Architecture Decision Record) trong `00_Project/ARCHITECTURE_DECISION_RECORDS.md`.
2. Cập nhật `CHANGELOG.md` nếu có thay đổi phá vỡ (Breaking Changes).
3. Cập nhật Domain Markdown liên quan trong `03_Domain` nếu có thay đổi về thuật ngữ hoặc logic.

---

> END OF AI ENTRYPOINT
> *Mọi Agent đọc tới dòng này đã được nạp bối cảnh hệ thống thành công.*
