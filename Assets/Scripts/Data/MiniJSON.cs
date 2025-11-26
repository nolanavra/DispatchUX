using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

// Lightweight JSON parser/serializer originally based on Unity's MiniJSON sample.
// Kept local to avoid extra dependencies for simple data ingestion.
namespace DispatchQuest.Data
{
    public static class MiniJSON
    {
        public static object Deserialize(string json)
        {
            if (string.IsNullOrEmpty(json)) return null;
            return Parser.Parse(json);
        }

        public static string Serialize(object obj)
        {
            return Serializer.Serialize(obj);
        }

        private sealed class Parser : IDisposable
        {
            private enum Token
            {
                None,
                CurlyOpen,
                CurlyClose,
                SquaredOpen,
                SquaredClose,
                Colon,
                Comma,
                String,
                Number,
                True,
                False,
                Null
            }

            private readonly string json;
            private int index;
            private StringBuilder stringBuilder;

            private Parser(string jsonString)
            {
                json = jsonString;
                stringBuilder = new StringBuilder();
            }

            public static object Parse(string jsonString)
            {
                using (var instance = new Parser(jsonString))
                {
                    return instance.ParseValue();
                }
            }

            public void Dispose()
            {
                stringBuilder = null;
            }

            private Dictionary<string, object> ParseObject()
            {
                var table = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                NextToken(); // consume '{'
                while (true)
                {
                    var token = NextToken();
                    if (token == Token.None)
                        return null;
                    if (token == Token.CurlyClose)
                        break;
                    if (token != Token.String)
                        return null;
                    var key = ParseString();
                    if (NextToken() != Token.Colon)
                        return null;
                    var value = ParseValue();
                    table[key] = value;
                    token = NextToken();
                    if (token == Token.Comma)
                        continue;
                    if (token == Token.CurlyClose)
                        break;
                    return null;
                }
                return table;
            }

            private List<object> ParseArray()
            {
                var array = new List<object>();
                NextToken(); // consume '['
                bool parsing = true;
                while (parsing)
                {
                    var token = NextToken();
                    switch (token)
                    {
                        case Token.None:
                            return null;
                        case Token.Comma:
                            continue;
                        case Token.SquaredClose:
                            parsing = false;
                            break;
                        default:
                            index--;
                            var value = ParseValue();
                            array.Add(value);
                            break;
                    }
                }
                return array;
            }

            private object ParseValue()
            {
                var token = NextToken();
                switch (token)
                {
                    case Token.String:
                        return ParseString();
                    case Token.Number:
                        return ParseNumber();
                    case Token.CurlyOpen:
                        return ParseObject();
                    case Token.SquaredOpen:
                        return ParseArray();
                    case Token.True:
                        return true;
                    case Token.False:
                        return false;
                    case Token.Null:
                        return null;
                    default:
                        return null;
                }
            }

            private string ParseString()
            {
                stringBuilder.Clear();
                ConsumeChar('"');
                bool parsing = true;
                while (parsing)
                {
                    if (index == json.Length)
                        break;
                    var c = json[index++];
                    switch (c)
                    {
                        case '"':
                            parsing = false;
                            break;
                        case '\\':
                            if (index == json.Length)
                            {
                                parsing = false;
                                break;
                            }
                            c = json[index++];
                            switch (c)
                            {
                                case '"':
                                case '\\':
                                case '/':
                                    stringBuilder.Append(c);
                                    break;
                                case 'b':
                                    stringBuilder.Append('\b');
                                    break;
                                case 'f':
                                    stringBuilder.Append('\f');
                                    break;
                                case 'n':
                                    stringBuilder.Append('\n');
                                    break;
                                case 'r':
                                    stringBuilder.Append('\r');
                                    break;
                                case 't':
                                    stringBuilder.Append('\t');
                                    break;
                                case 'u':
                                    if (json.Length - index >= 4)
                                    {
                                        var hex = json.Substring(index, 4);
                                        if (uint.TryParse(hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var codePoint))
                                        {
                                            stringBuilder.Append((char)codePoint);
                                            index += 4;
                                        }
                                    }
                                    break;
                            }
                            break;
                        default:
                            stringBuilder.Append(c);
                            break;
                    }
                }
                return stringBuilder.ToString();
            }

