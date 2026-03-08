-- FlashFood SQL Server script (reference)
CREATE DATABASE FlashFoodDb;
GO
USE FlashFoodDb;
GO

CREATE TABLE Categories (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(120) NOT NULL,
    Description NVARCHAR(400) NULL
);

CREATE TABLE Products (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(200) NOT NULL,
    BasePrice DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    IsAvailable BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CategoryId INT NOT NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(Id)
);

CREATE TABLE ProductVariants (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(120) NOT NULL,
    AdditionalPrice DECIMAL(18,2) NOT NULL,
    ProductId INT NOT NULL,
    CONSTRAINT FK_ProductVariants_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);

CREATE TABLE Vouchers (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(30) NOT NULL UNIQUE,
    Type INT NOT NULL, -- 1: Percentage, 2: FreeShip
    Value DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ExpiresAt DATETIME2 NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    UserId NVARCHAR(450) NULL
);

CREATE TABLE Orders (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderCode NVARCHAR(50) NOT NULL UNIQUE,
    UserId NVARCHAR(450) NULL,
    CustomerName NVARCHAR(200) NOT NULL,
    Phone NVARCHAR(30) NOT NULL,
    Email NVARCHAR(200) NOT NULL,
    Province NVARCHAR(120) NOT NULL,
    District NVARCHAR(120) NOT NULL,
    Ward NVARCHAR(120) NOT NULL,
    AddressDetail NVARCHAR(500) NOT NULL,
    Note NVARCHAR(500) NULL,
    Subtotal DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL,
    ShippingFee DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    DistanceKm DECIMAL(18,2) NOT NULL,
    PercentVoucherCode NVARCHAR(30) NULL,
    FreeShipVoucherCode NVARCHAR(30) NULL,
    IsPaid BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    Status INT NOT NULL -- 1 PendingConfirmation, 2 Preparing, 3 Delivering, 4 Delivered, 5 Completed, 6 Cancelled
);

CREATE TABLE OrderItems (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductVariantId INT NULL,
    ProductNameSnapshot NVARCHAR(200) NOT NULL,
    VariantNameSnapshot NVARCHAR(120) NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    Quantity INT NOT NULL,
    LineTotal DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderItems_Orders FOREIGN KEY (OrderId) REFERENCES Orders(Id),
    CONSTRAINT FK_OrderItems_Products FOREIGN KEY (ProductId) REFERENCES Products(Id),
    CONSTRAINT FK_OrderItems_ProductVariants FOREIGN KEY (ProductVariantId) REFERENCES ProductVariants(Id)
);

CREATE TABLE Reviews (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    ProductId INT NOT NULL,
    ReviewerName NVARCHAR(120) NOT NULL,
    Rating INT NOT NULL,
    Comment NVARCHAR(1000) NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Reviews_Products FOREIGN KEY (ProductId) REFERENCES Products(Id)
);
GO

INSERT INTO Categories(Name, Description)
VALUES
('Burgers', 'Signature burgers'),
('Ga ran', 'Crispy fried chicken'),
('Mon phu', 'Side dishes'),
('Do uong', 'Refreshing drinks');
GO
