using System;
using System.CommandLine;
using System.IO;
using SGReader.Core;

namespace SGReader.CLI
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            var fileArgument = new Argument<FileInfo>("file")
            {
                Description = "Path to an SG2 or SG3 file"
            };

            var outputOption = new Option<DirectoryInfo>("--output", "-o")
            {
                Description = "Output directory for extracted PNGs (default: ./<sg-name>/)"
            };

            var listOption = new Option<bool>("--list", "-l")
            {
                Description = "List file info without exporting"
            };

            var rootCommand = new RootCommand("Extract sprites from Impressions Games SG2/SG3 files")
            {
                fileArgument,
                outputOption,
                listOption
            };

            rootCommand.SetAction(parseResult =>
            {
                var file = parseResult.GetValue(fileArgument);
                var output = parseResult.GetValue(outputOption);
                var listOnly = parseResult.GetValue(listOption);
                return Run(file, output, listOnly);
            });

            return rootCommand.Parse(args).Invoke();
        }

        private static int Run(FileInfo file, DirectoryInfo output, bool listOnly)
        {
            if (file == null || !file.Exists)
            {
                Console.Error.WriteLine("File not found.");
                return 1;
            }

            try
            {
                using var sgFile = new SGFile(file.FullName);
                sgFile.Load();

                Console.WriteLine($"File   : {sgFile.Name}");
                Console.WriteLine($"Version: {sgFile.Header.Version}");
                Console.WriteLine($"Images : {sgFile.Images.Count}");
                Console.WriteLine($"Bitmaps: {sgFile.Bitmaps.Count}");
                Console.WriteLine($"Groups : {sgFile.AnimationsGroups.Count}");

                if (listOnly)
                    return 0;

                string outputDir = output?.FullName
                    ?? Path.Combine(Directory.GetCurrentDirectory(), sgFile.Name);

                Console.WriteLine($"Output : {outputDir}");
                int exported = SGPngExporter.ExportFile(sgFile, outputDir);
                Console.WriteLine($"Exported {exported} PNG file(s).");
                return 0;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error: {ex.Message}");
                return 1;
            }
        }
    }
}
