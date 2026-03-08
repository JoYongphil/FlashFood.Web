# Flash Food - ASP.NET Core MVC (.NET 8)

Project web ban thuc an nhanh theo mo hinh ASP.NET Core MVC + EF Core + Identity.

## Chuc nang da co
- Khach/nguoi dung xem danh sach mon an, tim kiem theo ten va danh muc, sap xep theo gia/moi nhat.
- Chi tiet mon an, chon bien the, them vao gio hang (session).
- Thanh toan cho guest va user.
- Guest checkout bat buoc nhap ho ten, sdt, email, dia chi.
- Voucher chi dung cho user dang nhap:
  - Moi lan dang nhap, neu user khong co voucher con han thi he thong tao 1 voucher ngau nhien.
  - Voucher giam % (5-20%) hoac freeship.
  - Han su dung voucher: 24h.
- Tinh phi giao hang theo khoang cach:
  - <= 5km: 15,000d
  - > 5km va <= 10km: 30,000d
  - > 10km: khong tao don.
- Tich hop Google Distance Matrix API (co fallback nhap km thu cong neu chua co key).
- Don hang:
  - Trang thai: PendingConfirmation, Preparing, Delivering, Delivered, Completed, Cancelled.
  - User chi huy khi PendingConfirmation.
  - User xac nhan da nhan khi Delivered.
- Xuat hoa don PDF bang QuestPDF.
- Danh gia mon an (guest duoc phep, ten mac dinh la User).
- Admin:
  - Dashboard: tong doanh thu, tong so don, mat hang ban chay nhat (theo so luong), loc theo ngay/thang/nam + khoang thoi gian.
  - CRUD san pham (kem bien the)
  - CRUD danh muc
  - Quan ly don hang va cap nhat trang thai

## Cong nghe
- .NET 8 MVC
- ASP.NET Core Identity
- Entity Framework Core (SQL Server)
- QuestPDF
- Tailwind CDN cho giao dien

## Tai khoan mac dinh
- Admin email: `admin@flashfood.vn`
- Password: `Admin@123`

## Cau hinh truoc khi chay
1. Mo `appsettings.json`
2. Chinh connection string SQL Server neu can:
   - Mac dinh: `(localdb)\\MSSQLLocalDB`
3. Cau hinh Google Maps:
   - `GoogleMaps:ApiKey`: API key cua ban
   - `GoogleMaps:StoreAddress`: dia chi cua cua hang

## Lenh chay
```powershell
dotnet restore
dotnet run
```

## Script DB
- File script SQL tham khao: `database-script.sql`
