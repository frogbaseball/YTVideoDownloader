using Spectre.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos.Streams;

namespace YTVideoDownloader {
    internal class Program {
        static private string path;
        static private string input;
        static private string url;
        static async Task Main(string[] args) {
            path = FileData.ReadFromFile();
            if (path == string.Empty || path == null) {
                path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                FileData.SaveToFile(path);
            }
            await Menu();
        }
        static async Task Menu() {
            AnsiConsole.Clear();
            var table = new Table();
            table.AddColumns("Mode", "Number");
            table.AddRow("Download YT Video By URL", "1");
            table.AddRow("Download YT Audio By URL", "2");
            table.AddRow("Change Download Path", "3");
            table.AddRow("Quit Application", "4");
            table.Border = TableBorder.Heavy;
            table.BorderColor(Color.Red);
            AnsiConsole.Write(table);
            AnsiConsole.WriteLine($"\n The current download path is: {path} \n");
            Input();
            switch (input) {
                case "1":
                    AnsiConsole.Clear();
                    SetURL();
                    bool urlValid = await IsURLValidAsync();
                    if (urlValid == true) {
                        Task[] tasks = { DownloadVideoAsync(url, path) };
                        await Task.WhenAll(tasks);
                        PressAnyKeyToContinue();
                    }
                    if (urlValid == false) {
                        if (url != "1") {
                            AnsiConsole.MarkupLine($"[red]\"{url}\"[/] is not a valid URL. \n");
                            PressAnyKeyToContinue();
                        }
                    }
                    await Menu();
                    break;
                case "2":
                    AnsiConsole.Clear();
                    SetURL();
                    bool urlValidd = await IsURLValidAsync();
                    if (urlValidd == true) {
                        Task[] tasks = { DownloadAudioAsync(url, path) };
                        await Task.WhenAll(tasks);
                        PressAnyKeyToContinue();                        
                    }
                    if (urlValidd == false) {
                        if (url != "1") {
                            AnsiConsole.MarkupLine($"[red]\"{url}\"[/] is not a valid URL. \n");
                            PressAnyKeyToContinue();
                        }
                    }
                    await Menu();
                    break;
                case "3":
                    await SetPathAsync();
                    break;
                case "4":
                    Environment.Exit(0);
                    break;
                default:
                    await Menu();
                    break;
            }
        }
        /// <summary>
        /// Asks the user to replace the old URL with a new one.
        /// </summary>
        static void SetURL() {
            AnsiConsole.WriteLine("Set the URL of the video you want to download (type '1' to exit):");
            Input();
            url = input;
        }
        /// <summary>
        /// Returns true if its possible to get metadata from the video using the URL.
        /// </summary>
        static async Task<bool> IsURLValidAsync() {
            try {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(url);
                return true;
            } catch {
                return false;
            }
        }
        /// <summary>
        /// Download youtube video using its URL.
        /// </summary>
        static async Task DownloadVideoAsync(string url, string path) {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(url);
            var videoName = CheckIfNameHasValidSymbols(video.Title);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
            // audio
            var audioStreamInfo = streamManifest
                .GetAudioStreams()
                .Where(s => s.Container == Container.Mp4)
                .GetWithHighestBitrate();
            // video
            var videoStreamInfo = streamManifest
                .GetVideoStreams()
                .Where(s => s.Container == Container.Mp4)
                .GetWithHighestVideoQuality();
            // mix audio and video streams
            var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
            AnsiConsole.WriteLine($"\nDownloading... (AUDIO: {audioStreamInfo.Size} VIDEO: {videoStreamInfo.Size}) \n");
            //mix the stream infos and create a file in the bin/debug folder
            await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder($"{videoName}.mp4").SetFFmpegPath($"{Environment.CurrentDirectory}\\..\\..\\ffmpeg-windows-x64\\ffmpeg.exe").Build());
            AnsiConsole.Markup($"[lime]Downloaded![/]\nin {path}\\{videoName}.mp4 \n\n");
            //move file from the bin\debug folder to the desired path
            File.Move($"{Environment.CurrentDirectory}\\{videoName}.mp4", $"{path}\\{videoName}.mp4");
        }
        /// <summary>
        /// Download youtube audio using its URL.
        /// </summary>
        static async Task DownloadAudioAsync(string url, string path) {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(url);
            var videoName = CheckIfNameHasValidSymbols(video.Title);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            AnsiConsole.WriteLine($"\nDownloading... ({streamInfo.Size}) \n");
            //create file in the desired path
            await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{path}\\{videoName}.mp3");
            AnsiConsole.Markup($"[lime]Downloaded![/]\nin {path}\\{videoName}.mp3\n\n");
        }
        /// <summary>
        /// Changes the current desired path to a new, user set one.
        /// </summary>
        static async Task SetPathAsync() {
            AnsiConsole.Clear();
            AnsiConsole.WriteLine($"Set a new download path (type '1' to exit): ");
            Input();
            if (input == "1") {
                await Menu();
            }

            if (!Directory.Exists(input)) {
                AnsiConsole.MarkupLine($"[red]\"{input}\"[/] is not a valid path. \n");
                PressAnyKeyToContinue();
                await Menu();
            } else {
                path = input;
                FileData.SaveToFile(path);
                await Menu();
            }
        }
        /// <summary>
        /// Replaces specific characters in a string with an X, if theyre not supported as a windows file name.
        /// </summary>
        /// <returns>A string that can be a file name.</returns>
        static string CheckIfNameHasValidSymbols(string name) {
            char[] newName = name.ToCharArray();
            for (int i = 0; i < newName.Length; i++) {
                if (Path.GetInvalidFileNameChars().Contains(newName[i])) {
                    newName[i] = 'X';
                }
            }
            return new string(newName);
        }
        /// <summary>
        /// User Input.
        /// </summary>
        static void Input() {
            try {
                input = Console.ReadLine();
            } catch {
                Input();
            }
        }
        /// <summary>
        /// Asks the user to press any key to continue.
        /// </summary>
        static void PressAnyKeyToContinue() {
            AnsiConsole.Markup("[lime]Press Any Key To Continue...[/] \n");
            Console.ReadKey();
        }
    }
}