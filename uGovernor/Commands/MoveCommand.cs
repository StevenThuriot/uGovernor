using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using uGovernor.Domain;

namespace uGovernor.Commands
{
    class MoveCommand : Command
    {
        private readonly string _sourceFolder;
        private readonly string _file;
        private readonly string _destinationFolder;

        public MoveCommand(string label, string sourceFolder, string destinationFolder, string file, Execution execution)
            : base (null, null, execution)
        {
            _sourceFolder = sourceFolder ?? throw new ArgumentNullException(nameof(sourceFolder));
            _destinationFolder = destinationFolder;

            if (destinationFolder != null && label != null)
            {
                _destinationFolder = Regex.Replace(_destinationFolder, "%label%", label, RegexOptions.IgnoreCase);
                _destinationFolder = Regex.Replace(_destinationFolder, "%year%", DateTime.Now.Year.ToString(), RegexOptions.IgnoreCase);
                _destinationFolder = Regex.Replace(_destinationFolder, "%month%", DateTime.Now.Month.ToString("D2"), RegexOptions.IgnoreCase);
            }

            _file = file;
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
