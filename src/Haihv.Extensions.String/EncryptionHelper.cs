using System.Security.Cryptography;
using System.Text;

namespace Haihv.Extensions.String;

/// <summary>
/// Lớp chứa các phương thức mở rộng để mã hóa và giải mã chuỗi văn bản với bảo mật cao.
/// Sử dụng AES-256-CBC với IV ngẫu nhiên và PBKDF2 để tạo khóa từ mật khẩu.
/// </summary>
public static class EncryptionHelper
{
    // Salt cố định cho ứng dụng - chỉ dùng cho backward compatibility
    private static readonly byte[] AppSalt = Convert.FromBase64String("RWxpc1Rvb2xzQXBwU2FsdDIwMjU=");

    // Số lần lặp cho PBKDF2 - cao để chống brute force
    private const int Pbkdf2Iterations = 100000;

    // Mật khẩu mặc định cho ứng dụng (chỉ dùng cho backward compatibility)
    private const string DefaultAppPassword = "H9A&^3XRq8jLnxnw$ancpV!8cs4U4Zr^M^%^";

    // Kích thước khóa AES và IV
    private const int AesKeySize = 256; // bits
    private const int AesBlockSize = 128; // bits (16 bytes)

    /// <summary>
    /// Mã hóa chuỗi văn bản với mật khẩu tùy chỉnh và salt ngẫu nhiên.
    /// </summary>
    /// <param name="plaintext">Văn bản cần mã hóa</param>
    /// <param name="password">Mật khẩu mã hóa</param>
    /// <returns>Chuỗi base64 chứa salt + IV + dữ liệu mã hóa</returns>
    public static string EncryptWithPassword(string plaintext, string password)
    {
        if (string.IsNullOrEmpty(plaintext))
            return string.Empty;

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Mật khẩu không được để trống", nameof(password));

        // Tạo salt ngẫu nhiên cho mỗi lần mã hóa
        byte[] salt = new byte[32]; // 256 bits
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }

        // Tạo khóa từ mật khẩu và salt bằng PBKDF2
        byte[] key;
        using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
        {
            key = pbkdf2.GetBytes(32); // 256 bits
        }

        // Mã hóa dữ liệu
        byte[] encrypted = EncryptBytes(Encoding.UTF8.GetBytes(plaintext), key);

