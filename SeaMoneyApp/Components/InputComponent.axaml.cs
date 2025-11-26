using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SeaMoneyApp.Components;

public partial class InputComponent : UserControl
{
    public string? Label { get; set; }
    public string?Value { get; set; }
    public bool? IsReadOnly { get; set; } = false;
    public bool? IsPassword { get; set; } = false;
    public InputComponent()
    {
        InitializeComponent();
    }
}