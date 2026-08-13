/* RFID Library Management Schema for Microsoft SQL Server */

IF DB_ID('RFIDLibrary') IS NULL
    CREATE DATABASE RFIDLibrary;
GO

USE RFIDLibrary;
GO

CREATE TABLE Books (
    BookID INT IDENTITY(1,1) PRIMARY KEY,
    ISBN NVARCHAR(20),
    AccessionNo NVARCHAR(50) NOT NULL UNIQUE,
    Title NVARCHAR(500),
    Subtitle NVARCHAR(500),
    AuthorName NVARCHAR(255),
    Publisher NVARCHAR(255),
    Edition NVARCHAR(100),
    PublicationYear SMALLINT,
    SubjectName NVARCHAR(255),
    LanguageName NVARCHAR(100),
    BookType NVARCHAR(50),
    CallNumber NVARCHAR(100),
    Status NVARCHAR(20) DEFAULT 'Available'
        CHECK (Status IN ('Available','Issued','Reserved','Lost','Damaged','Withdrawn')),
    CreatedAt DATETIME2 DEFAULT SYSDATETIME()
);
GO

CREATE TABLE RFIDTags (
    TagID INT IDENTITY PRIMARY KEY,
    EPC NVARCHAR(64) NOT NULL UNIQUE,
    TID NVARCHAR(64),
    TagType NVARCHAR(10) CHECK(TagType IN('HF','UHF')),
    TagStatus NVARCHAR(20) DEFAULT 'Unused'
        CHECK(TagStatus IN('Unused','Assigned','Lost','Damaged')),
    AssignedDate DATETIME2,
    Remarks NVARCHAR(MAX)
);
GO

CREATE TABLE Members (
    MemberID INT IDENTITY PRIMARY KEY,
    MemberCode NVARCHAR(50) UNIQUE,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Department NVARCHAR(200),
    Designation NVARCHAR(100),
    Email NVARCHAR(150),
    Mobile NVARCHAR(20),
    MemberType NVARCHAR(50),
    ValidityDate DATE,
    Status NVARCHAR(20) DEFAULT 'Active'
        CHECK(Status IN('Active','Inactive','Blocked'))
);
GO

CREATE TABLE BookRFIDMapping (
    MappingID INT IDENTITY PRIMARY KEY,
    BookID INT NOT NULL,
    TagID INT NOT NULL,
    AssignedBy NVARCHAR(100),
    AssignedOn DATETIME2 DEFAULT SYSDATETIME(),
    CONSTRAINT FK_BRM_Book FOREIGN KEY(BookID) REFERENCES Books(BookID),
    CONSTRAINT FK_BRM_Tag FOREIGN KEY(TagID) REFERENCES RFIDTags(TagID)
);
GO

CREATE TABLE CirculationTransactions (
    TransactionID BIGINT IDENTITY PRIMARY KEY,
    BookID INT NOT NULL,
    MemberID INT NOT NULL,
    IssueDate DATETIME2,
    DueDate DATETIME2,
    ReturnDate DATETIME2,
    IssueBy NVARCHAR(100),
    ReturnBy NVARCHAR(100),
    RenewalCount INT DEFAULT 0,
    TransactionStatus NVARCHAR(20)
        CHECK(TransactionStatus IN('Issued','Returned','Lost')),
    CONSTRAINT FK_CT_Book FOREIGN KEY(BookID) REFERENCES Books(BookID),
    CONSTRAINT FK_CT_Member FOREIGN KEY(MemberID) REFERENCES Members(MemberID)
);
GO

CREATE TABLE Fines (
    FineID INT IDENTITY PRIMARY KEY,
    TransactionID BIGINT,
    Amount DECIMAL(10,2),
    PaidAmount DECIMAL(10,2),
    PaymentDate DATETIME2,
    PaymentMode NVARCHAR(50),
    Remarks NVARCHAR(MAX),
    CONSTRAINT FK_Fine_CT FOREIGN KEY(TransactionID)
        REFERENCES CirculationTransactions(TransactionID)
);
GO

