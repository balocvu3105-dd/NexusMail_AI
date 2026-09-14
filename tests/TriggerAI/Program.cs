using System;
using System.Text;
using Npgsql;
using RabbitMQ.Client;
using Newtonsoft.Json;

// ─── Configuration ──────────────────────────────────────────────────────────
const string connStr = "Host=localhost;Database=nexusmail;Username=postgres;Password=password";
const string rabbitUri = "amqp://guest:guest@localhost:5672";

// These are fixed from DB:
// AccountId = 7fc30664-c1e3-48cf-b938-26323cf5cdf9 (balocvu3105@gmail.com)
// WorkspaceId = 21ebbc0b-586e-48f2-8a1d-92af61defcce
var accountId  = Guid.Parse("7fc30664-c1e3-48cf-b938-26323cf5cdf9");
var workspaceId = Guid.Parse("21ebbc0b-586e-48f2-8a1d-92af61defcce");

var emailId    = Guid.NewGuid();
var analysisId = Guid.NewGuid();
var messageId  = $"validation-test-{Guid.NewGuid():N}";

Console.WriteLine("=== NexusMail Phase D Validation Trigger ===");
Console.WriteLine($"EmailId:    {emailId}");
Console.WriteLine($"AnalysisId: {analysisId}");

// ─── Step 1: Insert test Email ────────────────────────────────────────────
using var conn = new NpgsqlConnection(connStr);
conn.Open();

Console.WriteLine("\n[1] Inserting test Email...");
using (var cmd = new NpgsqlCommand(@"
    INSERT INTO ""Emails"" (""Id"", ""AccountId"", ""MessageId"", ""Sender"", ""Subject"", ""Content"", ""LifecycleState"", ""ReceivedAt"")
    VALUES (@id, @accountId, @messageId, @sender, @subject, @content, @state, @receivedAt)", conn))
{
    cmd.Parameters.AddWithValue("id", emailId);
    cmd.Parameters.AddWithValue("accountId", accountId);
    cmd.Parameters.AddWithValue("messageId", messageId);
    cmd.Parameters.AddWithValue("sender", "validation@nexusmail-test.local");
    cmd.Parameters.AddWithValue("subject", "NexusMail AI Validation Test");
    cmd.Parameters.AddWithValue("content", "Please review this email and summarize the key action I need to take.\nI need to confirm the project deployment schedule by Friday and reply with my availability.");
    cmd.Parameters.AddWithValue("state", "Received");
    cmd.Parameters.AddWithValue("receivedAt", DateTime.UtcNow);
    cmd.ExecuteNonQuery();
}
Console.WriteLine($"  ✓ Email inserted: Id={emailId}");

// ─── Step 2: Insert AIAnalysis in Pending state ────────────────────────────
Console.WriteLine("\n[2] Inserting AIAnalysis (Pending)...");
using (var cmd = new NpgsqlCommand(@"
    INSERT INTO ""AIAnalyses"" (""Id"", ""EmailId"", ""ProcessingState"", ""EmbeddingStatus"", ""IsCompletedEventPublished"", ""NeedsAttention"", ""Confidence"", ""Priority"", ""PriorityScore"", ""Tags"", ""Language"", ""Category"")
    VALUES (@id, @emailId, @state, @embeddingStatus, @eventPublished, @needsAttention, @confidence, @priority, @priorityScore, @tags, @language, @category)", conn))
{
    cmd.Parameters.AddWithValue("id", analysisId);
    cmd.Parameters.AddWithValue("emailId", emailId);
    cmd.Parameters.AddWithValue("state", "Pending");
    cmd.Parameters.AddWithValue("embeddingStatus", "Pending");
    cmd.Parameters.AddWithValue("eventPublished", false);
    cmd.Parameters.AddWithValue("needsAttention", false);
    cmd.Parameters.AddWithValue("confidence", 0.0);
    cmd.Parameters.AddWithValue("priority", "Medium");
    cmd.Parameters.AddWithValue("priorityScore", 0);
    cmd.Parameters.AddWithValue("tags", new string[] { });
    cmd.Parameters.AddWithValue("language", DBNull.Value);
    cmd.Parameters.AddWithValue("category", DBNull.Value);
    cmd.ExecuteNonQuery();
}
Console.WriteLine($"  ✓ AIAnalysis inserted: Id={analysisId}, State=Pending");

// ─── Step 3: Publish AIProcessingRequestedMessage to RabbitMQ ─────────────
Console.WriteLine("\n[3] Publishing AIProcessingRequestedMessage to RabbitMQ...");

var factory = new ConnectionFactory { Uri = new Uri(rabbitUri) };
using var rabbitConn = factory.CreateConnection();
using var channel = rabbitConn.CreateModel();

// MassTransit queue naming convention
const string queueName = "AIProcessingRequested";
channel.QueueDeclarePassive(queueName); // Will throw if queue doesn't exist

// MassTransit message envelope
var message = new
{
    messageId = Guid.NewGuid().ToString(),
    messageType = new[] { "urn:message:NexusMail.Contracts.AI:AIProcessingRequestedMessage" },
    message = new
    {
        emailId = emailId,
        workspaceId = workspaceId
    },
    sentTime = DateTime.UtcNow.ToString("O"),
    headers = new { }
};

var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
var props = channel.CreateBasicProperties();
props.ContentType = "application/vnd.masstransit+json";
props.DeliveryMode = 2; // persistent
props.MessageId = message.messageId;

channel.BasicPublish(
    exchange: "",
    routingKey: queueName,
    basicProperties: props,
    body: body);

Console.WriteLine($"  ✓ Published AIProcessingRequestedMessage for EmailId={emailId}");
Console.WriteLine($"\n=== Evidence Summary ===");
Console.WriteLine($"  EmailId:    {emailId}");
Console.WriteLine($"  AnalysisId: {analysisId}");
Console.WriteLine($"  WorkspaceId: {workspaceId}");
Console.WriteLine($"\nNow watch AI Worker logs for:");
Console.WriteLine($"  'Received AIProcessingRequestedMessage for EmailId: {emailId}'");
Console.WriteLine($"  Then query DB: SELECT \"ProcessingState\", \"Summary\" FROM \"AIAnalyses\" WHERE \"Id\" = '{analysisId}'");
