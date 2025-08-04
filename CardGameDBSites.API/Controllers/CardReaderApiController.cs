using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SkytearHorde.Business.Config;
using SkytearHorde.Entities.Models.ViewModels;
using System.Diagnostics;
using System.Text.Json;

namespace CardGameDBSites.API.Controllers
{
    [Route("api/cardreader")]
    [ApiController]
    public class CardReaderApiController : Controller
    {
        private readonly string _uploadDir = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "uploads");
        private readonly string _outputPath = Path.Combine(Directory.GetCurrentDirectory(), "output_results", "results.json");
        private readonly string _configPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location),  "config.json");
        private readonly string _pythonScript = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location), "CardReader.py");

        private readonly CardGameSettingsConfig _config;

        public CardReaderApiController(IOptions<CardGameSettingsConfig> options)
        {
            _config = options.Value;
        }

        [HttpPost]
        [Route("read")]
        public async Task<IActionResult> ReadImages([FromForm] List<IFormFile> files, string apiKey)
        {
            if (_config.CardReaderApiKey != apiKey)
            {
                return NotFound();
            }

            if (!Directory.Exists(_uploadDir))
                Directory.CreateDirectory(_uploadDir);

            List<string> imagePaths = [];

            var i = 0;
            foreach (var file in files)
            {
                var filePath = Path.Combine(_uploadDir, file.FileName);
                using var stream = System.IO.File.Create(filePath);
                await file.CopyToAsync(stream);
                imagePaths.Add(filePath);
                i++;
            }

            // Construct arguments
            string arguments = $"\"{_pythonScript}\" \"{_configPath}\" {string.Join(" ", imagePaths.Select(p => $"\"{p}\""))}";

            // Run Python script
            var psi = new ProcessStartInfo
            {
                FileName = "py", // or full path to python.exe
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);
            string stdOut = await process.StandardOutput.ReadToEndAsync();
            string stdErr = await process.StandardError.ReadToEndAsync();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return StatusCode(500, $"Python error:\n{stdErr}");

            if (!System.IO.File.Exists(_outputPath))
                return StatusCode(500, "No results.json file was created by the script.");

            var json = await System.IO.File.ReadAllTextAsync(_outputPath);
            var parsed = JsonSerializer.Deserialize<CardReaderOuputItem[]>(json);

            foreach (var file in imagePaths)
            {
                System.IO.File.Delete(file);
            }
            Directory.Delete(_uploadDir);

            return Ok(parsed);
        }
    }
}
