using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Interaction logic for YoutubeSearch.xaml
    /// </summary>
    public partial class YoutubeSearch: Window {
        private string searchQ = "";
        List<SearchResult> _results = new List<SearchResult>();

        public YoutubeSearch(string name) {
            InitializeComponent();

            searchQ = name;

            this.Loaded += YoutubeSearch_Loaded;
            this.KeyDown += (sender, e) => {
                if(e.Key == Key.Escape) {
                    Close();
                }
            };

            stacker.KeyDown += Stacker_KeyDown;
        }

        private void Stacker_KeyDown(object sender, KeyEventArgs e) {
            if(e.Key != Key.Enter) { return; }

            if(stacker.SelectedItem != null) {
                ListBoxItem lbi = (ListBoxItem) stacker.SelectedValue;
                if(lbi != null) {
                    new YoutubeVideo(((SearchResult) lbi.DataContext)).Show();
                    Close();
                }
            }
        }
        

        private void YoutubeSearch_Loaded(object sender, RoutedEventArgs e) {
            Width = SystemParameters.PrimaryScreenWidth / 1.4;
            Height = SystemParameters.PrimaryScreenHeight / 1.4;

            Left = (SystemParameters.PrimaryScreenWidth - Width) / 2;
            Top = (SystemParameters.PrimaryScreenHeight - Height) / 2;

            Task.Factory.StartNew(() => GetResults());
        }

        private void GetResults() {
            YouTubeService ys = new YouTubeService(new BaseClientService.Initializer() {
                ApiKey = YoutubeHook.ApiKey,
                ApplicationName = "Butler-YoutubeViewer"
            });

            SearchResource.ListRequest req = new SearchResource.ListRequest(ys, "snippet") {
                Q = searchQ,
                MaxResults = 50
            };

            SearchListResponse resp = req.Execute();
            _results = resp.Items.Where(itm => itm.Id.Kind == "youtube#video").ToList();

            Dispatcher.Invoke(() => UpdateItems());
        }

        private void UpdateItems() {
            stacker.Items.Clear();

            _results.ForEach(item => {

                ListBoxItem lbi = new ListBoxItem() {
                    Content = $@"{item.Snippet.Title} | {item.Snippet.ChannelTitle}",
                    FontSize = 16,
                    IsTabStop = true,
                    DataContext = item
                };

                lbi.MouseDoubleClick += (sender, e) => {
                    new YoutubeVideo(item).Show();
                    Close();
                };
                                                                
                stacker.Items.Add(lbi);
            });
                        
        }

    }
}