CREATE TABLE Readers (
    ReaderID INT IDENTITY PRIMARY KEY,
    ReaderName NVARCHAR(200),
    IPAddress NVARCHAR(50),
    LocationName NVARCHAR(255),
    ReaderType NVARCHAR(30),
    Status NVARCHAR(20)
);
GO

CREATE TABLE RFIDScanLog (
    ScanID BIGINT IDENTITY PRIMARY KEY,
    ReaderID INT,
    TagID INT,
    ScanTime DATETIME2 DEFAULT SYSDATETIME(),
    Antenna INT,
    RSSI DECIMAL(10,2),
    EventType NVARCHAR(20),
    CONSTRAINT FK_Scan_Reader FOREIGN KEY(ReaderID) REFERENCES Readers(ReaderID),
    CONSTRAINT FK_Scan_Tag FOREIGN KEY(TagID) REFERENCES RFIDTags(TagID)
);
GO

CREATE TABLE GateSecurityLog (
    GateLogID BIGINT IDENTITY PRIMARY KEY,
    ReaderID INT,
    TagID INT,
    MemberID INT NULL,
    EventTime DATETIME2 DEFAULT SYSDATETIME(),
    AlarmStatus NVARCHAR(20),
    Remarks NVARCHAR(MAX),
    FOREIGN KEY (ReaderID) REFERENCES Readers(ReaderID),
    FOREIGN KEY (TagID) REFERENCES RFIDTags(TagID),
    FOREIGN KEY (MemberID) REFERENCES Members(MemberID)
);
GO

CREATE TABLE InventorySessions (
    SessionID INT IDENTITY PRIMARY KEY,
    InventoryName NVARCHAR(255),
    StartTime DATETIME2,
    EndTime DATETIME2,
    ConductedBy NVARCHAR(100),
    Remarks NVARCHAR(MAX)
);
GO

CREATE TABLE InventoryDetails (
    DetailID BIGINT IDENTITY PRIMARY KEY,
    SessionID INT,
    BookID INT,
    TagID INT,
    ShelfLocation NVARCHAR(255),
    FoundStatus NVARCHAR(20),
    ScanTime DATETIME2,
    FOREIGN KEY(SessionID) REFERENCES InventorySessions(SessionID),
    FOREIGN KEY(BookID) REFERENCES Books(BookID),
    FOREIGN KEY(TagID) REFERENCES RFIDTags(TagID)
);
GO

CREATE TABLE Shelves (
    ShelfID INT IDENTITY PRIMARY KEY,
    FloorName NVARCHAR(100),
    RackNo NVARCHAR(50),
    ShelfNo NVARCHAR(50),
    LocationCode NVARCHAR(100)
);
GO

CREATE TABLE BookShelfMapping (
    ID INT IDENTITY PRIMARY KEY,
    BookID INT,
    ShelfID INT,
    UpdatedOn DATETIME2,
    FOREIGN KEY(BookID) REFERENCES Books(BookID),
    FOREIGN KEY(ShelfID) REFERENCES Shelves(ShelfID)
);
GO

CREATE TABLE Users (
    UserID INT IDENTITY PRIMARY KEY,
    Username NVARCHAR(100) UNIQUE,
    PasswordHash NVARCHAR(255),
    FullName NVARCHAR(255),
    UserRole NVARCHAR(30),
    Status NVARCHAR(20)
);
GO

CREATE TABLE AuditLog (
    LogID BIGINT IDENTITY PRIMARY KEY,
    UserID INT,
    ActionName NVARCHAR(255),
    TableName NVARCHAR(100),
    RecordID BIGINT,
    ActionTime DATETIME2 DEFAULT SYSDATETIME(),
    IPAddress NVARCHAR(50),
    FOREIGN KEY(UserID) REFERENCES Users(UserID)
);
GO
