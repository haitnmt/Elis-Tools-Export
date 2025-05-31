# Hướng dẫn sử dụng tính năng Import/Export

## Tổng quan
Ứng dụng đã được bổ sung 2 tính năng mới:
- **Import từ File**: Nhập dữ liệu từ file JSON hoặc CSV do người dùng chọn
- **Export ra File**: Xuất dữ liệu ra file JSON với vị trí người dùng chọn

## Tính năng Import

### Cách sử dụng:
1. Nhấn nút **"Import từ File"** trên giao diện chính
2. Chọn file cần import (hỗ trợ định dạng .json và .csv)
3. Ứng dụng sẽ xử lý và hiển thị thông báo kết quả

### Định dạng file hỗ trợ:
- **JSON**: File có đuôi .json với cấu trúc dữ liệu hợp lệ
- **CSV**: File có đuôi .csv với dữ liệu phân tách bằng dấu phẩy

### Lưu ý:
- Các phương thức `ProcessJsonImport()` và `ProcessCsvImport()` hiện tại chỉ là placeholder
- Bạn cần implement logic xử lý dữ liệu cụ thể dựa trên yêu cầu nghiệp vụ

## Tính năng Export

### Cách sử dụng:
1. Đảm bảo đã kết nối thành công với cơ sở dữ liệu
2. Nhấn nút **"Export ra File"** trên giao diện chính
3. Ứng dụng sẽ tạo file export và cho phép bạn chọn vị trí lưu

### Định dạng file xuất:
- **JSON**: File có đuôi .json với cấu trúc dữ liệu chuẩn
- Tên file: `elis_export_data_YYYYMMDD_HHMMSS.json`

### Dữ liệu xuất bao gồm:
- Thời gian export
- Thông tin kết nối (không bao gồm mật khẩu)
- Dữ liệu mẫu (hiện tại là dữ liệu test)

### Lưu ý:
- Phương thức `PrepareExportData()` hiện tại chỉ tạo dữ liệu mẫu
- Bạn cần implement logic truy vấn dữ liệu thực từ cơ sở dữ liệu

## Cấu trúc Code

### Các phương thức chính:

#### Import:
- `ImportDataBtn_Clicked()`: Xử lý sự kiện nhấn nút Import
- `ProcessImportData()`: Xử lý dữ liệu import dựa trên loại file
- `ProcessJsonImport()`: Xử lý file JSON (cần implement)
- `ProcessCsvImport()`: Xử lý file CSV (cần implement)

#### Export:
- `ExportDataBtn_Clicked()`: Xử lý sự kiện nhấn nút Export  
- `PrepareExportData()`: Chuẩn bị dữ liệu để export (cần implement)

## Các package đã thêm:
- `CommunityToolkit.Maui`: Cung cấp các tính năng mở rộng cho MAUI

## Phát triển tiếp:

### Để hoàn thiện tính năng Import:
1. Implement logic parse JSON trong `ProcessJsonImport()`
2. Implement logic parse CSV trong `ProcessCsvImport()`
3. Thêm validation dữ liệu
4. Implement logic lưu vào cơ sở dữ liệu

### Để hoàn thiện tính năng Export:
1. Implement logic truy vấn dữ liệu từ database trong `PrepareExportData()`
2. Thêm các tùy chọn filter/điều kiện export
3. Hỗ trợ thêm định dạng file khác (CSV, Excel)
4. Thêm progress bar cho việc export dữ liệu lớn

## Liên hệ
Nếu có thắc mắc hoặc cần hỗ trợ, vui lòng liên hệ với đội phát triển.
