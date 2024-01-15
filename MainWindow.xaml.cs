using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using NaturalSort.Extension;

namespace SlideShow;

public partial class MainWindow : Window
{
    private List<string> images = [];
    private int currentIndex = 0;
    private bool isTargetImage1FG;
    private readonly SettingsViewModel viewModel;
    private readonly DispatcherTimer timer;
    private readonly DispatcherTimer keepAwake;
    private readonly DoubleAnimation daIn;
    private readonly DoubleAnimation daOut;
    private State savedState = new(false);

    public MainWindow()
    {
        InitializeComponent();

        viewModel = new SettingsViewModel();
        viewModel.Initialize();
        DataContext = viewModel;
        settingsView.DataContext = viewModel;
        viewModel.PropertyChanged += ViewModel_PropertyChanged;
        viewModel.SettingsClosed += ViewModel_SettingsClosed;

        daIn = new()
        {
            From = 0,
            To = 1,
            Duration = new Duration(TimeSpan.FromSeconds(1)),
        };
        daOut = new()
        {
            From = 1,
            To = 0,
            Duration = new Duration(TimeSpan.FromSeconds(1)),
        };
        daIn.Freeze();
        daOut.Freeze();

        timer = new()
        {
            Interval = TimeSpan.FromSeconds(viewModel.Interval),
        };
        timer.Tick += (s, e) => ShowNextImage();

        keepAwake = new()
        {
            Interval = TimeSpan.FromSeconds(90),
        };
        keepAwake.Tick += (s, e) => Native.KeepAwake();
        keepAwake.Start();

        Loaded += (s, e) =>
        {
            _ = Native.UseImmersiveDarkMode(this, true);
            info.Visibility = viewModel.ShowInfo ? Visibility.Visible : Visibility.Collapsed;
            status.Visibility = viewModel.ShowInfo ? Visibility.Visible : Visibility.Collapsed;

            string location = Properties.Settings.Default.ImageDirectory;

            if (string.IsNullOrEmpty(location) || !Directory.Exists(location))
            {
                location = GetImageDirectory();
            }

            if (LoadImages(location))
            {
                timer.Start();
                status.Text = "⟳";
                WindowState = WindowState.Maximized;
                Native.KeepAwake();
            }
        };

        Closing += (s, e) =>
        {
            timer.Stop();
            status.Text = "⊘";
            keepAwake.Stop();
            Native.AllowSleep();
        };
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Space:
                Pause();
                break;
            case Key.Next:
            case Key.Right:
            case Key.Enter:
                ShowNextImage();
                break;
            case Key.Left:
            case Key.PageUp:
                ShowPreviousImage();
                break;
            case Key.Escape:
                Escape();
                break;
            case Key.F11:
                ToggleFullScreen();
                break;
            case Key.G:
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    GoTo();
                }
                break;
            case Key.O:
                if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
                {
                    timer.Stop();
                    status.Text = "⊘";
                    string location = GetImageDirectory();
                    if (LoadImages(location))
                    {
                        timer.Start();
                        status.Text = "⟳";
                    }
                }
                break;
            case Key.F1:
            case Key.OemQuestion:
            case Key.Separator:
                if (viewModel.Visible == Visibility.Collapsed)
                {
                    savedState = new(timer.IsEnabled);
                    timer.Stop();
                    status.Text = "⊘";
                    Cursor = Cursors.Arrow;
                    viewModel.Visible = Visibility.Visible;
                }
                else
                {
                    viewModel.Visible = Visibility.Collapsed;
                    ViewModel_SettingsClosed(this, EventArgs.Empty);
                }
                break;
            default:
                break;
        }
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(SettingsViewModel.Interval):
                bool enabled = timer.IsEnabled;
                timer.Stop();
                timer.Interval = TimeSpan.FromSeconds(viewModel.Interval);
                if (enabled)
                    timer.Start();
                break;

            case nameof(SettingsViewModel.ShowInfo):
                info.Visibility = viewModel.ShowInfo ? Visibility.Visible : Visibility.Collapsed;
                status.Visibility = viewModel.ShowInfo ? Visibility.Visible : Visibility.Collapsed;
                break;

            case nameof(SettingsViewModel.Sort):
                if (viewModel.Sort)
                    SortImages();
                break;

            case nameof(SettingsViewModel.Shuffle):
                if (viewModel.Shuffle)
                    ShuffleImages();
                break;
        }
    }

    private void ViewModel_SettingsClosed(object? sender, EventArgs e)
    {
        if (savedState.TimerEnabled)
        {
            timer.Start();
            status.Text = "⟳";
        }
        if (WindowStyle == WindowStyle.None)
        {
            Cursor = Cursors.None;
        }
    }

    private void GoTo()
    {
        bool enabled = timer.IsEnabled;
        timer.Stop();
        status.Text = "⊘";

        Goto dlg = new(currentIndex + 1, images.Count);
        if (dlg.ShowDialog() ?? false)
        {
            currentIndex = dlg.Index - 1;
            ShowImage(currentIndex);
        }
        if (enabled)
        {
            timer.Start();
            status.Text = "⟳";
        }
    }

    private void Escape()
    {
        if (WindowStyle == WindowStyle.None)
        {
            ToggleFullScreen();
        }
        else
        {
            Close();
        }
    }

    private void ToggleFullScreen()
    {
        Visibility = Visibility.Collapsed;
        if (WindowStyle == WindowStyle.None)
        {
            WindowStyle = WindowStyle.SingleBorderWindow;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.CanResize;
            Topmost = false;
            Cursor = Cursors.Arrow;
        }
        else
        {
            WindowStyle = WindowStyle.None;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
            Cursor = Cursors.None;
        }
        Visibility = Visibility.Visible;
    }

    private void Pause()
    {
        if (timer.IsEnabled)
        {
            timer.Stop();
            status.Text = "⊘";
        }
        else
        {
            timer.Start();
            status.Text = "⟳";
        }
    }

    private void ShowImage(int index)
    {
        var imageSource = new BitmapImage();
        imageSource.BeginInit();
        imageSource.UriSource = new Uri(images[index]);
        imageSource.CacheOption = BitmapCacheOption.OnLoad;
        imageSource.EndInit();
        imageSource.Freeze();

        if (targetImage1.Source == null)
        {
            targetImage1.Source = imageSource;
            isTargetImage1FG = true;
        }
        else
        {
            var fg = isTargetImage1FG ? targetImage2 : targetImage1;
            var bg = isTargetImage1FG ? targetImage1 : targetImage2;
            isTargetImage1FG = !isTargetImage1FG;

            fg.Source = imageSource;
            fg.BeginAnimation(OpacityProperty, daIn);
            bg.BeginAnimation(OpacityProperty, daOut);
        }
        info.Text = $"{Path.GetFileName(images[index])} ({index + 1}/{images.Count})";
    }

    private void ShowNextImage()
    {
        currentIndex++;
        if (currentIndex >= images.Count) currentIndex = 0;
        ShowImage(currentIndex);
    }

    private void ShowPreviousImage()
    {
        if (currentIndex > 0) currentIndex--;
        ShowImage(currentIndex);
    }

    private void ShuffleImages()
    {
        if (images.Count > 0)
        {
            images.Shuffle();
            ShowImage(0);
        }
    }

    private void SortImages()
    {
        if (images.Count > 0)
        {
            images = [.. images.OrderBy(x => x, StringComparison.OrdinalIgnoreCase.WithNaturalSort())];
            ShowImage(0);
        }
    }

    private static string GetImageDirectory()
    {
        string location = string.Empty;
        var dlg = new OpenFolderDialog
        {
            Title = "Slide Show Images Directory",
            Multiselect = false,
        };

        if (dlg.ShowDialog() ?? false)
        {
            location = dlg.FolderName;
            Properties.Settings.Default.ImageDirectory = location;
            Properties.Settings.Default.Save();
        }
        return location;
    }

    private bool LoadImages(string location)
    {
        if (!string.IsNullOrEmpty(location) && Directory.Exists(location))
        {
            images = ImageLocator.GetImagesFromLocation(location);
            if (images.Count > 0)
            {
                if (viewModel.Sort)
                {
                    SortImages();
                }
                else if (viewModel.Shuffle)
                {
                    ShuffleImages();
                }
                return true;
            }
            else
            {
                MessageBox.Show($"No image files were found in the directory: {location}",
                    "Slide Show", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        else
        {
            MessageBox.Show($"The directory was not found: {location}",
                "Slide Show", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        return false;
    }

    private record State(bool TimerEnabled)
    {
    }
}
