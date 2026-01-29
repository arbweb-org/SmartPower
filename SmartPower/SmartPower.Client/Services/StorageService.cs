using SmartPower.Client.Models;
using System.Runtime.InteropServices;

namespace SmartPower.Client.Services
{
    public class StorageService
    {
        private const int MAX_READINGS_PER_FILE = 100000;
        private const int MAX_RMS_PER_FILE = 5000;

        private string _basePath;
        private string _readingsPath;
        private string _rmsPath;

        private FileStream? _readingsStream;
        private FileStream? _rmsStream;

        private int _readingCount;
        private int _rmsCount;

        private string _currentTimestamp;

        public StorageService()
        {
            _basePath = Path.Combine(FileSystem.AppDataDirectory, "logs");
            _readingsPath = Path.Combine(_basePath, "readings");
            _rmsPath = Path.Combine(_basePath, "rms");

            if (!Directory.Exists(_readingsPath)) Directory.CreateDirectory(_readingsPath);
            if (!Directory.Exists(_rmsPath)) Directory.CreateDirectory(_rmsPath);
            
            _currentTimestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }

        public void AppendReadings(double[] readings)
        {
            try
            {
                if (readings.Length == 0) return;

                if (_readingsStream == null || _readingCount >= MAX_READINGS_PER_FILE)
                {
                    RotateReadingsFile();
                }

                // Convert processed doubles (mA) to ints for storage
                byte[] buffer = new byte[readings.Length * 4];
                for (int i = 0; i < readings.Length; i++)
                {
                    int val = (int)readings[i];
                    BitConverter.TryWriteBytes(new Span<byte>(buffer, i * 4, 4), val);
                }

                _readingsStream?.Write(buffer, 0, buffer.Length);
                _readingsStream?.Flush();
                
                _readingCount += readings.Length;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Storage Error (Readings): {ex.Message}");
            }
        }

        public void AppendRms(int rmsValue)
        {
            try
            {
                if (_rmsStream == null || _rmsCount >= MAX_RMS_PER_FILE)
                {
                    RotateRmsFile();
                }

                byte[] buffer = BitConverter.GetBytes(rmsValue);
                _rmsStream?.Write(buffer, 0, buffer.Length);
                _rmsStream?.Flush();

                _rmsCount++;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Storage Error (RMS): {ex.Message}");
            }
        }

        private void RotateReadingsFile()
        {
            _readingsStream?.Dispose();
            _readingCount = 0;
            string fileName = $"readings_{_currentTimestamp}_{DateTime.Now.Ticks}.bin";
            string path = Path.Combine(_readingsPath, fileName);
            _readingsStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        private void RotateRmsFile()
        {
            _rmsStream?.Dispose();
            _rmsCount = 0;
            string fileName = $"rms_{_currentTimestamp}_{DateTime.Now.Ticks}.bin";
            string path = Path.Combine(_rmsPath, fileName);
            _rmsStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Read);
        }

        public void Close()
        {
            if (_readingsStream != null)
            {
                _readingsStream.Dispose();
                _readingsStream = null;
            }
            if (_rmsStream != null)
            {
                _rmsStream.Dispose();
                _rmsStream = null;
            }
        }

        public List<LogFile> GetLogFiles()
        {
            var files = new List<LogFile>();

            if (Directory.Exists(_readingsPath))
            {
                foreach (var file in Directory.GetFiles(_readingsPath, "*.bin"))
                {
                    var info = new FileInfo(file);
                    files.Add(new LogFile { Name = info.Name, Path = file, Size = info.Length, Created = info.CreationTime, IsReadings = true });
                }
            }

            if (Directory.Exists(_rmsPath))
            {
                foreach (var file in Directory.GetFiles(_rmsPath, "*.bin"))
                {
                    var info = new FileInfo(file);
                    files.Add(new LogFile { Name = info.Name, Path = file, Size = info.Length, Created = info.CreationTime, IsReadings = false });
                }
            }

            return files.OrderByDescending(f => f.Created).ToList();
        }

        public List<int> ReadLog(string path)
        {
            var result = new List<int>();
            if (!File.Exists(path)) return result;

            try
            {
                using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] buffer = new byte[4];
                while (stream.Read(buffer, 0, 4) == 4)
                {
                    result.Add(BitConverter.ToInt32(buffer, 0));
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Read Log Error: {ex.Message}");
            }
            return result;
        }

        public void DeleteLog(string path)
        {
            if (File.Exists(path)) File.Delete(path);
        }
    }

    public class LogFile
    {
        public string Name { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public long Size { get; set; }
        public DateTime Created { get; set; }
        public bool IsReadings { get; set; }
    }
}