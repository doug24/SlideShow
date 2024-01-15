using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SlideShow;

public partial class SettingsViewModel : ObservableObject
{
    public event EventHandler? SettingsClosed;

    public void Load()
    {
        AppSettings.Load();

        Interval = AppSettings.Instance.Interval;
        Sort = AppSettings.Instance.Sort;
        Shuffle = AppSettings.Instance.Shuffle;
        ShowInfo = AppSettings.Instance.ShowInfo;
    }

    public void Save()
    {
        AppSettings.Instance.Interval = Interval;
        AppSettings.Instance.Sort = Sort;
        AppSettings.Instance.Shuffle = Shuffle;
        AppSettings.Instance.ShowInfo = ShowInfo;

        AppSettings.Save();
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
