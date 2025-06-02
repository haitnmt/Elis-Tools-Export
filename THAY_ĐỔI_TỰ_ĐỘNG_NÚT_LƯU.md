# Tự động Disable/Enable Nút "Lưu tệp kết nối"

## Mô tả thay đổi

Đã cập nhật `MainPage.xaml.cs` để tự động disable/enable nút **ShareConnectionBtn** (Lưu tệp kết nối) dựa trên sự thay đổi của thông tin kết nối.

## Chức năng mới

### 1. Theo dõi thay đổi Entry
- Tự động đăng ký sự kiện `TextChanged` cho tất cả các Entry: `EntryServer`, `EntryDatabase`, `EntryUserId`, `EntryPassword`
- Khi bất kỳ Entry nào thay đổi, sẽ gọi method `OnConnectionInfoChanged`

### 2. Quản lý trạng thái nút ShareConnectionBtn
- **Disable nút** khi:
  - Thông tin kết nối hiện tại khác với thông tin đã kiểm tra thành công gần nhất
  - Chưa có thông tin kết nối nào được kiểm tra thành công
  - Kết nối kiểm tra thất bại

- **Enable nút** khi:
  - Thông tin kết nối hiện tại giống với thông tin đã kiểm tra thành công gần nhất
  - Kết nối kiểm tra thành công
  - Mở file kết nối thành công

### 3. Các method mới được thêm

#### `OnConnectionInfoChanged(object? sender, TextChangedEventArgs e)`
- Sự kiện được gọi khi bất kỳ Entry nào thay đổi
- Gọi `UpdateShareButtonState()` để cập nhật trạng thái nút

#### `UpdateShareButtonState()`
- So sánh thông tin hiện tại với thông tin đã lưu
- Enable/Disable nút ShareConnectionBtn tương ứng

#### `IsConnectionInfoEqual(ConnectionInfo info1, ConnectionInfo info2)`
- So sánh hai đối tượng ConnectionInfo
- Kiểm tra Server, Database, Username, Password

#### `CloneConnectionInfo(ConnectionInfo original)`
- Tạo bản sao của ConnectionInfo
- Dùng để lưu trạng thái đã kiểm tra thành công

### 4. Biến mới
- `_lastValidConnectionInfo`: Lưu trữ thông tin kết nối đã kiểm tra thành công gần nhất

## Luồng hoạt động

1. **Khởi tạo**: Đăng ký sự kiện TextChanged cho các Entry
2. **Kiểm tra kết nối thành công**: Lưu thông tin vào `_lastValidConnectionInfo` và enable nút
3. **Entry thay đổi**: Disable nút nếu thông tin khác với `_lastValidConnectionInfo`
4. **Khôi phục Entry**: Enable nút nếu thông tin giống với `_lastValidConnectionInfo`
5. **Mở file kết nối**: Cập nhật `_lastValidConnectionInfo` và enable nút

## Ưu điểm

- **Tự động**: Không cần can thiệp thủ công
- **Trực quan**: Người dùng biết ngay khi nào có thể lưu file kết nối
- **Chính xác**: Chỉ enable khi thông tin thực sự không thay đổi so với lần kiểm tra thành công
- **Hiệu quả**: Tránh lưu file với thông tin chưa được kiểm tra

## Test cases

1. ✅ Khởi động app → Nút disabled
2. ✅ Kiểm tra kết nối thành công → Nút enabled  
3. ✅ Thay đổi Server → Nút disabled
4. ✅ Khôi phục Server về giá trị cũ → Nút enabled
5. ✅ Thay đổi Database → Nút disabled
6. ✅ Thay đổi Username → Nút disabled
7. ✅ Thay đổi Password → Nút disabled
8. ✅ Khôi phục tất cả về giá trị cũ → Nút enabled
9. ✅ Mở file kết nối thành công → Nút enabled
10. ✅ Kiểm tra kết nối thất bại → Nút disabled
