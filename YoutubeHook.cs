using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ModuleAPI;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using Google.Apis.Services;
using System.IO;
using System.Windows;

namespace Youtube {

    [ApplicationHook]
    public class YoutubeHook: ModuleAPI.Module {

        #region Useless Stuff
        public override string Name => "Youtube Viewer";
        public override string SemVer => "0.2.0";
        public override string Author => "Aryan Mann";
        public override Uri Website => new Uri("http://www.aryanmann.com/");
        public override string Prefix => "yt";

        public override async Task ConfigureSettings() { await Task.CompletedTask; }
        public override async Task OnInitialized() { await Task.CompletedTask; }
        public override async Task OnShutdown() { await Task.CompletedTask; }
        #endregion

        public override Dictionary<string, Regex> RegisteredCommands => new Dictionary<string, Regex>() {
            ["Play ID"] = new Regex(@"^play (?<id>[A-Za-z0-9]{11})$"),
            ["Search"] = new Regex(@"^search (?<name>.+?)$"),
            ["Lucky Search"] = new Regex(@"^lucky (?<name>.+)$"),
            ["Choose Option"] = new Regex(@"^choose (?<choice>\d{1,2})$"),
            ["Close"] = new Regex(@"^(close|stop)$"),
            ["State"] = new Regex(@"^state (?<state>(min|max)(imize)?)$")
        };

        //ENTER API KEY HERE
        public static string ApiKey { get; } = "AIzaSyDmZ5rGzV38mrGfcSMPegvx8xxndSHmnT4";

        public SearchListResponse LastSearchResponse;
        public static YoutubeVideo Current;
        public static WindowState State = WindowState.Maximized;

        private YoutubeVideo _video;    

        public override async Task OnCommandRecieved(Command cmd) {
            
            string commandName = cmd.LocalCommand;
            string userInput = cmd.UserInput;
            

            if (string.IsNullOrEmpty(ApiKey)) {

                if (!cmd.IsLocalCommand) {
                    cmd.Respond("Invalid API Key.");
                }

                return;
            }

            if(commandName == "Play ID") {
                string id = RegisteredCommands[commandName].Match(userInput).Groups["id"].Value.ToString();

                if(Current != null) {
                    if(Current.IsLoaded) {
                        Current.Close();
                    }
                }

                Application.Current.Dispatcher.Invoke(() => {
                    Current = new YoutubeVideo(id);
                    Current.Show();
                });
            }

            if(commandName == "Search") {
                Match m = RegisteredCommands[commandName].Match(userInput);

                string name = m.Groups["name"].Value.ToString();
                if(!string.IsNullOrWhiteSpace(name)) {
                    if(cmd.IsLocalCommand) {

                        Application.Current.Dispatcher.Invoke(() => {
                            new YoutubeSearch(name).Show();
                        });

                    } else {
                        YouTubeService ys = new YouTubeService(new BaseClientService.Initializer() {
                            ApiKey = YoutubeHook.ApiKey,
                            ApplicationName = "Butler-YoutubeViewer"
                        });

                        SearchResource.ListRequest req = new SearchResource.ListRequest(ys, "snippet") {
                            Q = name,
                            MaxResults = 30
                        };

                        LastSearchResponse = req.Execute();

                        string output = "Search Results:-\n";

                        for (int i = 0; i < LastSearchResponse.Items.Count; i++) {
                            output += $"{i+1}.] {LastSearchResponse.Items[i].Snippet.Title}\n";
                        }
                        
                        cmd.Respond(output);
                    }
                }
            }

            if(commandName == "Lucky Search") {
                Match m = RegisteredCommands[commandName].Match(userInput);

                YouTubeService ys = new YouTubeService(new BaseClientService.Initializer() {
                    ApiKey = YoutubeHook.ApiKey,
                    ApplicationName = "Butler-YoutubeViewer"
                });

                SearchResource.ListRequest req = new SearchResource.ListRequest(ys, "snippet") {
                    Q = m.Groups["name"].Value.ToString(),
                    MaxResults = 1
                };

                SearchListResponse resp = req.Execute();

                if(Current != null) { if(Current.IsLoaded) Current.Close(); }

                Application.Current.Dispatcher.Invoke(() => {
                    Current = new YoutubeVideo(resp.Items[0].Id.VideoId);
                    Current.Show();
                });

            }

            if (commandName == "Choose Option") {
                string choiceString = RegisteredCommands[cmd.LocalCommand].Match(cmd.UserInput).Groups["choice"].Value;
                int choiceInt;

                if(LastSearchResponse == null) { cmd.Respond("You didn't search for anything."); return; }
                if(!int.TryParse(choiceString, out choiceInt)) { cmd.Respond("That is not a number."); return; }

                choiceInt -= 1;
                if(choiceInt < 0 || choiceInt > LastSearchResponse.Items.Count-1) { cmd.Respond("Dude.. Enter a acceptable value."); return; }

                if(Current != null) { if(Current.IsLoaded) Current.Close(); }

                Application.Current.Dispatcher.Invoke(() => {
                    Current = new YoutubeVideo(LastSearchResponse.Items[choiceInt].Id.VideoId);
                    Current.Show();
                });


                // Should I clear the search options after an option has been selected?
                //LastSearchResponse = null;
            }

            if (commandName == "Close") {
                if (Current != null) {
                    if (Current.IsLoaded) {
                        Current.Close();
                    }
                }
            }

            if (commandName == "State") {
                string state = RegisteredCommands[commandName].Match(userInput).Groups["state"].Value.ToLower();

                switch (state) {
                    case "min": 
                    case "minimize": State = WindowState.Minimized;
                        break;
                    case "max":
                    case "maximize": State = WindowState.Maximized;
                        break;
                }

                if (Current != null) {
                    if (Current.IsLoaded) {

                        if (State == WindowState.Maximized) {
                            Current.Maximize();
                        } else {
                            Current.Minimize();
                        }

                    }
                }
            }
        }


    }
}
