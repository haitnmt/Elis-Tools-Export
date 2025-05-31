# Hướng dẫn Import/Export thông tin kết nối

## Tổng quan
Tính năng Import/Export thông tin kết nối cho phép bạn sao lưu và chia sẻ cấu hình kết nối cơ sở dữ liệu một cách dễ dàng.

## Cách sử dụng

### 🔄 Export thông tin kết nối

1. **Nhập thông tin kết nối** vào các trường:
   - Máy chủ dữ liệu
   - Tên cơ sở dữ liệu  
   - Tên tài khoản truy cập
   - Mật khẩu truy cập

2. **Nhấn nút "Export Kết nối"**
   - Hệ thống sẽ tạo file mã hóa .inf trong thư mục Downloads
   - File được đặt tên theo format: `elis_connection_[Database]_YYYYMMDD_HHMMSS.inf`
   - Sau khi lưu thành công, hệ thống sẽ hỏi:
     - **"Chia sẻ"**: Mở dialog chia sẻ file cho các ứng dụng khác
     - **"Mở vị trí file"**: Mở Windows Explorer/Finder và highlight file

### 📥 Import thông tin kết nối

1. **Nhấn nút "Import Kết nối"**
2. **Chọn file .inf** chứa thông tin kết nối đã export trước đó (hỗ trợ cả file .json)
3. **Hệ thống sẽ tự động**:
   - Đọc thông tin từ file (tự động phát hiện mã hóa)
   - Điền vào các trường trong form
   - Cập nhật giao diện

## Định dạng file

### File .inf (mã hóa - khuyến nghị)
File export mặc định sẽ là định dạng .inf được mã hóa để bảo vệ thông tin nhạy cảm.

### File .json (không mã hóa - fallback)  
Hệ thống vẫn hỗ trợ đọc file JSON với cấu trúc như sau:

```json
{
  "Server": "localhost",
  "Database": "ElisTools", 
  "Username": "sa",
  "Password": "123456",
  "ExportDate": "2025-05-31T10:30:00",
  "ExportedBy": "Elis Tools App"
}
```

## Lưu ý bảo mật

✅ **CẢI THIỆN BẢO MẬT**: File export hiện được mã hóa để bảo vệ thông tin kết nối.

**Khuyến nghị**:
- Sử dụng file .inf (mã hóa) thay vì .json 
- Lưu file export ở vị trí an toàn
- Không chia sẻ file qua mạng không bảo mật
- Xóa file export sau khi sử dụng xong
- Chỉ chia sẻ với người có quyền truy cập
- Xóa file export sau khi sử dụng xong
- Sử dụng mã hóa disk/folder khi cần thiết

## Trường hợp sử dụng

✅ **Phù hợp cho**:
- Sao lưu cấu hình kết nối
- Chia sẻ cấu hình trong team
- Chuyển đổi giữa các môi trường (Dev/Test/Prod)
- Migration giữa các máy khác nhau

✅ **Lợi ích**:
- Tiết kiệm thời gian nhập thông tin
- Tránh lỗi typo khi nhập thủ công
- Dễ dàng quản lý nhiều cấu hình
- Standardize cấu hình trong team

## Xử lý lỗi

### Import không thành công
- Kiểm tra file .inf/.json có đúng format không
- Đảm bảo file không bị corrupted
- Kiểm tra quyền đọc file
- Thử với file .json nếu .inf bị lỗi

### Export không thành công  
- Kiểm tra quyền ghi vào folder đích
- Đảm bảo có đủ dung lượng ổ đĩa
- Kiểm tra thông tin kết nối đã được điền đầy đủ

## Liên hệ hỗ trợ

Nếu gặp vấn đề khi sử dụng tính năng này, vui lòng liên hệ team phát triển.
