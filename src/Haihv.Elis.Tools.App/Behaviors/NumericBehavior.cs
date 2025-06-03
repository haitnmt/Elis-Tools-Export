using System.Text.RegularExpressions;

namespace Haihv.Elis.Tools.App.Behaviors;

public partial class NumericBehavior : Behavior<Entry>
{
    protected override void OnAttachedTo(Entry entry)
    {
        entry.TextChanged += OnEntryTextChanged;
        base.OnAttachedTo(entry);
    }

    protected override void OnDetachingFrom(Entry entry)
    {
        entry.TextChanged -= OnEntryTextChanged;
        base.OnDetachingFrom(entry);
    }

    private static void OnEntryTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not Entry entry)
            return;

        var newText = e.NewTextValue;

        // Chỉ cho phép số nguyên (không có dấu thập phân)
        if (!string.IsNullOrEmpty(newText) && !IntegerOnlyRegex().IsMatch(newText))
        {
            // Lọc bỏ tất cả ký tự không phải số
            var numericOnly = RemoveNonDigitRegex().Replace(newText, "");
            entry.Text = numericOnly;
        }
    }

    [GeneratedRegex(@"^\d+$")]
    private static partial Regex IntegerOnlyRegex();
    [GeneratedRegex(@"[^\d]")]
    private static partial Regex RemoveNonDigitRegex();
}