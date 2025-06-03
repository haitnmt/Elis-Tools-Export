using System.Windows.Input;

namespace Haihv.Elis.Tools.App.Models;

public class MenuToolbarItem
{
    public string Icon { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
    public ICommand Command { get; set; } = null!;
    public bool IsActive { get; set; }
}
