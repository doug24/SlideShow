using System.Windows;

namespace SlideShow;

/// <summary>
/// Interaction logic for Goto.xaml
/// </summary>
public partial class Goto : Window
{
    private readonly int maxIndex;

    public Goto(int currentIndex, int maxIndex)
    {
        InitializeComponent();

        this.maxIndex = maxIndex;

        label.Content = $"Index number (1 - {maxIndex}):";
        input.Text = currentIndex.ToString();

        input.Focus();
        input.SelectAll();
    }

    public int Index
    {
        get
        {
            if (!string.IsNullOrEmpty(input.Text) &&
                int.TryParse(input.Text, out int value) &&
                value > 0 && value <= maxIndex)
            {
                return value;
            }
            return -1;
        }
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
        Close();
    }
}

