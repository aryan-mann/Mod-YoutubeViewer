﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ModuleAPI;

namespace Youtube {
    
    [ApplicationHook]    
    public class YoutubeHook : ModuleAPI.Module {

        #region Useless Stuff
        public override string Author {
            get { return "Aryan Mann"; }
        }
        public override string SemVer {
            get { return "0.1.0"; }
        }
        public override string Name {
            get { return "Youtube Viewer"; }
        }
        public override Uri Website {
            get { return new Uri("http://www.aryanmann.com/"); }
        }
        public override void ConfigureSettings() {
            return;
        }
        public override void OnInitialized() {
            return;
        }
        public override void OnShutdown() {
            return;
        }
        #endregion

        private Dictionary<string, Regex> _RegisteredCommands = new Dictionary<string, Regex>() {
            ["play id"] = new Regex(@"^yplay (?<id>[A-Za-z0-9]{11})$"), //VideoIDs are 11 characters long
            ["search"] = new Regex(@"^ysearch (?<name>.+)$")
        };
        public override Dictionary<string, Regex> RegisteredCommands {
            get {
                return _RegisteredCommands;
            }
        }

        public static string ApiKey { get; } = "AIzaSyDmZ5rGzV38mrGfcSMPegvx8xxndSHmnT4";

        public override void OnCommandRecieved(string CommandName, string UserInput) {
            if(CommandName == "play id") {
                string id = _RegisteredCommands[CommandName].Match(UserInput).Groups["id"].Value.ToString();
                new YoutubeVideo(id).Show();
            }

            if(CommandName == "search") {
                string name = _RegisteredCommands[CommandName].Match(UserInput).Groups["name"].Value.ToString();
                if(!string.IsNullOrWhiteSpace(name)){ new YoutubeSearch(name).Show(); }
            }
        }

    }
}
