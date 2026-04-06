# FlashFood Project Guide

## 1. Tong quan do an

FlashFood la website dat do an nhanh xay dung bang ASP.NET Core MVC (.NET 8), su dung Entity Framework Core, ASP.NET Core Identity, Google login, xac thuc email, thanh toan VNPAY Sandbox va giao dien Razor Views.

Project duoc thiet ke theo mo hinh MVC va chia tach phan Admin bang `Areas/Admin`.

## 2. Cong nghe su dung

- ASP.NET Core MVC (.NET 8)
- Razor Views
- ASP.NET Core Identity
- Entity Framework Core + SQL Server
- Session cart
- QuestPDF de xuat hoa don PDF
- Google Distance Matrix API
- Google OAuth login
- Gmail SMTP de gui email xac thuc va reset password
- VNPAY Sandbox
- Tailwind CDN

## 3. Chuc nang hien co

### 3.1. Khach hang / Nguoi dung

- Xem danh sach mon an.
- Tim kiem theo ten mon.
- Loc theo danh muc.
- Sap xep theo gia tang/giam va moi nhat.
- Xem chi tiet san pham.
- Chon bien the mon an.
- Them vao gio hang bang session.
- Dat hang khi dang nhap hoac khi la guest.
- Tinh phi ship theo khoang cach.
- Nhap khoang cach du phong neu chua cau hinh Google Maps API.
- Theo doi don hang cua guest trong phien lam viec hien tai.
- Xem don hang cua tai khoan da dang nhap.
- Tai hoa don PDF.
- Danh gia mon an.

### 3.2. Tai khoan

- Dang ky bang email/password.
- Bat buoc xac thuc email truoc khi dang nhap.
- Dang nhap bang Google.
- Neu Google login lan dau, he thong tu tao tai khoan.
- Neu email Google trung voi tai khoan thuong, he thong lien ket cung mot tai khoan.
- Quen mat khau qua email reset link.
- Gui lai email xac nhan.
- Cap nhat thong tin ca nhan.

### 3.3. Voucher

- User dang nhap duoc cap voucher ngam dinh.
- Voucher giam phan tram hoac freeship.
- Ap dung voucher trong checkout.

### 3.4. Thanh toan

- Tao don hang truoc khi thanh toan.
- Thanh toan qua VNPAY Sandbox.
- Cap nhat `IsPaid` khi thanh toan thanh cong.
- Ho tro thanh toan lai neu don chua thanh toan.
- Co callback `return` va `ipn`.

### 3.5. Don hang

- Trang thai don hang:
  - `PendingPayment`
  - `PendingConfirmation`
  - `Preparing`
  - `Delivering`
  - `Delivered`
  - `Completed`
  - `Cancelled`
- User co the huy don khi don dang cho thanh toan hoac cho xac nhan.
- User co the xac nhan da nhan hang khi don o trang thai `Delivered`.

### 3.6. Admin

- Dashboard thong ke tong doanh thu.
- Dashboard thong ke tong so don.
- Dashboard thong ke mon ban chay.
- Loc thong ke theo ngay, thang, nam, hoac khoang thoi gian.
- CRUD san pham.
- CRUD bien the san pham.
- CRUD danh muc.
- Quan ly don hang.
- Cap nhat trang thai don hang.

## 4. Goi y kich ban quay demo

### 4.1. Demo user

1. Mo trang chu, tim kiem va loc mon an.
2. Vao chi tiet mon, chon bien the, them vao gio hang.
3. Dang ky tai khoan moi.
4. Mo email va xac thuc tai khoan.
5. Dang nhap lai.
6. Vao checkout, ap voucher.
7. Thanh toan qua VNPAY Sandbox.
8. Quay ve trang thanh cong.
9. Mo chi tiet don hang va tai PDF hoa don.

### 4.2. Demo guest

1. Them san pham vao gio ma khong dang nhap.
2. Thanh toan guest.
3. Theo doi don trong `GuestOrders`.

### 4.3. Demo admin

1. Dang nhap bang tai khoan admin.
2. Vao dashboard de xem thong ke.
3. Vao quan ly don hang.
4. Cap nhat trang thai don.
5. Vao quan ly san pham va danh muc.

