using System;
using System.Globalization;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GoPlay
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isPlaying = false, isMouseDown = false, isLoop = false;
        private TimeSpan duration;
        private TimeSpan timeSpan;
        private TimeSpan previousTime = TimeSpan.Zero;
        private Timer timer = new Timer();
        private Timer volumeTimeInterval = new Timer();
        private string source;
        private double previousVolume = 50;
        
        private bool IsPlaying
        {
            get { return isPlaying; }
            set
            {
                isPlaying = value;
                if(isPlaying)
                {
                    PlayPause.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/pause.png", UriKind.Absolute));
                    if(!MediaPlayer.HasVideo && MediaPlayer.HasAudio)
                    {
                        AppIcon.Source = new BitmapImage(new Uri("pack://application:,,,/GoPlay.ico", UriKind.Absolute));
                    }
                }
                else
                {
                    PlayPause.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/play.png", UriKind.Absolute));
                    if (!MediaPlayer.HasVideo && MediaPlayer.HasAudio)
                    {
                        AppIcon.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/pauseIcon.ico", UriKind.Absolute));
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            timer.Interval = 500;
            volumeTimeInterval.Interval = 1000;

            timer.Tick += (sender, e) =>
            {
                timeSpan = new TimeSpan(MediaPlayer.Position.Hours, MediaPlayer.Position.Minutes, MediaPlayer.Position.Seconds);
                if (timeSpan.Seconds != previousTime.Seconds)
                    VideoProgess.Value++;

                VideoDuration.Content = timeSpan + " / " + duration;
                
                previousTime = new TimeSpan(MediaPlayer.Position.Hours, MediaPlayer.Position.Minutes, MediaPlayer.Position.Seconds);
            };
            
            volumeTimeInterval.Tick += (s, a) =>
            {
                VolumeLabel.Visibility = Visibility.Collapsed;
                volumeTimeInterval.Stop();
            };
        }
        
        private void PlayPause_MouseUp(object sender, MouseButtonEventArgs e)
        {
            PlayOrPause();
        }

        private void Stop_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (MediaPlayer.Source == null) return;

            MediaPlayer.Stop();
            MediaPlayer.Source = null;
            MediaPlayer.Source = new Uri(source);

            timer.Stop();
            timeSpan = previousTime = TimeSpan.Zero;
            VideoDuration.Content = timeSpan + " / " + duration;
            VideoProgess.Value = 0;
            IsPlaying = false;
        }

        private void OpenFile_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var ofd = new OpenFileDialog();

            ofd.Filter = "All Media|*.mp3;*.wma;*.wav;*.mp4;*.m4p;*.m4v;*.mpg;*.mp2;*.mpeg;*.mpe;*.mpv;*.flv;*.gif;*.avi;*.qt;*.mov;*.wmv;*.vob|" +
                "All Videos|*.mp4;*.m4p;*.m4v;*.mpg;*.mp2;*.mpeg;*.mpe;*.mpv;*.flv;*.gif;*.avi;*.qt;*.mov;*.wmv;*.vob|" +
                "flv (*.flv)|*.flv|" +
                "gif (*.gif)|*.gif|" +
                "avi (*.avi)|*.avi|" +
                "QuickTime (*.qt, *.mov)|*.qt;*.mov|" +
                "wmv (*.wmv)|*.wmv|" +
                "MPEG (*.mp4, *.m4p, *.m4v, *.mpg, *.mp2, *.mpeg, *.mpe, *.mpv)|*.mp4;*.m4p;*.m4v;*.mpg;*.mp2;*.mpeg;*.mpe;*.mpv";
            ofd.FilterIndex = 0;
            if(ofd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    if(System.IO.Path.GetExtension(ofd.FileName) == ".VOB")
                    {
                        MediaPlayer.Visibility = Visibility.Collapsed;
                        DvdPlayer.Visibility = Visibility.Visible;
                        DvdPlayer.DvdDirectory = System.IO.Path.GetFullPath(System.IO.Path.Combine(ofd.FileName, @"..\"));
                        DvdPlayer.Play();
                    }
                    else
                    {
                        MediaPlayer.Source = new Uri(ofd.FileName);
                        MediaPlayer.Play();
                    }
                    
                    IsPlaying = true;

                    source = ofd.FileName;
                    Title = System.IO.Path.GetFileNameWithoutExtension(ofd.SafeFileName);
                    
                }
                catch
                {
                    MediaPlayer.Source = null;
                    System.Windows.MessageBox.Show("Failed to load file", "Error", MessageBoxButton.OK);

                    Title = "GoPlay";
                }
            }

            
        }

        private void MediaPlayer_MediaOpened(object sender, RoutedEventArgs e)
        {
            if (!MediaPlayer.HasVideo && !MediaPlayer.HasAudio) return;

            if (MediaPlayer.HasVideo)
            {
                AppIcon.Visibility = Visibility.Hidden;
                TextBlock.Visibility = Visibility.Collapsed;
                NowPlaying.Visibility = Visibility.Collapsed;
            }
            else
            {
                AppIcon.Visibility = Visibility.Visible;
                TextBlock.Visibility = Visibility.Visible;
                NowPlaying.Visibility = Visibility.Visible;
                NowPlaying.Text = Title;
                TextBlock.Margin = new Thickness((MainGrid.ActualWidth / 2) - (MeasureString(TextBlock.Text).Width / 2), MainGrid.ActualHeight / 4.7, 0, 0);
                NowPlaying.Margin = new Thickness((MainGrid.ActualWidth / 2) - (MeasureString(NowPlaying.Text).Width / 2), MainGrid.ActualHeight / 3.76, 0, 0);
            }
            
            duration = MediaPlayer.NaturalDuration.TimeSpan;
            duration = new TimeSpan(duration.Hours, duration.Minutes, duration.Seconds);

            VideoProgess.Maximum = MediaPlayer.NaturalDuration.TimeSpan.TotalSeconds;
            VideoProgess.Value = 0;
            timeSpan = TimeSpan.Zero;
            VideoDuration.Content = "00:00:00 " + "/ " + duration;
            timer.Start();
        }

        private void MediaPlayer_MediaEnded(object sender, RoutedEventArgs e)
        {
            IsPlaying = false;
            timer.Stop();
            VideoDuration.Content = duration + " / " + duration;
            VideoProgess.Value = VideoProgess.Maximum;

            if (isLoop)
            {
                MediaPlayer.Source = null;
                MediaPlayer.Source = new Uri(source);

                timeSpan = previousTime = TimeSpan.Zero;
                VideoDuration.Content = timeSpan + " / " + duration;
                VideoProgess.Value = 0;

                IsPlaying = true;
            }
            
        }

        private void MediaPlayer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PlayOrPause();
        }

        private void MainGrid_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (ButtonGrid.Visibility == Visibility.Visible) return;
            
            if (Mouse.GetPosition(this).Y >= this.ActualHeight - ButtonGrid.ActualHeight)
            {
                ButtonGrid.Background.Opacity = 1;
                ButtonGrid.Visibility = Visibility.Visible;
            }
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MediaPlayer.Width = MainGrid.ActualWidth;
            MediaPlayer.Height = MainGrid.ActualHeight;


            ButtonGrid.Width = MainGrid.ActualWidth - 20;
            //ButtonGrid.Margin = new Thickness(10, MainGrid.ActualHeight - ButtonGrid.ActualHeight - 10, 10, 5);
            VideoProgess.Width = MainGrid.ActualWidth - 60;

            double playMargin = (MainGrid.ActualWidth / 2) - (PlayPause.ActualWidth / 2);
            double stopMargin = (MainGrid.ActualWidth / 2) - (PlayPause.ActualWidth / 2) + 150;
            double openMargin = (MainGrid.ActualWidth / 2) - (PlayPause.ActualWidth / 2) - 150;

            AppIcon.Margin = new Thickness(MainGrid.ActualWidth / 2 - AppIcon.ActualWidth / 2, MainGrid.ActualHeight / 2 - AppIcon.ActualHeight / 2, 0, 0);
            PlayPauseBorder.Margin = new Thickness(playMargin, 35, 0, 0);
            StopBorder.Margin = new Thickness(stopMargin, 35, 0, 0);
            OpenFileBorder.Margin = new Thickness(openMargin, 35, 0, 0);
            SoundBorder.Margin = new Thickness(MainGrid.ActualWidth - Volume.ActualWidth - SoundBorder.ActualWidth - 50, 64, 0, 20);
            Volume.Margin = new Thickness(SoundBorder.Margin.Left + SoundBorder.ActualWidth + 20, 72, 0, 31);
            TextBlock.Margin = new Thickness((MainGrid.ActualWidth / 2) - (MeasureString(TextBlock.Text).Width / 2), MainGrid.ActualHeight / 4.7, 0, 0);
            NowPlaying.Margin = new Thickness((MainGrid.ActualWidth / 2) - (MeasureString(NowPlaying.Text).Width / 2), MainGrid.ActualHeight / 3.76, 0, 0);
        }

        private void ButtonGrid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            ButtonGrid.Visibility = Visibility.Hidden;
        }

        private void ButtonGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ButtonGrid.Visibility != Visibility.Visible) return;

            Timer t = new Timer();
            Timer fade = new Timer();
            fade.Interval = 50;
            t.Interval = 2000;

            t.Tick += (s, a) =>
            {
                if (!ButtonGrid.IsMouseOver)
                {
                    fade.Start();
                    t.Stop();
                }
            };
            fade.Tick += (s, a) =>
            {
                if (ButtonGrid.IsMouseOver)
                {
                    ButtonGrid.Background.Opacity = 1;
                    ButtonGrid.Visibility = Visibility.Visible;
                    fade.Stop();
                }
                ButtonGrid.Background.Opacity -= 0.1;
                if (ButtonGrid.Background.Opacity <= 0)
                {
                    ButtonGrid.Visibility = Visibility.Hidden;
                    fade.Stop();
                }
            };
            
            t.Start();
        }

        private void VideoProgess_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (MediaPlayer.Source == null) return;

            isMouseDown = true;
            double percentile = Mouse.GetPosition(VideoProgess).X / VideoProgess.ActualWidth;
            VideoProgess.Value = VideoProgess.Maximum * percentile;

            MediaPlayer.Position = new TimeSpan(0, 0, (int)VideoProgess.Value);
        }

        private void VideoProgess_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (MediaPlayer.Source == null) return;

            double time = VideoProgess.Maximum * (Mouse.GetPosition(VideoProgess).X / VideoProgess.ActualWidth);
            
            VideoProgess.ToolTip = new TimeSpan(0, 0, (int)time);
        }
        
        private void Sound_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Volume.Value != 0)
                Volume.Value = 0;
            else
                Volume.Value = previousVolume;
        }
        
        private void Volume_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if(e.Delta > 0)
            {
                Volume.Value += 2;
            }
            else
            {
                Volume.Value -= 2;
            }
            
            if(Volume.Value == 0 || Volume.Value == 100)
            {
                VolumeLabel.Visibility = Visibility.Visible;
                VolumeLabel.Content = "Volume: " + (int)Volume.Value + "%";

                volumeTimeInterval.Stop();
                volumeTimeInterval.Start();
            }
        }

        private void Volume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            MediaPlayer.Volume = e.NewValue / 100;

            if(e.NewValue != 0) previousVolume = e.NewValue;

            if (MediaPlayer.Volume <= 0)
            {
                Sound.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/mute.png", UriKind.Absolute));
            }
            else
            {
                Sound.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/sound.png", UriKind.Absolute));
            }

            if (!IsLoaded) return;
            VolumeLabel.Visibility = Visibility.Visible;
            VolumeLabel.Content = "Volume: " + (int)Volume.Value + "%";

            volumeTimeInterval.Stop();
            volumeTimeInterval.Start();
        }
        
        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                PlayOrPause();
            }
            else if (e.Key == Key.LeftAlt)
            {
                e.Handled = true;
            }
        }
        
        private void Window_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (isMouseDown)
            {
                double percentile = Mouse.GetPosition(VideoProgess).X / VideoProgess.ActualWidth;
                VideoProgess.Value = VideoProgess.Maximum * percentile;

                MediaPlayer.Position = new TimeSpan(0, 0, (int)VideoProgess.Value);
            }
        }

        private void Window_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isMouseDown)
                isMouseDown = false;
            
        }
        
        private void UriLink_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var prompt = new UriPrompt();
            prompt.ShowDialog();

            if(prompt.DialogResult.HasValue && prompt.DialogResult.Value)
            {
                
            }
        }
        
        private void Loop_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isLoop)
            {
                isLoop = false;
                Loop.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/loopOff.png", UriKind.Absolute));
            }
            else
            {
                isLoop = true;
                Loop.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/loopOn.png", UriKind.Absolute));
            }
        }

        private void TextBlock_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (TextBlock.Visibility != Visibility.Visible) return;

            Timer t = new Timer();
            Timer fade = new Timer();
            fade.Interval = 50;
            t.Interval = 3000;

            t.Tick += (s, a) =>
            {
                fade.Start();
                t.Stop();
            };
            fade.Tick += (s, a) =>
            {
                TextBlock.Foreground.Opacity -= 0.1;
                NowPlaying.Foreground.Opacity -= 0.075;
                if (NowPlaying.Foreground.Opacity <= 0)
                {
                    TextBlock.Visibility = Visibility.Collapsed;
                    NowPlaying.Visibility = Visibility.Collapsed;
                    TextBlock.Foreground.Opacity = 1;
                    NowPlaying.Foreground.Opacity = 1;
                    fade.Stop();
                }
            };

            t.Start();
        }

        private void PlayOrPause()
        {
            if (MediaPlayer.Source == null) return;

            if (isPlaying)
            {
                MediaPlayer.Pause();
                timer.Stop();
                IsPlaying = false;
            }
            else
            {
                if(MediaPlayer.Position >= MediaPlayer.NaturalDuration.TimeSpan)
                {
                    MediaPlayer.Position = timeSpan = TimeSpan.Zero;
                    VideoDuration.Content = timeSpan + " / " + duration;
                    VideoProgess.Value = 0;
                }
                MediaPlayer.Play();
                timer.Start();
                IsPlaying = true;
            }
        }

        private Size MeasureString(string candidate)
        {
            var formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentUICulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(this.NowPlaying.FontFamily, this.NowPlaying.FontStyle, this.NowPlaying.FontWeight, this.NowPlaying.FontStretch),
                this.NowPlaying.FontSize,
                Brushes.Black);

            return new Size(formattedText.Width, formattedText.Height);
        }

    }
}
