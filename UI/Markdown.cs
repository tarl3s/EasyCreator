using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace EasyCreator;

/// <summary>Renders the small Markdown subset used by lessons (#, ##, -, ``` blocks, **bold**, *italic*, `code`) into a panel.</summary>
public static class Markdown
{
    public static StackPanel Render(string md)
    {
        var panel = new StackPanel();
        var lines = (md ?? "").Replace("\r\n", "\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (line.TrimStart().StartsWith("```"))
            {
                var code = new List<string>();
                for (i++; i < lines.Length && !lines[i].TrimStart().StartsWith("```"); i++) code.Add(lines[i]);
                panel.Children.Add(new Border
                {
                    Background = Ui.Code, CornerRadius = new CornerRadius(6), Padding = new Thickness(10),
                    Margin = new Thickness(0, 4, 0, 8),
                    Child = new TextBlock { Text = string.Join("\n", code), FontFamily = Ui.Mono, FontSize = 13, Foreground = Ui.Text, TextWrapping = TextWrapping.Wrap },
                });
                continue;
            }
            if (string.IsNullOrWhiteSpace(line)) continue;
            double size = 14; bool bold = false; string text = line, bullet = "";
            if (line.StartsWith("### ")) { size = 15; bold = true; text = line[4..]; }
            else if (line.StartsWith("## ")) { size = 17; bold = true; text = line[3..]; }
            else if (line.StartsWith("# ")) { size = 20; bold = true; text = line[2..]; }
            else if (Regex.IsMatch(line, @"^\s*[-*] ")) { bullet = "•  "; text = Regex.Replace(line, @"^\s*[-*] ", ""); }
            var tb = new TextBlock
            {
                FontSize = size, Foreground = Ui.Text, TextWrapping = TextWrapping.Wrap, LineHeight = size * 1.45,
                Margin = new Thickness(bullet.Length > 0 ? 10 : 0, bold ? 10 : 0, 0, 6),
                FontWeight = bold ? FontWeights.SemiBold : FontWeights.Normal,
            };
            if (bullet.Length > 0) tb.Inlines.Add(new Run(bullet));
            AddInlines(tb.Inlines, text);
            panel.Children.Add(tb);
        }
        return panel;
    }

    static void AddInlines(InlineCollection inlines, string text)
    {
        foreach (var part in Regex.Split(text, @"(\*\*[^*]+\*\*|`[^`]+`|(?<![\w*])\*[^*\s](?:[^*]*[^*\s])?\*(?![\w*]))"))
        {
            if (part.Length == 0) continue;
            if (part.StartsWith("**") && part.EndsWith("**") && part.Length > 4)
                inlines.Add(new Bold(new Run(part[2..^2])));
            else if (part.StartsWith('`') && part.EndsWith('`') && part.Length > 2)
                inlines.Add(new Run(part[1..^1]) { FontFamily = Ui.Mono, Background = Ui.Bg3 });
            else if (part.Length > 2 && part[0] == '*' && part[^1] == '*')
                inlines.Add(new Italic(new Run(part[1..^1])));
            else inlines.Add(new Run(part));
        }
    }
}
