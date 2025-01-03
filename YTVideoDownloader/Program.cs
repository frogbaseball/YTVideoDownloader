using Spectre.Console;
using System;
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
                    var urlValid1 = await IsURLValidAsync();
                    if (urlValid1 == true) {
                        Task[] tasks1 = { DownloadVideoAsync() };
                        await Task.WhenAll(tasks1);
                        PressAnyKeyToContinue();
                    }
                    if (urlValid1 == false) {
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
                    var urlValid2 = await IsURLValidAsync();
                    if (urlValid2 == true) {
                        Task[] tasks2 = { DownloadAudioAsync() };
                        await Task.WhenAll(tasks2);
                        PressAnyKeyToContinue();                        
                    }
                    if (urlValid2 == false) {
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
        static void SetURL() {
            AnsiConsole.WriteLine("Set the URL of the video you want to download (type '1' to exit):");
            Input();
            url = input;
        }
        static async Task<bool> IsURLValidAsync() {
            try {
                var youtube = new YoutubeClient();
                var video = await youtube.Videos.GetAsync(url);
                return true;
            } catch {
                return false;
            }
        }
        static async Task DownloadVideoAsync() {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(url);
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
                .First(s => s.VideoQuality.Label == "480p");
            // mix audio and video streams
            var streamInfos = new IStreamInfo[] { audioStreamInfo, videoStreamInfo };
            AnsiConsole.WriteLine($"\nDownloading... (AUDIO: {audioStreamInfo.Size} VIDEO: {videoStreamInfo.Size}) \n");
            await youtube.Videos.DownloadAsync(streamInfos, new ConversionRequestBuilder($"{video.Title}.mp4").SetFFmpegPath($"{Environment.CurrentDirectory}\\..\\..\\ffmpeg-windows-x64\\ffmpeg.exe").Build());
            AnsiConsole.Markup($"[lime]Downloaded![/] in {path}\\{video.Title}.mp4 \n\n");

            //TODO:
            //Able to pick Video Quality that is available
            //Ensure that the file name has symbols it supports
            //Save the video to the set path
        }
        static async Task DownloadAudioAsync() {
            var youtube = new YoutubeClient();
            var video = await youtube.Videos.GetAsync(url);
            var streamManifest = await youtube.Videos.Streams.GetManifestAsync(url);
            var streamInfo = streamManifest.GetAudioOnlyStreams().GetWithHighestBitrate();
            var stream = await youtube.Videos.Streams.GetAsync(streamInfo);
            AnsiConsole.WriteLine($"\nDownloading... ({streamInfo.Size}) \n");
            await youtube.Videos.Streams.DownloadAsync(streamInfo, $"{path}\\{video.Title}.mp3");
            AnsiConsole.Markup($"[lime]Downloaded![/] in {path}\\{video.Title}.mp3 \n\n");
        }

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
        static void Input() {
            try {
                input = Console.ReadLine();
            } catch {
                Input();
            }
        }
        static void PressAnyKeyToContinue() {
            AnsiConsole.Markup("[lime]Press Any Key To Continue...[/] \n");
            Console.ReadKey();
        }
    }
}