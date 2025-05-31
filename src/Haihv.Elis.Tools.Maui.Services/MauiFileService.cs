using System.Text;
using Haihv.Elis.Tools.Services;
using ILogger = Serilog.ILogger;

namespace Haihv.Elis.Tools.Maui.Services;

public class MauiFileService(ILogger logger) : IFileService
{
    public async Task<string> ReadFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                logger.Debug("Tệp không tồn tại! [{filePath}]", filePath);
                return string.Empty;
            }

            await using var fileStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            using var reader = new StreamReader(
                fileStream,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 4096,
                leaveOpen: false);

            return await reader.ReadToEndAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Warning($"Lỗi khi đọc tệp: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<byte[]?> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (File.Exists(filePath)) return await File.ReadAllBytesAsync(filePath, cancellationToken);
            logger.Debug("Tệp không tồn tại! [{filePath}]", filePath);
            return null;

        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Lỗi khi đọc tệp!");
            return null;
        }
    }

    public byte[]? ReadAllBytes(string filePath)
    {
        try
        {
            if (File.Exists(filePath)) return File.ReadAllBytes(filePath);
            logger.Debug("Tệp không tồn tại! [{filePath}]", filePath);
            return null;

        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Lỗi khi đọc tệp!");
            return null;
        }
    }

    public async Task WriteFileAsync(string filePath, string content, CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await using var fileStream = new FileStream(
                filePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                useAsync: true);

            await using var writer = new StreamWriter(
                fileStream,
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                bufferSize: 4096,
                leaveOpen: false);

            await writer.WriteAsync(content);
            await writer.FlushAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Lỗi khi ghi tệp!");
        }
    }

    public async Task WriteAllBytesAsync(string filePath, byte[] content, CancellationToken cancellationToken = default)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(filePath, content, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Lỗi khi ghi tệp!");
        }
    }

    public void WriteAllBytes(string filePath, byte[] content)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }
            File.WriteAllBytes(filePath, content);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Lỗi khi ghi tệp");
        }
    }

    public async Task<bool> CreateAsync(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                logger.Warning("Tệp đã tồn tại!");
                return false;
            }
            await using var file = File.Create(filePath);
            return true;
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Lỗi khi tạo tệp");
            return false;
        }
    }

    public async Task DeleteAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!await ExistsAsync(filePath))
            {
                logger.Information("Tệp không tồn tại! [{filePath}]", filePath);
            }
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Lỗi khi xóa tệp");
        }
    }

    public void Delete(string filePath)
    {
        try
        {
            if (!Exists(filePath))
            {
                logger.Debug("Tệp không tồn tại! [{filePath}]", filePath);
            }
            File.Delete(filePath);
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Lỗi khi xóa tệp");
        }
    }

    public Task DeleteDirectoryAsync(string directoryPath, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                logger.Information("Thư mục không tồn tại! [{directoryPath}]", directoryPath);
            }
            Directory.Delete(directoryPath, true);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Lỗi khi xóa thư mục");
            return Task.CompletedTask;
        }
    }

    public bool DeleteDirectory(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                logger.Debug("Thư mục không tồn tại! [{directoryPath}]", directoryPath);
            }
            Directory.Delete(directoryPath, true);
            return true;
        }
        catch (Exception ex)
        {
            logger.Warning(ex, "Lỗi khi xóa thư mục");
            return false;
        }
    }

    public Task<bool> ExistsAsync(string filePath)
    {
        return Task.FromResult(File.Exists(filePath));
    }

    public bool Exists(string filePath)
    {
        return File.Exists(filePath);
    }

    public async Task<string?> PickFileAsync(string[]? allowedFileTypes = null, string title = "Chọn file")
    {
        try
        {
            var fileTypes = allowedFileTypes?.Length > 0
                ? new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.iOS, allowedFileTypes },
                    { DevicePlatform.Android, allowedFileTypes },
                    { DevicePlatform.WinUI, allowedFileTypes },
                    { DevicePlatform.macOS, allowedFileTypes }
                })
                : null;

            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = title,
                FileTypes = fileTypes
            });

            if (result != null)
            {
                logger.Information("File picked: {FileName}", result.FileName);
                return await ReadFileAsync(result.FullPath);
            }

            return null;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Lỗi khi chọn file");
            return null;
        }
    }    public async Task<bool> SaveFileAsAsync(string content, string suggestedFileName = "ConnectionInfo.inf", string title = "Lưu file cấu hình")
    {
        try
        {
            // Lưu trực tiếp vào Downloads
            var downloadsPath = GetDefaultDownloadsPath();
            if (string.IsNullOrEmpty(downloadsPath))
            {
                logger.Error("Could not determine Downloads folder");
                return false;
            }

            // Đảm bảo tên file duy nhất
            var filePath = Path.Combine(downloadsPath, suggestedFileName);
            var counter = 1;
            var nameWithoutExt = Path.GetFileNameWithoutExtension(suggestedFileName);
            var extension = Path.GetExtension(suggestedFileName);

            while (File.Exists(filePath))
            {
                var newFileName = $"{nameWithoutExt}_{counter}{extension}";
                filePath = Path.Combine(downloadsPath, newFileName);
                counter++;
            }

            // Ghi file
            await File.WriteAllTextAsync(filePath, content, Encoding.UTF8);
            logger.Information("File saved successfully: {FilePath}", filePath);

            // Hiển thị thông báo thành công với option để mở thư mục
            var mainPage = Application.Current?.Windows.FirstOrDefault()?.Page;
            if (mainPage == null) return true;
            var openFolder = await mainPage.DisplayAlert(
                "Thành công", 
                $"File đã được lưu tại:\n{filePath}\n\nBạn có muốn mở thư mục Downloads?", 
                "Mở thư mục", 
                "OK");

            if (!openFolder) return true;
            try
            {
                // Mở thư mục Downloads (chỉ hỗ trợ Windows và macOS)
                if (DeviceInfo.Platform == DevicePlatform.WinUI)
                {
                    System.Diagnostics.Process.Start("explorer.exe", downloadsPath);
                }
                else if (DeviceInfo.Platform == DevicePlatform.macOS)
                {
                    System.Diagnostics.Process.Start("open", downloadsPath);
                }
                else
                {
                    await mainPage.DisplayAlert("Thông báo", "Tính năng mở thư mục chưa được hỗ trợ trên nền tảng này", "OK");
                }
            }
            catch (Exception ex)
            {
                logger.Warning(ex, "Could not open folder");
                await mainPage.DisplayAlert("Lỗi", "Không thể mở thư mục file", "OK");
            }

            return true;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Lỗi khi lưu file");
            return false;
        }
    }    private string GetDefaultDownloadsPath()
    {
        try
        {
            string downloadsPath;

            if (DeviceInfo.Platform == DevicePlatform.WinUI)
            {
                downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
            else if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                downloadsPath = "/storage/emulated/0/Download";
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
            }
            else if (DeviceInfo.Platform == DevicePlatform.macOS)
            {
                downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            }
            else
            {
                downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Downloads");
            }

            if (!Directory.Exists(downloadsPath))
            {
                Directory.CreateDirectory(downloadsPath);
            }

            logger.Information("Using Downloads folder: {Path}", downloadsPath);
            return downloadsPath;
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error getting Downloads folder, using Documents instead");
            var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (!Directory.Exists(documentsPath))
            {
                Directory.CreateDirectory(documentsPath);
            }

            return documentsPath;
        }
    }
}