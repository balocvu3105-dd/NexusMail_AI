using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace NexusMail.Infrastructure.AI.Services;

/// <summary>
/// An incremental JSON parser that specifically looks for the "answer" field and yields its decoded text chunks.
/// Uses a state machine to safely handle chunk boundaries and JSON escape sequences without regex.
/// </summary>
public class IncrementalAnswerParser
{
    private enum State
    {
        SeekingAnswerKey,
        InAnswerKey,
        SeekingColon,
        SeekingValueQuote,
        InValue,
        InValueEscape,
        InValueUnicodeEscape,
        Done
    }

    private State _state = State.SeekingAnswerKey;
    private readonly string _targetKey = "\"answer\"";
    private int _keyMatchIndex = 0;
    
    // For unicode escapes \uXXXX
    private readonly StringBuilder _unicodeBuffer = new StringBuilder(4);

    public IEnumerable<string> ParseChunk(string chunk)
    {
        if (string.IsNullOrEmpty(chunk) || _state == State.Done)
        {
            yield break;
        }

        var sb = new StringBuilder();

        foreach (var c in chunk)
        {
            switch (_state)
            {
                case State.SeekingAnswerKey:
                    if (c == _targetKey[_keyMatchIndex])
                    {
                        _keyMatchIndex++;
                        if (_keyMatchIndex == _targetKey.Length)
                        {
                            _state = State.SeekingColon;
                        }
                    }
                    else
                    {
                        // Reset if it was a false start. Wait, to be perfectly robust we'd need KMP, but for this specific LLM output format, 
                        // simple reset is generally fine since "answer" won't partially appear and then fail often in standard JSON unless there's another key like "ans".
                        // Let's implement simple reset. If we matched part of "answer" but failed, we just reset.
                        _keyMatchIndex = (c == _targetKey[0]) ? 1 : 0;
                    }
                    break;

                case State.SeekingColon:
                    if (c == ':')
                    {
                        _state = State.SeekingValueQuote;
                    }
                    else if (!char.IsWhiteSpace(c))
                    {
                        // Invalid JSON or something else, but we just keep seeking.
                    }
                    break;

                case State.SeekingValueQuote:
                    if (c == '"')
                    {
                        _state = State.InValue;
                    }
                    else if (!char.IsWhiteSpace(c))
                    {
                        // Invalid JSON or null value? We expect a string.
                    }
                    break;

                case State.InValue:
                    if (c == '\\')
                    {
                        _state = State.InValueEscape;
                    }
                    else if (c == '"')
                    {
                        _state = State.Done;
                        if (sb.Length > 0)
                        {
                            yield return sb.ToString();
                            sb.Clear();
                        }
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;

                case State.InValueEscape:
                    if (c == 'u')
                    {
                        _state = State.InValueUnicodeEscape;
                        _unicodeBuffer.Clear();
                    }
                    else
                    {
                        // Standard escapes: \", \\, \/, \b, \f, \n, \r, \t
                        char unescaped = c switch
                        {
                            '"' => '"',
                            '\\' => '\\',
                            '/' => '/',
                            'b' => '\b',
                            'f' => '\f',
                            'n' => '\n',
                            'r' => '\r',
                            't' => '\t',
                            _ => c // Fallback
                        };
                        sb.Append(unescaped);
                        _state = State.InValue;
                    }
                    break;

                case State.InValueUnicodeEscape:
                    _unicodeBuffer.Append(c);
                    if (_unicodeBuffer.Length == 4)
                    {
                        // We have 4 hex digits
                        if (int.TryParse(_unicodeBuffer.ToString(), System.Globalization.NumberStyles.HexNumber, null, out int codePoint))
                        {
                            sb.Append((char)codePoint);
                        }
                        _state = State.InValue;
                    }
                    break;
            }
        }

        if (sb.Length > 0)
        {
            yield return sb.ToString();
        }
    }
}
