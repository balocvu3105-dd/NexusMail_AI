# DOCUMENT_INDEX.md

> **NexusMail AI - Master Document Index**
> 
> Version: 1.0.0
> Status: Production
> Priority: HIGH
> Target Audience: Entire Team & AI Agents

---

## 1. Giới Thiệu (Introduction)

`DOCUMENT_INDEX.md` là danh mục bách khoa toàn thư của toàn bộ kho tài liệu (Documentation Repository) của dự án **NexusMail AI**.
Mục tiêu của tài liệu này là giúp bất kỳ thành viên nào (Human) hoặc AI Agent nào khi tham gia dự án có thể lập tức tra cứu và tìm thấy tài liệu mình cần trong vòng dưới 30 giây.

Toàn bộ tài liệu được cấu trúc theo triết lý **"Enterprise Product Repository"**, nghĩa là không chỉ chứa code mà chứa toàn bộ Vòng đời Sản phẩm (Product Lifecycle).

---

## 2. Cấu Trúc Tổng Thể (Master Structure)

Tài liệu được chia thành **27 thư mục cốt lõi**, trải dài từ lúc phôi thai ý tưởng sản phẩm cho đến lúc vận hành trên Production.

### Phase: Nền Tảng & Sản Phẩm (Foundation & Product)

- **`00_Project/`**: Thư mục gốc chứa các quy ước chung, bách khoa toàn thư, bảng thuật ngữ, tiêu chuẩn code và các quyết định kiến trúc quan trọng (ADR). Đây là nơi mọi người phải đọc đầu tiên.
- **`01_Product/`**: Chứa tầm nhìn (Vision), sứ mệnh (Mission), tài liệu yêu cầu sản phẩm (PRD), kế hoạch phát hành (Release Plan) và các mục tiêu kinh doanh. Dành cho Product Manager.
- **`02_Business/`**: Tập trung vào việc phân tích nghiệp vụ, định nghĩa User Personas, User Stories, Use Cases, và các quy tắc nghiệp vụ (Business Rules). Dành cho Business Analyst.
- **`03_Domain/`**: "Trái tim" của hệ thống. Chứa các bản thiết kế chi tiết cho từng Domain/Bounded Context (VD: Email, User, Workspace, AI). Phục vụ cho Domain-Driven Design.

### Phase: Thiết Kế & Kiến Trúc (Design & Architecture)

- **`04_Architecture/`**: Chứa các sơ đồ C4, Component Diagram, sơ đồ luồng sự kiện (Event Flow), định nghĩa CQRS, Clean Architecture và chiến lược Scale.
- **`05_Backend/`**: Thiết kế chi tiết cho phía Backend (API Contracts, Background Workers, Scheduler, Middleware).
- **`06_AI/`**: Chứa thư viện Prompt, cơ chế phân loại AI, chiến lược RAG, Prompt Versioning và Cost Optimization.
- **`07_Frontend/`**: Thiết kế UI/UX, State Management, Component Library, và Micro-frontend strategy (nếu có).
- **`08_Desktop/`**: Thiết kế cho ứng dụng Desktop (Windows/macOS), bao gồm giao tiếp IPC, Offline Mode, Local Database.

### Phase: Dữ Liệu & Tích Hợp (Data & Integration)

- **`09_Database/`**: Chuẩn hóa cơ sở dữ liệu, schema, Migration policies, ERD, Indexing strategies cho PostgreSQL/Redis.
- **`10_Search/`**: Kiến trúc Search Engine (Hybrid Search, Semantic Search), cấu hình Vector DB (Qdrant).
- **`11_Notification/`**: Hệ thống Real-time notification (SignalR/WebSockets), Push Notifications, Email Alerts.
- **`12_Automation/`**: Thiết kế Rule Engine (If-This-Then-That), Event Triggers, Workflow Execution.
- **`13_API/`**: Các nguyên tắc thiết kế REST/GraphQL/gRPC, Versioning, Pagination, Rate Limiting.

### Phase: Chất Lượng & Bảo Mật (Quality & Security)

- **`14_Security/`**: Cơ chế Auth (OAuth2, JWT, OpenID), RBAC, Mã hóa dữ liệu (Encryption at rest/transit), Audit Logs.
- **`15_Testing/`**: Chiến lược kiểm thử (Unit, Integration, E2E, Performance, Security Testing), Test Data Builders.

### Phase: Vận Hành & Hạ Tầng (Operations & Infrastructure)

- **`16_DevOps/`**: Git Workflow, CI/CD Pipelines (GitHub Actions), Containerization guidelines.
- **`17_Deployment/`**: Sơ đồ triển khai các môi trường (Dev, Staging, Prod), Infrastructure as Code (Terraform/Bicep).
- **`18_Operations/`**: Cấu hình Logging (Serilog/ELK), Monitoring (Prometheus/Grafana), Tracing (OpenTelemetry), Runbooks.

### Phase: Mở Rộng & Tương Lai (Expansion & Future)

- **`19_Plugins/`**: API & SDK cho hệ sinh thái Plugin, Marketplace architecture.
- **`20_Integrations/`**: Các tài liệu tích hợp với 3rd-party services (Salesforce, Hubspot, Slack, Discord).
- **`21_Prompts/`**: (Đã gộp một phần vào 06_AI nhưng có thể dùng làm nơi chứa Raw Prompt Files tĩnh).
- **`22_Decision_Records/`**: Log của các quyết định kinh doanh hoặc tính năng lớn.
- **`23_Roadmap/`**: Lộ trình phát triển 1-3 năm tới.

### Phase: Bổ Trợ (Auxiliary)

- **`24_Research/`**: Không gian nghiên cứu (Whitepapers, Gmail/Outlook API Notes, AI Benchmarks).
- **`25_Archive/`**: Nơi lưu trữ tài liệu đã lỗi thời (Deprecated ADRs, Cũ).
- **`26_Assets/`**: File hình ảnh, Sơ đồ, Logo, Figma exports, Wireframes.

---

## 3. Quy Ước Tra Cứu Nhanh (Quick Reference Guide)

Nếu bạn là...
- **Backend Developer**: Hãy bắt đầu với `04_Architecture`, `05_Backend`, `00_Project\CODING_STANDARDS.md`.
- **AI Engineer**: Tập trung vào `06_AI`, `24_Research`, `03_Domain\AI_DOMAIN.md`.
- **Product Manager**: Xem `01_Product`, `23_Roadmap`.
- **DevOps/SRE**: Xem `16_DevOps`, `17_Deployment`, `18_Operations`.
- **AI Coding Agent**: Bắt buộc đọc `00_Project\AI_ENTRYPOINT.md` và tuân thủ `00_Project\DOCUMENT_CONVENTIONS.md`.

---

## 4. Hướng Dẫn Cập Nhật Index (Index Maintenance)

`DOCUMENT_INDEX.md` không phải là file tĩnh.
Mỗi khi một Thư mục hoặc Module mới được sinh ra ở cấp độ Root của `docs/`, người thực hiện (Human hoặc AI) **phải** cập nhật file này.

**Quy tắc cập nhật**:
1. Đảm bảo đánh số thứ tự hai chữ số (00 đến 99).
2. Tên thư mục phải dùng tiếng Anh, viết hoa chữ cái đầu và dùng dấu gạch dưới (VD: `27_Analytics`).
3. Cập nhật giải thích ngắn gọn (1-2 dòng) về mục đích của thư mục đó vào phần `2. Cấu Trúc Tổng Thể`.

> END OF DOCUMENT INDEX