        // Kết hợp salt + encrypted data (đã bao gồm IV)
        byte[] result = new byte[salt.Length + encrypted.Length];
        Array.Copy(salt, 0, result, 0, salt.Length);
        Array.Copy(encrypted, 0, result, salt.Length, encrypted.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Giải mã chuỗi văn bản đã mã hóa với mật khẩu.
    /// </summary>
    /// <param name="encryptedText">Chuỗi base64 đã mã hóa</param>
    /// <param name="password">Mật khẩu giải mã</param>
    /// <returns>Văn bản gốc</returns>
    public static string DecryptWithPassword(string encryptedText, string password)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Mật khẩu không được để trống", nameof(password));

        try
        {
            byte[] data = Convert.FromBase64String(encryptedText);

            // Tách salt và dữ liệu mã hóa
            if (data.Length < 32 + 16 + 1) // salt + IV + ít nhất 1 byte dữ liệu
                throw new ArgumentException("Dữ liệu mã hóa không hợp lệ");

            byte[] salt = new byte[32];
            Array.Copy(data, 0, salt, 0, 32);

            byte[] encryptedData = new byte[data.Length - 32];
            Array.Copy(data, 32, encryptedData, 0, encryptedData.Length);

            // Tạo lại khóa từ mật khẩu và salt
            byte[] key;
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                key = pbkdf2.GetBytes(32); // 256 bits
            }

            // Giải mã dữ liệu
            byte[] decrypted = DecryptBytes(encryptedData, key);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Không thể giải mã dữ liệu. Mật khẩu có thể sai hoặc dữ liệu bị hỏng.", ex);
        }
    }

    /// <summary>
    /// Mã hóa chuỗi văn bản bằng mật khẩu mặc định của ứng dụng (để tương thích ngược).
    /// </summary>
    /// <param name="plaintext">Văn bản cần mã hóa</param>
    /// <returns>Chuỗi base64 đã mã hóa</returns>
    public static string Encrypt(this string plaintext)
    {
        if (string.IsNullOrEmpty(plaintext))
            return string.Empty;

        // Sử dụng salt cố định cho backward compatibility
        byte[] key;
        using (var pbkdf2 = new Rfc2898DeriveBytes(DefaultAppPassword, AppSalt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
        {
            key = pbkdf2.GetBytes(32); // 256 bits
        }

        byte[] encrypted = EncryptBytes(Encoding.UTF8.GetBytes(plaintext), key);
        return Convert.ToBase64String(encrypted);
    }

    /// <summary>
    /// Giải mã chuỗi văn bản bằng mật khẩu mặc định của ứng dụng (để tương thích ngược).
    /// </summary>
    /// <param name="encryptedText">Chuỗi base64 đã mã hóa</param>
    /// <returns>Văn bản gốc</returns>
    public static string Decrypt(this string encryptedText)
    {
        if (string.IsNullOrEmpty(encryptedText))
            return string.Empty;

        try
        {
            // Sử dụng salt cố định cho backward compatibility
            byte[] key;
            using (var pbkdf2 = new Rfc2898DeriveBytes(DefaultAppPassword, AppSalt, Pbkdf2Iterations, HashAlgorithmName.SHA256))
            {
                key = pbkdf2.GetBytes(32); // 256 bits
            }

            byte[] encryptedData = Convert.FromBase64String(encryptedText);
            byte[] decrypted = DecryptBytes(encryptedData, key);
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Không thể giải mã dữ liệu", ex);
        }
    }

    /// <summary>
    /// Mã hóa mảng byte bằng AES-256-CBC với IV ngẫu nhiên.
    /// </summary>
    /// <param name="data">Dữ liệu cần mã hóa</param>
    /// <param name="key">Khóa mã hóa 256-bit</param>
    /// <returns>IV + dữ liệu mã hóa</returns>
    private static byte[] EncryptBytes(byte[] data, byte[] key)
    {
        using var aes = Aes.Create();
        aes.KeySize = AesKeySize;
        aes.BlockSize = AesBlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;

        // Tạo IV ngẫu nhiên
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();
        // Ghi IV vào đầu stream
        ms.Write(aes.IV, 0, aes.IV.Length);

        // Ghi dữ liệu mã hóa
        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
        {
            cs.Write(data, 0, data.Length);
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Giải mã mảng byte bằng AES-256-CBC.
    /// </summary>
    /// <param name="encryptedData">IV + dữ liệu mã hóa</param>
    /// <param name="key">Khóa giải mã 256-bit</param>
    /// <returns>Dữ liệu gốc</returns>
    private static byte[] DecryptBytes(byte[] encryptedData, byte[] key)
    {
        using var aes = Aes.Create();
        aes.KeySize = AesKeySize;
        aes.BlockSize = AesBlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;
        aes.Key = key;

        // Tách IV từ đầu dữ liệu
        byte[] iv = new byte[16]; // AES block size
        Array.Copy(encryptedData, 0, iv, 0, iv.Length);
        aes.IV = iv;

        // Lấy phần dữ liệu mã hóa thực sự
        byte[] ciphertext = new byte[encryptedData.Length - iv.Length];
        Array.Copy(encryptedData, iv.Length, ciphertext, 0, ciphertext.Length);

        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(ciphertext);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        using var result = new MemoryStream();
        cs.CopyTo(result);
        return result.ToArray();
    }

    /// <summary>
    /// Tạo mật khẩu ngẫu nhiên mạnh.
    /// </summary>
    /// <param name="length">Độ dài mật khẩu (tối thiểu 12 ký tự)</param>
    /// <returns>Mật khẩu ngẫu nhiên</returns>
    public static string GenerateSecurePassword(int length = 22)
    {
        if (length < 12)
            throw new ArgumentException("Độ dài mật khẩu phải ít nhất 12 ký tự", nameof(length));

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*()_+-=[]{}|;:,.<>?";

        using var rng = RandomNumberGenerator.Create();
        byte[] randomBytes = new byte[length];
        rng.GetBytes(randomBytes);

        var password = new StringBuilder(length);
        for (int i = 0; i < length; i++)
        {
            password.Append(chars[randomBytes[i] % chars.Length]);
        }

        return password.ToString();
    }

    /// <summary>
    /// Kiểm tra độ mạnh của mật khẩu.
    /// </summary>
    /// <param name="password">Mật khẩu cần kiểm tra</param>
    /// <returns>Điểm số từ 0-100 (100 là mạnh nhất)</returns>
    public static int CheckPasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password))
            return 0;

        int score = 0;

        // Độ dài
        if (password.Length >= 8) score += 20;
        if (password.Length >= 12) score += 10;
        if (password.Length >= 16) score += 10;

        // Chữ thường
        if (password.Any(char.IsLower)) score += 15;

        // Chữ hoa
        if (password.Any(char.IsUpper)) score += 15;

        // Số
        if (password.Any(char.IsDigit)) score += 15;

        // Ký tự đặc biệt
        if (password.Any(c => !char.IsLetterOrDigit(c))) score += 15;

        return Math.Min(score, 100);
    }
}