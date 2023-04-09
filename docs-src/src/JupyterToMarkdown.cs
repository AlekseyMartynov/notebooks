using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Blog {

    static class JupyterToMarkdown {

        public static string Convert(string sourceText) {
            var builder = new StringBuilder();
            var json = JsonSerializer.Deserialize<JsonElement>(sourceText);

            foreach(var cell in json.GetProperty("cells").EnumerateArray()) {
                var source = cell.GetProperty("source").EnumerateArray();

                switch(cell.GetProperty("cell_type").GetString()) {
                    case "markdown":
                        AppendLines(builder, source);
                        break;

                    case "code":
                        AppendCode(builder, source, "python");
                        AppendOutputs(builder, cell.GetProperty("outputs").EnumerateArray().ToList());
                        break;

                    default:
                        throw new NotSupportedException();
                }

                builder.AppendLine();
            }

            var resultText = builder.ToString();
            resultText = Regex.Replace(resultText, "\n{3,}", "\n\n");

            return resultText;
        }

        static void AppendOutputs(StringBuilder builder, IReadOnlyList<JsonElement> outputs) {
            if(outputs.Count < 1)
                return;

            if(outputs.Count > 1)
                throw new NotSupportedException();

            builder.AppendLine();
            builder.AppendLine(":::jupyter-outputs");
            AppendOutput(builder, outputs[0]);
            builder.AppendLine(":::");
        }

        static void AppendOutput(StringBuilder builder, JsonElement output) {
            switch(output.GetProperty("output_type").GetString()) {
                case "execute_result":
                case "display_data":
                    var data = output.GetProperty("data");

                    if(data.TryGetProperty("image/svg+xml", out var svg)) {
                        builder
                            .Append("![](data:image/svg+xml,")
                            .Append(EscapeSvg(svg.GetString()))
                            .AppendLine(")");
                    } else if(data.TryGetProperty("text/plain", out var text)) {
                        AppendCode(builder, text.EnumerateArray(), "text");
                    } else {
                        throw new NotSupportedException();
                    }

                    break;

                default:
                    throw new NotSupportedException();
            }
        }

        static void AppendCode(StringBuilder builder, IEnumerable<JsonElement> lines, string lang) {
            builder
                .Append("```")
                .AppendLine(lang);
            AppendLines(builder, lines);
            builder
                .AppendLine("```");
        }

        static void AppendLines(StringBuilder builder, IEnumerable<JsonElement> lines) {
            foreach(var line in lines) {
                builder.Append(line.GetString());
                EnsureLineBreak(builder);
            }
        }

        static void EnsureLineBreak(StringBuilder builder) {
            if(builder[builder.Length - 1] != '\n')
                builder.Append('\n');
        }

        static string EscapeSvg(string svg) {
            svg = Regex.Replace(svg, "<!--.+?-->", "", RegexOptions.Singleline);
            svg = Regex.Replace(svg, "\\s+", " ", RegexOptions.Singleline);
            svg = svg.Replace("> <", "><");
            svg = Uri.EscapeDataString(svg);
            return svg;
        }
    }

}
