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
    public partial class YoutubeVideo : Window {
                    
        string videoID = "";
        SearchResult video;
        string videoUrl {
            get {
                return $@"https://www.youtube.com/embed/{videoID}?autoplay=1&fs=0&modestbranding=0&showinfo=0&iv_load_policy=3";
            }
        }
        string queryUrl {
            get {
                return $@"https://www.googleapis.com/youtube/v3/videos?id={videoID}&key={YoutubeHook.ApiKey}&part=status&fields=items(id,status(uploadStatus,privacyStatus))";
            }
        }
        
        /// <summary>
        /// Open a window that plays youtube videos.
        /// </summary>
        /// <param name="_videoID">ID of the video</param>
        public YoutubeVideo(SearchResult _res) {
            InitializeComponent();
            
            videoID = _res.Id.VideoId;
            video = _res;
            Loaded += YoutubeVideo_Loaded;

            Title = video.Snippet.Title;

            Browser.Navigated += Browser_Navigated;

            KeyDown += (sender, e) => {
                if(e.Key == Key.Escape) {
                    Close();
                }
            };

            Closing += YoutubeVideo_Closing;
        }
        public YoutubeVideo(string _id) {
            InitializeComponent();

            videoID = _id;
            Loaded += YoutubeVideo_Loaded;

            Browser.Navigated += Browser_Navigated;

            KeyDown += (sender, e) => {
                if(e.Key == Key.Escape) {
                    Close();
                }
            };

            Closing += YoutubeVideo_Closing;
        }


        private void Browser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e) {
            Browser.Navigated += (s, e2) => {
                dynamic activeX = Browser.GetType().InvokeMember("ActiveXInstance",
                    BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                    null, Browser, new object[] { });
                activeX.Silent = true;
            };
        }

        private void YoutubeVideo_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            Browser.Dispose();
        }

        private void YoutubeVideo_Loaded(object sender, RoutedEventArgs e) {
            Browser.Navigate(videoUrl);



            Width = SystemParameters.PrimaryScreenWidth / 1.4;
            Height = SystemParameters.PrimaryScreenHeight / 1.4;

            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;
        }
    }
}
