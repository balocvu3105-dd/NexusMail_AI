INSERT INTO "Workspaces" ("Id", "Name", "Plan")
VALUES ('00000000-0000-0000-0000-000000000001', 'Test Workspace', 'Free');

INSERT INTO "EmailAccounts" ("Id", "WorkspaceId", "Provider", "EmailAddress", "EncryptedAccessToken", "EncryptedRefreshToken", "EncryptionVersion", "Status", "TokenExpiresAt", "SyncEnabled", "LastSyncAt", "CreatedAt", "UpdatedAt")
VALUES ('00000000-0000-0000-0000-000000000002', '00000000-0000-0000-0000-000000000001', 'FakeProvider', 'user@example.com', 'dummy_token', 'dummy_token', 'v1', 'Connected', NOW(), true, NOW(), NOW(), NOW());

INSERT INTO "Emails" ("Id", "AccountId", "MessageId", "Sender", "Subject", "Content", "ReceivedAt", "LifecycleState")
VALUES ('00000000-0000-0000-0000-000000000010', '00000000-0000-0000-0000-000000000002', 'msg-1234', 'Microsoft Billing', 'Your June invoice is ready', 'Here is the full text of the invoice. It is $128.40.', NOW(), 'Processed');

INSERT INTO "AIAnalyses" ("Id", "EmailId", "Summary", "Category", "Priority", "PriorityScore", "Confidence", "Language", "Tags")
VALUES ('00000000-0000-0000-0000-000000000011', '00000000-0000-0000-0000-000000000010', 'Invoice for June: $128.40. Payment due July 15.', 'Finance', 'HIGH', 9, 0.95, 'en', ARRAY['Invoice', 'Payment']);
