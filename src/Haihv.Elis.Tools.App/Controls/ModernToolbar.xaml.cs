using System.Collections.ObjectModel;
using Microsoft.Maui.Controls;
using Haihv.Elis.Tools.App.Models;

namespace Haihv.Elis.Tools.App.Controls;

public partial class ModernToolbar : ContentView
{
    public static readonly BindableProperty ItemsSourceProperty =
        BindableProperty.Create(nameof(ItemsSource), typeof(ObservableCollection<MenuToolbarItem>), typeof(ModernToolbar), null, propertyChanged: OnItemsSourcePropertyChanged);

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
        // Đảm bảo CollectionView được gán ItemsSource ngay sau khi khởi tạo
        Loaded += (sender, e) => UpdateCollectionViewItemsSource();
    }
    private void OnItemTapped(object sender, SelectionChangedEventArgs e)
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
    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);

        if (propertyName == nameof(ItemsSource))
        {
            UpdateCollectionViewItemsSource();
        }
    }

    private static void OnItemsSourcePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernToolbar toolbar)
        {
            toolbar.UpdateCollectionViewItemsSource();
        }
    }
    private void UpdateCollectionViewItemsSource()
    {
        if (ToolbarCollectionView != null)
        {
            ToolbarCollectionView.ItemsSource = ItemsSource;
            System.Diagnostics.Debug.WriteLine($"ModernToolbar: Updated CollectionView ItemsSource. ItemsSource count: {ItemsSource?.Count ?? 0}");
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("ModernToolbar: ToolbarCollectionView is null");
        }
    }
}
