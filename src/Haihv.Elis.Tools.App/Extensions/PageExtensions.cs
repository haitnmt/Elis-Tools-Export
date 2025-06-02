using Haihv.Elis.Tools.App.Controls;
using Haihv.Elis.Tools.Data.Models;

namespace Haihv.Elis.Tools.App.Extensions;

public static class PageExtensions
{    /// <summary>
     /// Hiển thị dialog nhập mật khẩu với ký tự ẩn (****)
     /// </summary>
     /// <param name="page">Page hiện tại</param>
     /// <param name="title">Tiêu đề dialog</param>
     /// <param name="message">Thông điệp hiển thị</param>
     /// <returns>Mật khẩu đã nhập hoặc null nếu hủy</returns>
    public static async Task<string?> DisplayPasswordPromptAsync(this Page page, string title, string message)
    {
        var passwordPrompt = new OpenConnectionPage(title, message);
        await page.Navigation.PushModalAsync(passwordPrompt);
        return await passwordPrompt.GetPasswordAsync();
    }

    /// <summary>
    /// Hiển thị dialog nhập mật khẩu với kiểm tra thời gian hết hạn
    /// </summary>
    /// <param name="page">Page hiện tại</param>
    /// <param name="title">Tiêu đề dialog</param>
    /// <param name="message">Thông điệp hiển thị</param>
    /// <param name="connectionInfo">Thông tin kết nối để kiểm tra hết hạn</param>
    /// <returns>Mật khẩu đã nhập hoặc null nếu hủy</returns>
    public static async Task<string?> DisplayPasswordPromptAsync(this Page page, string title, string message, ConnectionInfo? connectionInfo)
    {
        var passwordPrompt = new OpenConnectionPage(title, message, connectionInfo);
        await page.Navigation.PushModalAsync(passwordPrompt);
        return await passwordPrompt.GetPasswordAsync();
    }
}
