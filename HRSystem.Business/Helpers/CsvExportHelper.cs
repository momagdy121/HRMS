using System.Text;

namespace HRSystem.Business.Helpers;

public static class CsvExportHelper
{
    public static byte[] CreateCsvBytes(IEnumerable<string> headers, IEnumerable<IEnumerable<object?>> rows)
    {
        var sb = new StringBuilder();

        // Header row
        sb.AppendLine(string.Join(",", headers.Select(EscapeCsvField)));

        // Data rows
        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row.Select(EscapeCsvField)));
        }

        // Prepend UTF-8 BOM so Microsoft Excel correctly renders all encodings
        var preamble = Encoding.UTF8.GetPreamble();
        var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[preamble.Length + contentBytes.Length];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(contentBytes, 0, result, preamble.Length, contentBytes.Length);

        return result;
    }

    public static string EscapeCsvField(object? value)
    {
        if (value == null) return string.Empty;

        var str = value switch
        {
            DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            DateOnly d => d.ToString("yyyy-MM-dd"),
            TimeOnly t => t.ToString("HH:mm:ss"),
            decimal dec => dec.ToString("0.00"),
            double dbl => dbl.ToString("0.00"),
            float flt => flt.ToString("0.00"),
            bool b => b ? "Yes" : "No",
            _ => value.ToString() ?? string.Empty
        };

        if (str.Contains(',') || str.Contains('"') || str.Contains('\n') || str.Contains('\r'))
        {
            return $"\"{str.Replace("\"", "\"\"")}\"";
        }

        return str;
    }
}
