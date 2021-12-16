using System.Windows;
using System.Windows.Media;
using System.Windows.Input;

namespace GoPlay
{
    /// <summary>
    /// Interaction logic for UriPrompt.xaml
    /// </summary>
    public partial class UriPrompt : Window
    {
        public string Link { get; private set; }

        public UriPrompt()
        {
            InitializeComponent();
            UriText.Focus();
        }

        private void PromptText_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            UriText.Focus();
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string text = UriText.Text.ToLower();
            if (!text.Contains("http://") && !text.Contains("https://"))
            {
                text = "http://" + text;
            }
            var client = new System.Net.WebClient();

            try
            {
                client.DownloadString(text);
            }
            catch
            {
                MessageBox.Show("Link is invalid", "Error", MessageBoxButton.OK);
                UriText.Focus();
                UriText.Select(0, UriText.Text.Length);
                return;
            }
            Link = text;
            DialogResult = true;
        }

        private void Confirm_MouseEnter(object sender, MouseEventArgs e)
        {
            ConfirmBorder.Background = new SolidColorBrush(Color.FromRgb(48, 138, 248));
            Confirm.Foreground = new SolidColorBrush(Color.FromRgb(18, 35, 81));
        }

        private void Confirm_MouseLeave(object sender, MouseEventArgs e)
        {
            ConfirmBorder.Background = new SolidColorBrush(Color.FromRgb(24, 69, 124));
            Confirm.Foreground = new SolidColorBrush(Color.FromRgb(134, 167, 253));
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Escape)
            {
                this.Close();
            }
        }
    }
}
