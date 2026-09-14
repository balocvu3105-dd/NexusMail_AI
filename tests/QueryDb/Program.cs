using System;
using Npgsql;

string connStr = "Host=localhost;Database=nexusmail;Username=postgres;Password=password";
var analysisId = "36c8f937-8c21-48d7-9b5f-3b05cd550b66";
var emailId    = "45ea9ffa-fd3f-4773-868b-5fef5840c4ed";

using var conn = new NpgsqlConnection(connStr);
conn.Open();

Console.WriteLine($"=== AIAnalysis state for Id={analysisId} ===");
using (var cmd = new NpgsqlCommand(
    @"SELECT ""Id"", ""ProcessingState"", ""ProcessingError"", ""Summary"", ""Category"", ""Priority"", ""Confidence"", ""NeedsAttention"", ""ProcessingAttemptId""
      FROM ""AIAnalyses"" WHERE ""Id"" = @id", conn))
{
    cmd.Parameters.AddWithValue("id", Guid.Parse(analysisId));
    using var r = cmd.ExecuteReader();
    while (r.Read())
    {
        Console.WriteLine($"  ProcessingState:    {r[1]}");
        Console.WriteLine($"  ProcessingError:    {r[2]}");
        Console.WriteLine($"  Summary:            {r[3]}");
        Console.WriteLine($"  Category:           {r[4]}");
        Console.WriteLine($"  Priority:           {r[5]}");
        Console.WriteLine($"  Confidence:         {r[6]}");
        Console.WriteLine($"  NeedsAttention:     {r[7]}");
        Console.WriteLine($"  ProcessingAttemptId:{r[8]}");
    }
}

Console.WriteLine($"\n=== Email for Id={emailId} ===");
using (var cmd = new NpgsqlCommand(
    @"SELECT ""Subject"", ""Content"", ""ReceivedAt"" FROM ""Emails"" WHERE ""Id"" = @id", conn))
{
    cmd.Parameters.AddWithValue("id", Guid.Parse(emailId));
    using var r = cmd.ExecuteReader();
    while (r.Read())
    {
        Console.WriteLine($"  Subject:    {r[0]}");
        Console.WriteLine($"  Content:    {r[1]}");
        Console.WriteLine($"  ReceivedAt: {r[2]}");
    }
}
