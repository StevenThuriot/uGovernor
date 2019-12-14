using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using uGovernor.Domain;

namespace uGovernor.Commands
{
    class MoveCommand : Command
    {
        private readonly string _sourceFolder;
        private readonly string _file;
        private readonly string _destinationFolder;

        public MoveCommand(string label, string sourceFolder, string destinationFolder, string file, Execution execution)
            : base(null, null, execution)
        {
            _sourceFolder = sourceFolder ?? throw new ArgumentNullException(nameof(sourceFolder));
            _destinationFolder = destinationFolder;

            if (destinationFolder != null)
            {
                var now = DateTime.Now;

                var season = ResolveSeason(now);

                _destinationFolder = _destinationFolder.Replace("%label%", label, StringComparison.OrdinalIgnoreCase)
                                                       .Replace("%year%", now.Year.ToString(), StringComparison.OrdinalIgnoreCase)
                                                       .Replace("%month%", now.Month.ToString("D2"), StringComparison.OrdinalIgnoreCase)
                                                       .Replace("%season%", season.ToString(), StringComparison.OrdinalIgnoreCase)
                                                       .Replace("%seasonnr%", ((int)season).ToString(), StringComparison.OrdinalIgnoreCase);
            }

            _file = file;
        }

        enum Seasons
        {
            Spring = 1,
            Summer = 2,
            Autumn = 3,
            Winter = 4
        }

        private static Seasons ResolveSeason(DateTime date)
        {
            int doy = date.DayOfYear - Convert.ToInt32((DateTime.IsLeapYear(date.Year)) && date.DayOfYear > 59);

            if (doy < 80 || doy >= 355) return Seasons.Winter;

            if (doy >= 80 && doy < 172) return Seasons.Spring;

            if (doy >= 172 && doy < 266) return Seasons.Summer;

            return Seasons.Autumn;
        }

        public override void Run(Torrent torrent)
        {
            Trace.TraceInformation($"Running action Move...");

            if (_destinationFolder is null)
                return;

            if (!torrent.CanExecute(ExecutionLevel))
                return;

            if (string.IsNullOrEmpty(_file))
            {
                MoveDirectory();
            }
            else
            {
                MoveFile();
            }
        }

        private void MoveFile()
        {
            var source = Path.Combine(_sourceFolder, _file);
            var destination = Path.Combine(_destinationFolder, _file);

            Directory.CreateDirectory(_destinationFolder);
            File.Move(source, destination, true);
        }

        private void MoveDirectory()
        {
            Directory.CreateDirectory(_destinationFolder);

            var sourcePath = _sourceFolder.TrimEnd('\\', ' ');
            var targetPath = Path.Combine(_destinationFolder, Path.GetFileName(sourcePath)).TrimEnd('\\', ' ');

            var files = Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories)
                                 .GroupBy(s => Path.GetDirectoryName(s));

            foreach (var folder in files)
            {
                var targetFolder = folder.Key.Replace(sourcePath, targetPath);

                Directory.CreateDirectory(targetFolder);

                foreach (var file in folder)
                {
                    var targetFile = Path.Combine(targetFolder, Path.GetFileName(file));
                    File.Move(file, targetFile, true);
                }
            }

            Directory.Delete(_sourceFolder, true);
        }
    }
}
