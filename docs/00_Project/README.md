# README.md (00_Project)

> **NexusMail AI - Project Documentation Core**
> 
> Version: 1.0.0
> Status: Production
> Priority: HIGH
> Category: System Root

---

## 1. Giới Thiệu (Introduction)

Chào mừng bạn đến với thư mục **`00_Project`** - bộ não trung tâm của toàn bộ kho lưu trữ **NexusMail AI**.

Khác với các thư mục tài liệu thông thường, `00_Project` không chứa mã nguồn, không chứa thiết kế tính năng, mà chứa **các quy chuẩn, tiêu chuẩn, và bản đồ điều hướng** cho toàn bộ dự án. Bất kể bạn là một kỹ sư giàu kinh nghiệm, một Quản lý dự án, hay một AI Coding Agent, mọi hành trình đóng góp vào NexusMail AI đều bắt đầu tại đây.

Đây là nơi thiết lập "Luật chơi" (Rules of the Game) cho một quy trình phát triển theo chuẩn Enterprise.

---

## 2. Vì Sao Thư Mục Này Quan Trọng? (Why does this matter?)

Trong một dự án quy mô lớn với hàng trăm tài liệu và hàng chục module (như Backend, AI, Database, Search, Desktop), sự phân mảnh thông tin là rủi ro lớn nhất.

`00_Project` giải quyết vấn đề đó bằng cách:
1. **Cung cấp Bản đồ (Mapping)**: Để bạn biết tìm thông tin gì ở đâu.
2. **Thống nhất Thuật ngữ (Ubiquitous Language)**: Đảm bảo cả team Tech, Business và AI nói chung một ngôn ngữ.
3. **Định hình Tiêu chuẩn (Standardization)**: Code sinh ra phải giống nhau dù được viết bởi nhiều người (hoặc nhiều AI) khác nhau.
4. **Lưu trữ Lịch sử Quyết định (Decision Tracking)**: Ghi nhận TẠI SAO chúng ta chọn một công nghệ hay giải pháp thông qua ADR (Architecture Decision Records).

---

## 3. Cấu Trúc Thư Mục `00_Project` (Directory Structure)

Dưới đây là danh sách các tài liệu cốt lõi có trong thư mục này và thứ tự bạn nên tiếp cận chúng:

### Dành Cho Mọi Thành Viên (For Everyone)
- **`README.md`**: Tài liệu bạn đang đọc, giải thích tổng quan về thư mục này.
- **`DOCUMENT_INDEX.md`**: Danh mục toàn bộ các thư mục (từ `00` đến `26`) trong repository.
- **`GLOSSARY.md` / `TERMINOLOGY.md`**: Bộ từ điển thống nhất ngôn ngữ nghiệp vụ và kỹ thuật.
- **`PROJECT_CONTEXT.md`**: Tóm tắt ngắn gọn bối cảnh dự án để nắm bắt nhanh tình hình.
- **`CONTRIBUTING.md`**: Hướng dẫn cách tạo Pull Request, commit code, và các luồng làm việc cơ bản.

### Dành Cho Lập Trình Viên & Kiến Trúc Sư (For Engineering & Architecture)
- **`CODING_STANDARDS.md`**: Các tiêu chuẩn viết code C#, tổ chức file, sử dụng thư viện.
- **`NAMING_CONVENTIONS.md`**: Quy tắc đặt tên biến, class, endpoint, database table.
- **`ARCHITECTURE_DECISION_RECORDS.md`**: (ADR) Sổ tay ghi chép lại các quyết định kỹ thuật lớn (VD: Tại sao chọn Qdrant thay vì Pinecone).
- **`DEPENDENCY_GRAPH.md`**: Sơ đồ thể hiện sự phụ thuộc giữa các module trong hệ thống.
- **`DOCUMENT_MAP.md`**: Bản đồ chi tiết hơn Index, chỉ ra flow đọc tài liệu cho từng role.

### Dành Riêng Cho AI (For AI Agents)
- **`AI_ENTRYPOINT.md`**: Tài liệu **BẮT BUỘC ĐỌC** dành cho bất kỳ LLM / AI Assistant nào trước khi sinh mã nguồn. Nó chứa các System Prompt, giới hạn, và luật lệ (Guardrails) để đảm bảo AI sinh code đúng kiến trúc.
- **`DOCUMENT_CONVENTIONS.md`**: Hướng dẫn cho AI (và người) cách format, đặt tên, và duy trì các file Markdown theo chuẩn thống nhất.

---

## 4. Nguyên Tắc Cốt Lõi Khi Làm Việc Tại Đây (Core Principles)

1. **Read-First Policy**: Hãy đọc trước khi hỏi, và đọc trước khi code.
2. **Living Documentation**: Các tài liệu trong này không phải là đồ cổ. Nếu có một tiêu chuẩn mới được team thống nhất, nó phải được cập nhật vào đây ngay lập tức.
3. **Ubiquitous Language**: Không dùng hai từ khác nhau cho cùng một khái niệm. Nếu `GLOSSARY.md` gọi là "Workspace", không được dùng "Tenant" hay "Organization" trong code.
4. **Single Source of Truth**: `00_Project` là nguồn chân lý duy nhất về tiêu chuẩn kỹ thuật. Mọi tranh cãi về code style hay naming đều được giải quyết bằng cách chiếu theo các tài liệu trong này.

---

> END OF README
