using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SlideShow;

public partial class SettingsViewModel : ObservableObject
{
    public event EventHandler? SettingsClosed;

    public void Initialize()
    {
        Interval = Properties.Settings.Default.Interval;
        Sort = Properties.Settings.Default.Sort;
        Shuffle = Properties.Settings.Default.Shuffle;
        ShowInfo = Properties.Settings.Default.ShowInfo;
    }

    public void Save()
    {
        Properties.Settings.Default.Interval = Interval;
        Properties.Settings.Default.Sort = Sort;
        Properties.Settings.Default.Shuffle = Shuffle;
        Properties.Settings.Default.ShowInfo = ShowInfo;

        Properties.Settings.Default.Save();
    }

    [ObservableProperty]
    private double interval = 6;

    [ObservableProperty]
    private Visibility visible = Visibility.Collapsed;

    [ObservableProperty]
    private bool sort = true;

    [ObservableProperty]
    private bool shuffle = false;

    [ObservableProperty]
    private bool showInfo = true;

    [RelayCommand]
    private void OK()
    {
        Save();
        Visible = Visibility.Collapsed;

        SettingsClosed?.Invoke(this, EventArgs.Empty);
    }
}
