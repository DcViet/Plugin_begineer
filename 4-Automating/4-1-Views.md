**4.1. Views (Tạo ViewPlan)**  

Revit API cung cấp nhiều cách để tạo, chỉnh sửa và hiển thị phần tử, bao gồm việc tạo các khung nhìn (views), gắn thẻ (tagging) và tạo tờ (sheets). Các lớp khung nhìn chính kế thừa từ lớp `View` bao gồm: `View3D`, `ViewDrafting`, `ViewPlan`, `ViewSection`, và `ViewSheet`. Mỗi khung nhìn có thuộc tính `ViewType` (ví dụ: `FloorPlan`, `Elevation`) và `ViewFamilyType` (loại khung hình dùng để tạo khung nhìn). Trong phần này, chúng ta sẽ tạo một lệnh để tự động tạo một **Floor Plan** dựa trên tầng `Ground Floor`.

**Tra cứu ViewPlan**:  
- Trong **Object Browser**, tìm lớp `ViewPlan` trong namespace `Autodesk.Revit.DB`.  
- Phương thức tĩnh: `ViewPlan.Create(Document, ElementId viewFamilyTypeId, ElementId levelId)` tạo một `ViewPlan` với:  
  - `Document`: Tài liệu Revit.  
  - `viewFamilyTypeId`: ID của `ViewFamilyType` (loại khung nhìn, ví dụ `FloorPlan`).  
  - `levelId`: ID của tầng (Level).  

**Tạo lệnh PlanView**:  
1. **Tạo lớp lệnh**:  
   - Tạo lớp `PlanView.cs`, triển khai `IExternalCommand`.  
   - Đặt thuộc tính `[Transaction(TransactionMode.Manual)]` để cho phép thay đổi mô hình nội dung.  
   - Thêm namespace: `Autodesk.Revit.DB`, `Autodesk.Revit.UI`, `Autodesk.Revit.Attributes`, `System.Linq`.  

2. **Lấy ViewFamilyType**:  
   - Tạo `FilteredElementCollector` để tìm `ViewFamilyType`:  
     - `FilteredElementCollector collector = new FilteredElementCollector(doc).OfClass(typeof(ViewFamilyType));`.  
   - Lọc loại `ViewFamily` là `FloorPlan` bằng LINQ:  
     - `ViewFamilyType viewFamily = collector.Cast<ViewFamilyType>().First(x => x.ViewFamily == ViewFamily.FloorPlan);`.  

3. **Lấy Level**:  
   - Lấy tầng `Ground Floor` bằng `FilteredElementCollector`:  
     - `Level level = new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().First(x => x.Name == "Ground Floor");`.  

4. **Tạo ViewPlan**:  
   - Trong khối `using (Transaction trans = new Transaction(doc, "Create ViewPlan"))`:  
     - Gọi `trans.Start()`.  
     - Tạo `ViewPlan`:  
       - `ViewPlan vPlan = ViewPlan.Create(doc, viewFamily.Id, level.Id);`.  
     - Đặt tên khung nhìn: `vPlan.Name = "Our first plan!";`.  
     - Gọi `trans.Commit()`.  

**Mã nguồn mẫu**:  
```csharp
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using System.Linq;

namespace MyRevitCommands
{
    [Transaction(TransactionMode.Manual)]
    public class PlanView : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;
            try
            {
                // Lấy ViewFamilyType cho FloorPlan
                ViewFamilyType viewFamily = new FilteredElementCollector(doc)
                    .OfClass(typeof(ViewFamilyType))
                    .Cast<ViewFamilyType>()
                    .First(x => x.ViewFamily == ViewFamily.FloorPlan);

                // Lấy Level Ground Floor
                Level level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .First(x => x.Name == "Ground Floor");

                // Tạo ViewPlan
                using (Transaction trans = new Transaction(doc, "Create ViewPlan"))
                {
                    trans.Start();
                    ViewPlan vPlan = ViewPlan.Create(doc, viewFamily.Id, level.Id);
                    vPlan.Name = "Our first plan!";
                    trans.Commit();
                }
            }
            catch (Exception e)
            {
                message = e.Message;
                return Result.Failed;
            }
            return Result.Succeeded;
        }
    }
}
```

**Cập nhật tệp manifest (.addin)**:  
- Mở `MyRevitCommands.addin`, sao chép khối `<AddIn>` từ lệnh trước.  
- Cập nhật:  
  - `<AddInId>`: Tạo GUID mới (**Tools > Create GUID**).  
  - `<FullClassName>`: `MyRevitCommands.PlanView`.  
  - `<Name>` và `<Text>`: `PlanView`.  
  - `<Description>`: “Creates a floor plan view for the Ground Floor”.  

**Kiểm tra lệnh**:  
1. Nhấn **Start** trong Visual Studio để chạy Debug.  
2. Mở Revit, mở tệp dự án từ thư mục bài tập.  
3. Vào **Add-Ins > External Tools > PlanView`, chạy lệnh.  
4. Kiểm tra trong **Project Browser** xem khung nhìn mới “Our first plan!” có được tạo dựa trên tầng `Ground Floor` không.  
5. **Lưu ý**: Nếu tầng `Ground Floor` hoặc `ViewFamilyType` cho `FloorPlan` không tồn tại, lệnh sẽ gây lỗi. Có thể bọc trong `try-catch` để xử lý.

**Mục tiêu**  
- Hiểu các loại khung nhìn trong Revit API (`ViewPlan`, `View3D`, v.v.) và cách phân biệt chúng qua `ViewType` và `ViewFamilyType`.  
- Tạo một `ViewPlan` tự động bằng `ViewPlan.Create`.  
- Lọc `ViewFamilyType` và `Level` bằng `FilteredElementCollector` và LINQ.  

**Ứng dụng**: Tự động tạo hàng loạt khung nhìn (batch creation) để tiết kiệm thời gian trong các dự án lớn. Trong các phần tiếp theo, bạn sẽ học cách gắn thẻ (tagging) phần tử và tạo tờ (sheets) để hoàn thiện tài liệu thiết kế.