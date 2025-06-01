using Haihv.Elis.Tools.App.Controls;

namespace Haihv.Elis.Tools.App.Extensions;

public static class PageExtensions
{
    /// <summary>
    /// Hiển thị dialog nhập mật khẩu với ký tự ẩn (****)
    /// </summary>
    /// <param name="page">Page hiện tại</param>
    /// <param name="title">Tiêu đề dialog</param>
    /// <param name="message">Thông điệp hiển thị</param>
    /// <returns>Mật khẩu đã nhập hoặc null nếu hủy</returns>
    public static async Task<string?> DisplayPasswordPromptAsync(this Page page, string title, string message)
    {
        var passwordPrompt = new PasswordPromptPage(title, message);
        await page.Navigation.PushModalAsync(passwordPrompt);
        return await passwordPrompt.GetPasswordAsync();
    }
}
