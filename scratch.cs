using System;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexusMail.Infrastructure.Persistence;

var options = new DbContextOptionsBuilder<ApplicationDbContext>()
    .UseNpgsql("Host=localhost;Database=nexusmail;Username=postgres;Password=password")
    .Options;

using var db = new ApplicationDbContext(options);
var analysis = db.AIAnalyses.OrderByDescending(a => a.CreatedAt).FirstOrDefault();
if (analysis != null)
{
    Console.WriteLine($"EmailId: {analysis.EmailId}");
    Console.WriteLine($"State: {analysis.ProcessingState}");
    Console.WriteLine($"Classification: {analysis.Classification}");
    Console.WriteLine($"Priority: {analysis.Priority}");
    Console.WriteLine($"Summary: {analysis.Summary}");
    Console.WriteLine($"NeedsAttention: {analysis.NeedsAttention}");
}
else
{
    Console.WriteLine("No analysis found.");
}