            private object ParseNumber()
            {
                var lastIndex = GetLastIndexOfNumber(index);
                var substring = json.Substring(index, lastIndex - index);
                index = lastIndex;
                if (double.TryParse(substring, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
                {
                    if (Math.Abs(number % 1) <= double.Epsilon)
                        return (long)number;
                    return number;
                }
                return 0d;
            }

            private int GetLastIndexOfNumber(int startIndex)
            {
                int i;
                for (i = startIndex; i < json.Length; i++)
                {
                    if ("0123456789+-.eE".IndexOf(json[i]) == -1)
                        break;
                }
                return i;
            }

            private Token NextToken()
            {
                EatWhitespace();
                if (index == json.Length)
                    return Token.None;
                var c = json[index++];
                switch (c)
                {
                    case '{': return Token.CurlyOpen;
                    case '}': return Token.CurlyClose;
                    case '[': return Token.SquaredOpen;
                    case ']': return Token.SquaredClose;
                    case ',': return Token.Comma;
                    case ':': return Token.Colon;
                    case '"': return Token.String;
                    case '0':
                    case '1':
                    case '2':
                    case '3':
                    case '4':
                    case '5':
                    case '6':
                    case '7':
                    case '8':
                    case '9':
                    case '-':
                        index--;
                        return Token.Number;
                }
                index--;
                if (Matches("true"))
                {
                    index += 4;
                    return Token.True;
                }
                if (Matches("false"))
                {
                    index += 5;
                    return Token.False;
                }
                if (Matches("null"))
                {
                    index += 4;
                    return Token.Null;
                }
                return Token.None;
            }

            private void EatWhitespace()
            {
                while (index < json.Length)
                {
                    var c = json[index];
                    if (c == ' ' || c == '\t' || c == '\n' || c == '\r')
                    {
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            private void ConsumeChar(char expected)
            {
                if (index < json.Length && json[index] == expected)
                {
                    index++;
                }
            }

            private bool Matches(string pattern)
            {
                if (json.Length - index < pattern.Length)
                    return false;
                for (var i = 0; i < pattern.Length; i++)
                {
                    if (json[index + i] != pattern[i])
                        return false;
                }
                return true;
            }
        }

        private sealed class Serializer
        {
            private readonly StringBuilder builder = new();

            public static string Serialize(object obj)
            {
                var instance = new Serializer();
                instance.SerializeValue(obj);
                return instance.builder.ToString();
            }

            private void SerializeValue(object value)
            {
                switch (value)
                {
                    case null:
                        builder.Append("null");
                        break;
                    case string s:
                        SerializeString(s);
                        break;
                    case bool b:
                        builder.Append(b ? "true" : "false");
                        break;
                    case IList list:
                        SerializeArray(list);
                        break;
                    case IDictionary dictionary:
                        SerializeObject(dictionary);
                        break;
                    case char c:
                        SerializeString(new string(c, 1));
                        break;
                    case IFormattable formattable:
                        builder.Append(formattable.ToString(null, CultureInfo.InvariantCulture));
                        break;
                    default:
                        SerializeString(value.ToString());
                        break;
                }
            }

            private void SerializeObject(IDictionary obj)
            {
                var first = true;
                builder.Append('{');
                foreach (var e in obj.Keys)
                {
                    if (!first)
                    {
                        builder.Append(',');
                    }
                    SerializeString(e.ToString());
                    builder.Append(':');
                    SerializeValue(obj[e]);
                    first = false;
                }
                builder.Append('}');
            }

            private void SerializeArray(IList array)
            {
                builder.Append('[');
                var first = true;
                foreach (var obj in array)
                {
                    if (!first)
                    {
                        builder.Append(',');
                    }
                    SerializeValue(obj);
                    first = false;
                }
                builder.Append(']');
            }

            private void SerializeString(string str)
            {
                builder.Append('"');
                foreach (var c in str)
                {
                    switch (c)
                    {
                        case '"':
                            builder.Append("\\\"");
                            break;
                        case '\\':
                            builder.Append("\\\\");
                            break;
                        case '\b':
                            builder.Append("\\b");
                            break;
                        case '\f':
                            builder.Append("\\f");
                            break;
                        case '\n':
                            builder.Append("\\n");
                            break;
                        case '\r':
                            builder.Append("\\r");
                            break;
                        case '\t':
                            builder.Append("\\t");
                            break;
                        default:
                            builder.Append(c);
                            break;
                    }
                }
                builder.Append('"');
            }
        }
    }
}
