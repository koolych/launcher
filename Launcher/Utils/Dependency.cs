using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Spectre.Console;
using System.Diagnostics;

namespace Launcher.Utils
{
    public class Dependency // to everyone seeing this: I am sorry, I think I am doing my best copying the rest of the code :innocent:
    {
        [JsonProperty(PropertyName = "name")]
        public required string Name { get; set; }

        [JsonProperty(PropertyName = "download_url")]
        public string? URL { get; set; }

        [JsonProperty(PropertyName = "path")]
        public required string Path { get; set; }
    }

    public class Dependencies(bool success, List<Dependency> localDependencies, List<Dependency> remoteDependencies)
    {
        public bool Success = success;
        public List<Dependency> LocalDependencies = localDependencies;
        public List<Dependency> RemoteDependencies = remoteDependencies;
    }

    public static class DependencyManager
    {
        private static Process? _process;
        public static string directory = Directory.GetCurrentDirectory();

        public async static Task<List<Dependency>> Get()
        {
            List<Dependency> dependencies = new List<Dependency>();

            if (Debug.Enabled())
                Terminal.Debug("Getting list of dependencies.");
            try
            {
                string responseString = await Api.GitHub.GetDependencies();

                JObject responseJson = JObject.Parse(responseString);

                if (responseJson["files"] != null)
                    dependencies = responseJson["files"]!.ToObject<Dependency[]>()!.ToList();
            }
            catch
            {
                if (Debug.Enabled())
                    Terminal.Debug("Couldn't get list of dependencies.");
            }
            return dependencies;
        }

        public async static Task<Boolean> Install(StatusContext ctx, Dependencies dependencies)
        {
            _process = new Process();
            Boolean success = false;
            foreach (var dependency in dependencies.LocalDependencies)
            {
                if (Debug.Enabled())
                    Terminal.Debug($"Executing dependency installer: {dependency.Name}");
                _process.StartInfo.FileName = $"{directory}{dependency.Path}";
                _process.StartInfo.UseShellExecute = true;
                _process.StartInfo.Verb = "runas";
                try
                {
                    _process.Start();
                    await _process.WaitForExitAsync();
                    if (Debug.Enabled())
                        Terminal.Debug($"Dependency installer {dependency.Name} has exited with status code {_process.ExitCode}");
                    success = true;
                }
                catch
                {
                    if (Debug.Enabled())
                        Terminal.Debug($"Couldn't execute setup for dependency: {dependency.Name}");
                    success = false;
                }
            }
            foreach (var dependency in dependencies.RemoteDependencies)
            {
                if (Debug.Enabled())
                    Terminal.Debug($"Executing dependency installer: {dependency.Name}");
                _process.StartInfo.FileName = $"{directory}{dependency.Path}";
                _process.StartInfo.UseShellExecute = true;
                _process.StartInfo.Verb = "runas";
                try
                {
                    _process.Start();
                    await _process.WaitForExitAsync();
                    if (Debug.Enabled())
                        Terminal.Debug($"Dependency installer {dependency.Name} has exited with status code {_process.ExitCode}");
                    success = true;
                }
                catch
                {
                    if (Debug.Enabled())
                        Terminal.Debug($"Couldn't execute setup for dependency: {dependency.Name}");
                    success = false;
                }
            }
            return success;
        }
    }
}
