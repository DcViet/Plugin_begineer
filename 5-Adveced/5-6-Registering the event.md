**5.6. Registering the Event (Đăng ký sự kiện)**  

Sau khi tạo phương thức xử lý sự kiện `ElementChangedEvent` (phần 5.5), chúng ta cần đăng ký sự kiện `DocumentChanged` để nó kích hoạt khi tài liệu thay đổi qua một giao dịch (transaction). Phần này hướng dẫn cách đăng ký sự kiện trong `OnStartup` và hủy đăng ký trong `OnShutdown` trong lớp `IExternalDBApplication`.

**Tra cứu sự kiện**:  
- Sự kiện `DocumentChanged` thuộc `ControlledApplication` (namespace `Autodesk.Revit.DB`).  
- Đăng ký: Sử dụng `+=` để thêm delegate (tham chiếu đến phương thức xử lý).  
- Hủy đăng ký: Sử dụng `-=` để xóa delegate.  
- Delegate: `EventHandler<DocumentChangedEventArgs>` đảm bảo phương thức xử lý có đúng chữ ký (signature) với tham số `object sender` và `DocumentChangedEventArgs args`.  
- Kết quả trả về: Sử dụng `ExternalDBApplicationResult` thay vì `Result` (dành cho `IExternalApplication`).  

**Chỉnh sửa lớp ExternalDBApp**:  
1. **Đăng ký sự kiện trong OnStartup**:  
   - Bọc mã trong khối `try-catch` để xử lý ngoại lệ.  
   - Đăng ký sự kiện `DocumentChanged` bằng cách thêm delegate:  
     ```csharp
     application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(ElementChangedEvent);
     ```  
   - Trả về `ExternalDBApplicationResult.Succeeded` trong `try` và `ExternalDBApplicationResult.Failed` trong `catch`.  

2. **Hủy đăng ký trong OnShutdown**:  
   - Hủy đăng ký sự kiện để tránh thực thi không mong muốn:  
     ```csharp
     application.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(ElementChangedEvent);
     ```  
   - Trả về `ExternalDBApplicationResult.Succeeded`.  

**Mã nguồn mẫu**:  
```csharp
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.UI;
using System.Linq;

namespace MyRevitApps
{
    public class ExternalDBApp : IExternalDBApplication
    {
        public ExternalDBApplicationResult OnStartup(ControlledApplication application)
        {
            try
            {
                // Đăng ký sự kiện DocumentChanged
                application.DocumentChanged += new EventHandler<DocumentChangedEventArgs>(ElementChangedEvent);
                return ExternalDBApplicationResult.Succeeded;
            }
            catch
            {
                return ExternalDBApplicationResult.Failed;
            }
        }

        public ExternalDBApplicationResult OnShutdown(ControlledApplication application)
        {
            // Hủy đăng ký sự kiện
            application.DocumentChanged -= new EventHandler<DocumentChangedEventArgs>(ElementChangedEvent);
            return ExternalDBApplicationResult.Succeeded;
        }

        public void ElementChangedEvent(object sender, DocumentChangedEventArgs args)
        {
            // Tạo bộ lọc cho nội thất
            ElementCategoryFilter filter = new(BuiltInCategory.OST_Furniture);

            // Lấy ID phần tử thay đổi đầu tiên
            ElementId elementId = args.GetModifiedElementIds(filter).FirstOrDefault();
            if (elementId != null)
            {
                // Lấy tên giao dịch
                string name = args.GetTransactionNames().FirstOrDefault() ?? "Unknown";

                // Hiển thị thông báo
                TaskDialog.Show("Modified Element",
                    $"Element ID {elementId} changed by transaction: {name}");
            }
        }
    }
}
```

**Cập nhật tệp manifest (.addin)**:  
- Bỏ comment mục `<AddIn Type="DBApplication">` trong tệp `.addin` (đã thêm từ phần 5.2):  
```xml
<?xml version="1.0" encoding="utf-8"?>
<RevitAddIns>
  <AddIn Type="DBApplication">
    <Name>MyExternalDBApp</Name>
    <Assembly>path\to\MyRevitApps.dll</Assembly>
    <AddInId>YOUR_GUID_HERE</AddInId>
    <FullClassName>MyRevitApps.ExternalDBApp</FullClassName>
    <VendorId>YOUR_VENDOR_ID</VendorId>
  </AddIn>
</RevitAddIns>
```
- Đảm bảo `FullClassName` là `MyRevitApps.ExternalDBApp`.  
- Thay `YOUR_GUID_HERE` bằng GUID mới (**Tools > Create GUID**).  
- Cập nhật đường dẫn `.dll` và `VendorId` nếu cần.  

**Kiểm tra**:  
1. Biên dịch dự án trong Visual Studio.  
2. Sao chép `.dll` và `.addin` vào thư mục add-in của Revit (ví dụ: `%appdata%\Autodesk\Revit\Addins\202X`).  
3. Khởi động Revit, mở tệp dự án từ thư mục bài tập.  
4. Thay đổi một phần tử nội thất (furniture), ví dụ: sử dụng lệnh **Move**.  
5. Kiểm tra:  
   - `TaskDialog` hiển thị, ví dụ: “Element ID 217813 changed by transaction: Move”.  
6. **Lưu ý**:  
   - Nếu không có nội thất thay đổi, `TaskDialog` không xuất hiện (`FirstOrDefault` trả về `null`).  
   - Đảm bảo hủy đăng ký sự kiện trong `OnShutdown` để tránh rò rỉ bộ nhớ.  

**Mục tiêu**:  
- Đăng ký sự kiện `DocumentChanged` trong `OnStartup` và hủy trong `OnShutdown`.  
- Sử dụng delegate `EventHandler<DocumentChangedEventArgs>` để liên kết với phương thức xử lý.  
- Hiểu cách trả về `ExternalDBApplicationResult` cho `IExternalDBApplication`.  

**Ứng dụng**:  
- Theo dõi và xử lý các thay đổi trong tài liệu để tự động hóa quy trình làm việc.  
- Mở rộng để xử lý nhiều sự kiện khác (`DocumentOpened`, `DocumentSaved`) hoặc nhiều danh mục phần tử (`OST_Walls`, `OST_Doors`).  
- Ghi log sự kiện vào tệp thay vì dùng `TaskDialog` để không làm gián đoạn người dùng.  

Phần này hoàn thành chuỗi hướng dẫn cơ bản về Revit API, cung cấp nền tảng để xây dựng các add-in phức tạp hơn trong tương lai.