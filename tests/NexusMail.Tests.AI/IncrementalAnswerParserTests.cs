using System.Linq;
using NexusMail.Infrastructure.AI.Services;
using Xunit;

namespace NexusMail.Tests.AI;

public class IncrementalAnswerParserTests
{
    [Fact]
    public void ParseChunk_ShouldHandleChunkBoundaries()
    {
        // 1. Chunk boundary test
        var parser = new IncrementalAnswerParser();
        var chunks = new[]
        {
            "{\"ans",
            "wer\":\"Hello",
            " world\",\"state\":\"Grounded\"}"
        };

        var results = chunks.SelectMany(c => parser.ParseChunk(c).ToList()).ToList();

        Assert.Equal(2, results.Count);
        Assert.Equal("Hello", results[0]);
        Assert.Equal(" world", results[1]);
    }

    [Fact]
    public void ParseChunk_ShouldHandleJsonEscapes()
    {
        // 2. JSON escape test
        var parser = new IncrementalAnswerParser();
        var chunks = new[]
        {
            "{\"answer\":\"He said \\\"hello\\\".\\nNext line\", \"state\":\"Grounded\"}"
        };

        var results = chunks.SelectMany(c => parser.ParseChunk(c).ToList()).ToList();

        Assert.Single(results);
        Assert.Equal("He said \"hello\".\nNext line", results[0]);
    }
    
    [Fact]
    public void ParseChunk_ShouldHandleUnicodeEscapes()
    {
        var parser = new IncrementalAnswerParser();
        var chunks = new[]
        {
            "{\"answer\":\"Unicode \\u00E9 test\"}"
        };

        var results = chunks.SelectMany(c => parser.ParseChunk(c).ToList()).ToList();

        Assert.Single(results);
        Assert.Equal("Unicode \u00E9 test", results[0]);
    }

    [Fact]
    public void ParseChunk_ShouldIgnoreFalseStarts()
    {
        var parser = new IncrementalAnswerParser();
        var chunks = new[]
        {
            "{\"any\":\"value\",\"ans\":\"foo\",\"answer\":\"Correct!\"}"
        };

        var results = chunks.SelectMany(c => parser.ParseChunk(c).ToList()).ToList();

        Assert.Single(results);
        Assert.Equal("Correct!", results[0]);
    }
}
