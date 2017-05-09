using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Youtube {

    /// <summary>
    /// Interaction logic for YoutubeVideo.xaml
    /// </summary>
    public partial class YoutubeVideo: Window {

        public string VideoID { get; private set; }
        public string VideoUrl => $@"http://www.youtube.com/watch?v={VideoID}";

        public YoutubeVideo(SearchResult _res) {
            InitializeComponent();

            VideoID = _res.Id.VideoId;
            Loaded += YoutubeVideo_Loaded;
        }
        public YoutubeVideo(string _id) {
            InitializeComponent();

            VideoID = _id;
            Loaded += YoutubeVideo_Loaded;
        }

        public void Maximize() {
            Width = SystemParameters.FullPrimaryScreenWidth;
            Height = SystemParameters.FullPrimaryScreenHeight;
            Left = 0;
            Top = 0;
            Show();
        }
        public void Minimize() {
            ShowInTaskbar = false;
            Hide();
        }

        private void YoutubeVideo_Loaded(object sender, RoutedEventArgs e) {
            WindowState = YoutubeHook.State;

            if (WindowState == WindowState.Maximized) {
                Maximize();
            } else {
                Minimize();
            }

            KeyDown += (o, args) => {
                if(args.Key == Key.Escape) { Hide(); }
            };

            Browser.Navigate(VideoUrl);
            Closing += (o, args) => Browser.Dispose();
        }
        
    }
}
