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

            builder.AppendLine();
            builder.AppendLine(":::jupyter-outputs");
            foreach(var output in outputs) {
                AppendOutput(builder, output);
            }
            builder.AppendLine(":::");
        }

        static void AppendOutput(StringBuilder builder, JsonElement output) {
            switch(output.GetProperty("output_type").GetString()) {
                case "stream":
                    AppendCode(builder, output.GetProperty("text").EnumerateArray(), "plaintext");
                    break;

                case "execute_result":
                case "display_data":
                    var data = output.GetProperty("data");

                    if(data.TryGetProperty("image/svg+xml", out var svg)) {
                        AppendInlineImage(builder, "image/svg+xml", EscapeDataString(svg.GetString()));
                    } else if(data.TryGetProperty("image/png", out var png)) {
                        AppendInlineImage(builder, "image/png;base64", png.GetString());
                    } else if(data.TryGetProperty("text/html", out var html)) {
                        builder.AppendLine(String.Concat(html.EnumerateArray()));
                    } else if(data.TryGetProperty("text/plain", out var text)) {
                        AppendCode(builder, text.EnumerateArray(), "plaintext");
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

        static void AppendInlineImage(StringBuilder builder, string mime, string data) {
            builder
                .Append("![](data:")
                .Append(mime)
                .Append(",")
                .Append(data)
                .AppendLine("){data-fancybox=jupyter-gallery}");
        }

        static void EnsureLineBreak(StringBuilder builder) {
            if(builder[builder.Length - 1] != '\n')
                builder.Append('\n');
        }

        static string EscapeSvg(string svg) {
            svg = Regex.Replace(svg, "<!--.+?-->", "", RegexOptions.Singleline);
            svg = Regex.Replace(svg, "\\s+", " ", RegexOptions.Singleline);
            svg = svg.Replace("> <", "><");
            svg = EscapeDataString(svg);
            return svg;
        }

        static string EscapeDataString(string text) {
            // https://stackoverflow.com/q/6695208

            var builder = new StringBuilder();
            var chunkSize = 1024;

            for(int i = 0; i < text.Length; i += chunkSize) {
                var chunk = text.Substring(i, Math.Min(chunkSize, text.Length - i));
                builder.Append(Uri.EscapeDataString(chunk));
            }

            return builder.ToString();
        }
    }

}
