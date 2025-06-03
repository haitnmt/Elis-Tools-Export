using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Haihv.Elis.Tools.App.Models;

namespace Haihv.Elis.Tools.App.Controls;

public partial class ModernToolbar : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<MenuToolbarItem>), typeof(ModernToolbar), null);

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(MenuToolbarItem), typeof(ModernToolbar), null);

    public static readonly BindableProperty OrientationProperty =
        BindableProperty.Create(nameof(Orientation), typeof(StackOrientation), typeof(ModernToolbar), StackOrientation.Horizontal);

    public ObservableCollection<MenuToolbarItem> ItemsSource
    {
        get => (ObservableCollection<MenuToolbarItem>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public MenuToolbarItem SelectedItem
    {
        get => (MenuToolbarItem)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public StackOrientation Orientation
    {
        get => (StackOrientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public ModernToolbar()
    {
        InitializeComponent();
        BindingContext = this;
    }    private void OnItemTapped(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is MenuToolbarItem selectedItem)
        {
            // Cập nhật trạng thái active
            UpdateActiveStates(selectedItem);
            
            // Thực thi command nếu có
            if (selectedItem.Command?.CanExecute(selectedItem) == true)
            {
                selectedItem.Command.Execute(selectedItem);
            }

            // Clear selection để cho phép tap lại
            ((CollectionView)sender).SelectedItem = null;
        }
    }

    private void UpdateActiveStates(MenuToolbarItem activeItem)
    {
        if (ItemsSource != null)
        {
            foreach (var item in ItemsSource)
            {
                item.IsActive = item == activeItem;
            }
        }
    }

    protected override void OnPropertyChanged(string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(ItemsSource))
        {
            OnItemsSourceChanged();
        }
    }
    private void OnItemsSourceChanged()
    {
        // Có thể thêm logic xử lý khi ItemsSource thay đổi
        // ItemsSource sẽ được bind trực tiếp qua XAML
    }
}