## 5. Tai khoan mac dinh

- Admin email: `admin@flashfood.vn`
- Admin password: `Admin@123`

## 6. Cac cau hinh can co khi chay tren may moi

Project nay can nhung nhom cau hinh sau:

- SQL Server connection string
- Google Maps API
- Gmail SMTP
- Google OAuth
- VNPAY Sandbox

Khuyen nghi:

- `appsettings.json` va `appsettings.Development.json` chi de placeholder.
- Secret that nen luu bang `dotnet user-secrets` hoac environment variables.

## 7. Mau cau hinh placeholder

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=FlashFoodDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
  },
  "GoogleMaps": {
    "ApiKey": "",
    "StoreAddress": "123 Nguyen Hue, Quan 1, Ho Chi Minh"
  },
  "VnPay": {
    "TmnCode": "",
    "HashSecret": "",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "ReturnUrl": "",
    "IpnUrl": ""
  },
  "Email": {
    "SenderName": "Flash Food",
    "SenderEmail": "",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "Username": "",
    "Password": "",
    "EnableSsl": true
  },
  "Authentication": {
    "Google": {
      "ClientId": "",
      "ClientSecret": ""
    }
  }
}
```

## 8. Cach chay project o may moi

### 8.1. Chuan bi

- Cai .NET SDK 8
- Cai SQL Server hoac LocalDB
- Clone source code

### 8.2. Cau hinh secret bang user-secrets

Chay trong thu muc project:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=(localdb)\\MSSQLLocalDB;Database=FlashFoodDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"

dotnet user-secrets set "GoogleMaps:ApiKey" "YOUR_GOOGLE_MAPS_API_KEY"
dotnet user-secrets set "GoogleMaps:StoreAddress" "123 Nguyen Hue, Quan 1, Ho Chi Minh"

dotnet user-secrets set "Email:SenderName" "Flash Food"
dotnet user-secrets set "Email:SenderEmail" "your-project-mail@gmail.com"
dotnet user-secrets set "Email:SmtpHost" "smtp.gmail.com"
dotnet user-secrets set "Email:SmtpPort" "587"
dotnet user-secrets set "Email:Username" "your-project-mail@gmail.com"
dotnet user-secrets set "Email:Password" "YOUR_GMAIL_APP_PASSWORD"
dotnet user-secrets set "Email:EnableSsl" "true"

dotnet user-secrets set "Authentication:Google:ClientId" "YOUR_GOOGLE_CLIENT_ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "YOUR_GOOGLE_CLIENT_SECRET"

dotnet user-secrets set "VnPay:TmnCode" "YOUR_VNPAY_TMN_CODE"
dotnet user-secrets set "VnPay:HashSecret" "YOUR_VNPAY_HASH_SECRET"
dotnet user-secrets set "VnPay:BaseUrl" "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html"
dotnet user-secrets set "VnPay:ReturnUrl" "https://your-domain/payment/vnpay-return"
dotnet user-secrets set "VnPay:IpnUrl" "https://your-domain/payment/vnpay-ipn"
```

### 8.3. Khoi phuc package va chay

```powershell
dotnet restore
dotnet build
dotnet run
```

### 8.4. Neu chay VNPAY tren may local

- Can co domain public de callback.
- Co the dung `ngrok`.
- Cap nhat `ReturnUrl` va `IpnUrl` theo domain public dang dung.

## 9. Viec can lam truoc khi push GitHub

- Xoa toan bo secret that khoi `appsettings*.json`.
- Khong push Gmail App Password.
- Khong push Google Client Secret.
- Khong push VNPAY HashSecret.
- Khong push file database backup `.bak`, `.bacpac`, `.db`.
- Khong push thu muc build tam va thu muc local nhu:
  - `.dotnet/`
  - `appdata/`
  - `build-verify/`
  - `build-verify FlashFood.Web.csproj/`

## 10. Ghi chu

- Sau khi doi secret that thanh placeholder, neu secret da tung duoc commit/push thi nen rotate:
  - Gmail App Password
  - VNPAY secret
  - Google Client Secret

