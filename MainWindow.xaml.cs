using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using NaturalSort.Extension;

namespace SlideShow;

public partial class MainWindow : Window
{
    private List<string> images = [];
    private int currentIndex = 0;
    private bool isTargetImage1FG;
    private readonly DispatcherTimer timer;
    private readonly DispatcherTimer keepAlive;
    private readonly DoubleAnimation daIn;
    private readonly DoubleAnimation daOut;

    public MainWindow()
    {
        InitializeComponent();

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
            Interval = TimeSpan.FromSeconds(6),
        };
        timer.Tick += (s, e) => ShowNextImage();

        keepAlive = new()
        {
            Interval = TimeSpan.FromSeconds(120),
        };
        keepAlive.Tick += (s, e) => NativeMethods.KeepAwake();

        Loaded += (s, e) =>
        {
            var location = @"C:\Users\djper\OneDrive\Pictures\1974 Show";

            // Uncomment this section if you want a folder dialog;
            // otherwise, manually set the 'location' above.
            //var dlg = new FolderBrowserDialog { SelectedPath = location };
            //if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //{
            //   location = dlg.SelectedPath;
            //}

            images = ImageLocator.GetImagesFromLocation(location);
            images = [.. images.OrderBy(x => x, StringComparison.OrdinalIgnoreCase.WithNaturalSort())];
            //images.Shuffle();
            NativeMethods.KeepAwake();
            ShowImage(0);
            timer.Start();
        };

        Closing += (s, e) =>
        {
            NativeMethods.AllowSleep();
        };
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
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
            default:
                break;
        }
    }

    private void GoTo()
    {
        bool enabled = timer.IsEnabled;
        timer.Stop();

        Goto dlg = new(currentIndex + 1, images.Count);
        if (dlg.ShowDialog() ?? false && dlg.Index != -1)
        {
            currentIndex = dlg.Index - 1;
            ShowImage(currentIndex);
        }

        if (enabled)
        {
            timer.Start();
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
            timer.Stop();
        else
            timer.Start();
    }

    private void ShowImage(int index)
    {
        var imageSource = new BitmapImage();
        imageSource.BeginInit();
        imageSource.UriSource = new Uri(images[index]);
        imageSource.CacheOption = BitmapCacheOption.OnLoad;
        imageSource.EndInit();
        imageSource.Freeze();

        if (index == 0)
        {
            targetImage1.Source = imageSource;
            isTargetImage1FG = true;
            WindowState = WindowState.Maximized;
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
}
