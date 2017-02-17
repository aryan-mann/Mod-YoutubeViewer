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
        public override string SemVer => "0.1.0";
        public override string Author => "Aryan Mann";
        public override Uri Website => new Uri("http://www.aryanmann.com/");
        public override string Prefix => "yt";

        public override void ConfigureSettings() { }
        public override void OnInitialized() { }
        public override void OnShutdown() { }
        #endregion

        public override Dictionary<string, Regex> RegisteredCommands => new Dictionary<string, Regex>() {
            ["play id"] = new Regex(@"^play (?<id>[A-Za-z0-9]{11})$"), //VideoIDs are 11 characters long
            ["search"] = new Regex(@"^search (?<name>.+?)$"),
            ["force search"] = new Regex(@"^force (?<name>.+)$"),
            ["choose"] = new Regex(@"^choose (?<choice>\d{1,2})$"),
            ["close"] = new Regex(@"^(close|stop)$")
        };

        //ENTER API KEY HERE
        public static string ApiKey { get; } = "AIzaSyDmZ5rGzV38mrGfcSMPegvx8xxndSHmnT4";

        public SearchListResponse LastSearchResponse;
        public YoutubeVideo Current;

        public override void OnCommandRecieved(Command cmd) {

            string commandName = cmd.LocalCommand;
            string userInput = cmd.UserInput;

            if(commandName == "play id") {
                string id = RegisteredCommands[commandName].Match(userInput).Groups["id"].Value.ToString();
                new YoutubeVideo(id).Show();
            }

            if(commandName == "search") {
                Match m = RegisteredCommands[commandName].Match(userInput);

                string name = m.Groups["name"].Value.ToString();
                if(!string.IsNullOrWhiteSpace(name)) {
                    if(cmd.IsLocalCommand) {
                        new YoutubeSearch(name).Show();
                    } else {
                        YouTubeService ys = new YouTubeService(new BaseClientService.Initializer() {
                            ApiKey = YoutubeHook.ApiKey,
                            ApplicationName = "Butler-YoutubeViewer"
                        });

                        SearchResource.ListRequest req = new SearchResource.ListRequest(ys, "snippet") {
                            Q = name,
                            MaxResults = 20
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

            if(commandName == "force search") {
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

                Current = new YoutubeVideo(resp.Items[0].Id.VideoId);
                Current.Show();
            }

            if (commandName == "choose") {
                string choiceString = RegisteredCommands[cmd.LocalCommand].Match(cmd.UserInput).Groups["choice"].Value;
                int choiceInt;

                if(LastSearchResponse == null) { cmd.Respond("You didn't search for anything."); return; }
                if(!int.TryParse(choiceString, out choiceInt)) { cmd.Respond("That is not a number."); return; }

                choiceInt -= 1;
                if(choiceInt < 0 || choiceInt > LastSearchResponse.Items.Count-1) { cmd.Respond("Dude.. Enter a acceptable value."); return; }

                if(Current != null) { if(Current.IsLoaded) Current.Close(); }

                Current = new YoutubeVideo(LastSearchResponse.Items[choiceInt].Id.VideoId);
                Current.Show();

                // Should I clear the search options after an option has been selected?
                //LastSearchResponse = null;
            }

            if (commandName == "close") {
                if (Current != null) {
                    if (Current.IsLoaded) {
                        Current.Close();
                    }
                }
            }
        }


    }
}
