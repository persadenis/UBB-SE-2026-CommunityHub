using System.Text.RegularExpressions;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;

namespace ChatModule.src.views
{
    public static class MentionTextHelper
    {
        private const string MentionRegexPattern = @"@\w+";
        private const byte HighlightAlpha = 210;
        private const byte HighlightRed = 92;
        private const byte HighlightGreen = 76;
        private const byte HighlightBlue = 196;

        private static readonly Regex _MentionPattern = new Regex(MentionRegexPattern, RegexOptions.Compiled);

        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached(
            "Text",
            typeof(string),
            typeof(MentionTextHelper),
            new PropertyMetadata(null, OnTextChanged));

        public static void SetText(TextBlock element, string? value)
        {
            element.SetValue(TextProperty, value);
        }

        public static string? GetText(TextBlock element)
        {
            return (string?)element.GetValue(TextProperty);
        }

        private static void OnTextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            if (dependencyObject is not TextBlock textBlock)
            {
                return;
            }

            var text = eventArgs.NewValue as string ?? string.Empty;
            textBlock.Text = text;
            textBlock.TextHighlighters.Clear();

            var matches = _MentionPattern.Matches(text);
            if (matches.Count == 0)
            {
                return;
            }

            var highlighter = new TextHighlighter
            {
                Background = new SolidColorBrush(ColorHelper.FromArgb(HighlightAlpha, HighlightRed, HighlightGreen, HighlightBlue)),
                Foreground = new SolidColorBrush(Colors.White)
            };

            foreach (Match match in matches)
            {
                highlighter.Ranges.Add(new TextRange
                {
                    StartIndex = match.Index,
                    Length = match.Length
                });
            }

            textBlock.TextHighlighters.Add(highlighter);
        }
    }
}