using CSGSI;
using CSGSI.Nodes;
using Downloader;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

namespace Launcher.Utils
{
    public static class Game
    {
        private static Process? _process;
        private static GameStateListener? _listener;
        private static int _port;
        private static MapNode? _node;

        private static string _map = "main_menu";
        private static int _scoreCT = 0;
        private static int _scoreT = 0;


        public static async Task DownloadGCVer(string reason)
        {

            Terminal.Error($"{reason} Downloading cc.exe ...");
            // sry if i mess smth up, never used this lib ~ ZRD/Zordon1337
            DownloadService downloadService = new();
            await downloadService.DownloadFileTaskAsync("https://patch.classiccounter.cc/cc.exe", _process?.StartInfo.FileName);
            // wait for download
            while (downloadService.Status != DownloadStatus.Completed) ;
            Terminal.Print("GC executable downloaded successfully.");
        }

        public static async Task<bool> Launch()
        {
            List<string> arguments = Argument.GenerateGameArguments();
            if (arguments.Count > 0) Terminal.Print($"Arguments: {string.Join(" ", arguments)}");

            string directory = Directory.GetCurrentDirectory();
            Terminal.Print($"Directory: {directory}");

            string gameStatePath = $"{directory}/csgo/cfg/gamestate_integration_cc.cfg";
            
            if (!Argument.Exists("--disable-rpc"))
            {
                _port = GeneratePort();

                if (Argument.Exists("--debug-mode"))
                    Terminal.Debug($"Starting Game State Integration with TCP port {_port}.");

                _listener = new($"http://localhost:{_port}/");
                _listener.NewGameState += OnNewGameState;
                _listener.Start();

                await File.WriteAllTextAsync(gameStatePath,
@"""ClassicCounter""
{
	""uri""                         ""http://localhost:" + _port + @"""
	""timeout""                     ""5.0""
	""auth""
	{
		""token""				    """ + $"ClassicCounter {Version.Current}" + @"""
	}
	""data""
	{
		""provider""              	""1""
		""map""                   	""1""
		""round""                 	""1""
		""player_id""				""1""
		""player_weapons""			""1""
		""player_match_stats""		""1""
		""player_state""			""1""
		""allplayers_id""			""1""
		""allplayers_state""		""1""
		""allplayers_match_stats""	""1""
	}
}"
                );
            }
            else if (File.Exists(gameStatePath)) File.Delete(gameStatePath);

            _process = new Process();
            if (!Argument.Exists("--gc"))
            {
                if (Argument.Exists("--debug-mode"))
                    Terminal.Debug("Launching the game without Game Coordinator...");
                _process.StartInfo.FileName = $"{directory}\\csgo.exe";
            }
            else
            {
                if (Argument.Exists("--debug-mode"))
                    Terminal.Debug("Launching the game with Game Coordinator...");
                _process.StartInfo.FileName = $"{directory}\\cc.exe";
                if(!File.Exists(_process.StartInfo.FileName))
                {
                    await DownloadGCVer("GC executable not found.");
                }
                // getting md5 checksum from cc.exe 
                // ps: Method Patch.GetHash() is private, since its not my code im not gonna make it public just use custom code ~ ZRD
                var md5 = MD5.Create();
                var stream = File.OpenRead(_process.StartInfo.FileName);
                var hash = md5.ComputeHash(stream);
                var res = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                stream.Close();

                string responseString = await Api.ClassicCounter.GetPatches();

                JObject responseJson = JObject.Parse(responseString);

                if (responseJson["files"] != null)
                {
                    var patches = responseJson["files"]!.ToObject<Patch[]>()!.ToList();
                    var cc = patches?.Find(patches => patches.File == "cc.exe");

                    // wtf?
                    // i mean, most likely it won't happen but just in case
                    if(cc == null)
                    {
                        if (Argument.Exists("--debug-mode"))
                            Terminal.Error("Couldn't find cc.exe patch in API response.");
                        else
                            Terminal.Error("Error occurred on api side, please try again later.");
                        return false;
                    } else
                    {
                        if(cc.Hash != res)
                        {
                            if (Argument.Exists("--debug-mode"))
                                Terminal.Debug("cc.exe hash doesn't match, downloading new version...");
                            await DownloadGCVer("Game Coordinator executable hash mismatch.");
                        }
                        else
                        {
                            if (Argument.Exists("--debug-mode"))
                                Terminal.Debug("cc.exe hash is ok");
                        }
                    }
                } else
                {
                    Terminal.Error("Couldn't get patches from API, please try again later.");
                    return false;
                }
                   


            }
            _process.StartInfo.Arguments = string.Join(" ", arguments);

            return _process.Start();
        }

        public static async Task Monitor()
        {
            while (true)
            {
                if (_process == null)
                    break;

                try
                {
                    Process.GetProcessById(_process.Id);
                }
                catch
                {
                    Environment.Exit(1);
                }

                if (_node != null && _node.Name.Trim().Length != 0)
                {
                    if (_map != _node.Name)
                    {
                        _map = _node.Name;
                        _scoreCT = _node.TeamCT.Score;
                        _scoreT = _node.TeamT.Score;

                        Discord.SetDetails(_map);
                        Discord.SetState($"Score → {_scoreCT}:{_scoreT}");
                        Discord.SetTimestamp(DateTime.UtcNow);
                        Discord.SetLargeArtwork($"https://assets.classiccounter.cc/maps/default/{_map}.jpg");
                        Discord.SetSmallArtwork("icon");
                        Discord.Update();
                    }

                    if (_scoreCT != _node.TeamCT.Score || _scoreT != _node.TeamT.Score)
                    {
                        _scoreCT = _node.TeamCT.Score;
                        _scoreT = _node.TeamT.Score;

                        Discord.SetState($"Score → {_scoreCT}:{_scoreT}");
                        Discord.Update();
                    }
                }
                else if (_map != "main_menu")
                {
                    _map = "main_menu";
                    _scoreCT = 0;
                    _scoreT = 0;

                    Discord.SetDetails("In Main Menu");
                    Discord.SetState(null);
                    Discord.SetTimestamp(DateTime.UtcNow);
                    Discord.SetLargeArtwork("icon");
                    Discord.SetSmallArtwork(null);
                    Discord.Update();
                }

                await Task.Delay(2000);
            }
        }

        private static int GeneratePort()
        {
            int port = new Random().Next(1024, 65536);

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            while (properties.GetActiveTcpConnections().Any(x => x.LocalEndPoint.Port == port))
            {
                port = new Random().Next(1024, 65536);
            }

            return port;
        }

        public static void OnNewGameState(GameState gs) => _node = gs.Map;
    }
}
