namespace SlideShow;

public class AppSettings : SettingsManager<AppSettings>
{
    public string ImageDirectory { get; set; } = string.Empty;
    public bool Sort { get; set; } = true;
    public bool Shuffle { get; set; } = false;
    public bool ShowInfo { get; set; } = true;
    public double Interval { get; set; } = 6;
}
