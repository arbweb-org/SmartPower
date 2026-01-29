using Microsoft.AspNetCore.Components;
using SmartPower.Client.Services;
using System.Text;

namespace SmartPower.Client.Components.Pages
{
    public partial class Logs
    {
        private List<LogFile>? _logs;
        private LogFile? _selectedLog;
        private string _points = string.Empty;

        protected override void OnInitialized()
        {
            LoadLogs();
        }

        private void LoadLogs()
        {
            _logs = StorageService.GetLogFiles();
        }

        private void GoBack()
        {
            NavigationManager.NavigateTo("/");
        }

        private void ViewLog(LogFile log)
        {
            _selectedLog = log;
            var data = StorageService.ReadLog(log.Path);
            
            var sb = new StringBuilder();
            float scaleY = 240f / 60000f; // 240 pixels for 60,000mA
            
            // Limit to first 5,000 samples for performance
            int count = Math.Min(data.Count, 5000);
            for (int i = 0; i < count; i++)
            {
                double x = (double)i * 5000 / count; 
                double y = 120 - (data[i] * scaleY);
                sb.Append($"{x:F1},{y:F1} ");
            }
            _points = sb.ToString();
        }

        private async Task DeleteLog(LogFile log)
        {
            bool confirm = await Application.Current.Windows[0].Page.DisplayAlert("Delete", $"Are you sure you want to delete {log.Name}?", "Yes", "No");
            if (confirm)
            {
                StorageService.DeleteLog(log.Path);
                LoadLogs();
                if (_selectedLog?.Path == log.Path)
                {
                    _selectedLog = null;
                    _points = string.Empty;
                }
            }
        }
    }
}
