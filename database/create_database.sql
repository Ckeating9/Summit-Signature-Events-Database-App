-- Summit Signature Events - Part 1

IF DB_ID('SummitSignatureEventsPart1') IS NULL
BEGIN
    CREATE DATABASE SummitSignatureEventsPart1;
END;
GO

USE SummitSignatureEventsPart1;
GO

SET NOCOUNT ON;
GO

DROP VIEW IF EXISTS dbo.vw_EventSummary;
GO
DROP PROCEDURE IF EXISTS dbo.usp_AssignEmployeeToEvent;
GO
DROP FUNCTION IF EXISTS dbo.ufn_TotalConfirmedVendorCost;
GO

DROP TABLE IF EXISTS dbo.EventStaff;
DROP TABLE IF EXISTS dbo.EventVendor;
DROP TABLE IF EXISTS dbo.Event;
DROP TABLE IF EXISTS dbo.Vendor;
DROP TABLE IF EXISTS dbo.VendorCategory;
DROP TABLE IF EXISTS dbo.Employee;
DROP TABLE IF EXISTS dbo.EventType;
DROP TABLE IF EXISTS dbo.Venue;
DROP TABLE IF EXISTS dbo.Client;
GO

-- Tables

CREATE TABLE dbo.Client
(
    ClientID INT NOT NULL,
    FirstName VARCHAR(30) NOT NULL,
    LastName VARCHAR(30) NOT NULL,
    Email VARCHAR(60) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    CompanyName VARCHAR(60) NULL,
    CONSTRAINT PK_Client PRIMARY KEY (ClientID),
    CONSTRAINT UQ_Client_Email UNIQUE (Email)
);
GO

CREATE TABLE dbo.Venue
(
    VenueID INT NOT NULL,
    VenueName VARCHAR(60) NOT NULL,
    City VARCHAR(30) NOT NULL,
    StateCode CHAR(2) NOT NULL,
    Capacity INT NOT NULL,
    RentalFee DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_Venue PRIMARY KEY (VenueID),
    CONSTRAINT CK_Venue_Capacity CHECK (Capacity > 0),
    CONSTRAINT CK_Venue_RentalFee CHECK (RentalFee >= 0));
GO

CREATE TABLE dbo.EventType
(
    EventTypeID INT NOT NULL,
    EventTypeName VARCHAR(30) NOT NULL,
    StandardDurationHours DECIMAL(4,1) NOT NULL,
    IndoorFlag CHAR(1) NOT NULL,
    CONSTRAINT PK_EventType PRIMARY KEY (EventTypeID),
    CONSTRAINT UQ_EventType_Name UNIQUE (EventTypeName),
    CONSTRAINT CK_EventType_Duration CHECK (StandardDurationHours > 0),
    CONSTRAINT CK_EventType_IndoorFlag CHECK (IndoorFlag IN ('Y', 'N')));
GO

CREATE TABLE dbo.Employee
(
    EmployeeID INT NOT NULL,
    FirstName VARCHAR(30) NOT NULL,
    LastName VARCHAR(30) NOT NULL,
    JobTitle VARCHAR(40) NOT NULL,
    HireDate DATE NOT NULL,
    HourlyRate DECIMAL(8,2) NOT NULL,
    Email VARCHAR(60) NOT NULL,
    CONSTRAINT PK_Employee PRIMARY KEY (EmployeeID),
    CONSTRAINT UQ_Employee_Email UNIQUE (Email),
    CONSTRAINT CK_Employee_HourlyRate CHECK (HourlyRate > 0));
GO

CREATE TABLE dbo.VendorCategory
(
    VendorCategoryID INT NOT NULL,
    CategoryName VARCHAR(30) NOT NULL,
    RequiresContract CHAR(1) NOT NULL,
    CONSTRAINT PK_VendorCategory PRIMARY KEY (VendorCategoryID),
    CONSTRAINT UQ_VendorCategory_Name UNIQUE (CategoryName),
    CONSTRAINT CK_VendorCategory_RequiresContract CHECK (RequiresContract IN ('Y', 'N')));
GO

CREATE TABLE dbo.Vendor
(
    VendorID INT NOT NULL,
    VendorCategoryID INT NOT NULL,
    VendorName VARCHAR(60) NOT NULL,
    Phone VARCHAR(20) NOT NULL,
    Email VARCHAR(60) NOT NULL,
    BaseFee DECIMAL(10,2) NOT NULL,
    CONSTRAINT PK_Vendor PRIMARY KEY (VendorID),
    CONSTRAINT UQ_Vendor_Email UNIQUE (Email),
    CONSTRAINT FK_Vendor_VendorCategory FOREIGN KEY (VendorCategoryID)
        REFERENCES dbo.VendorCategory (VendorCategoryID),
    CONSTRAINT CK_Vendor_BaseFee CHECK (BaseFee >= 0));
GO

CREATE TABLE dbo.Event
(
    EventID INT NOT NULL,
    ClientID INT NOT NULL,
    VenueID INT NOT NULL,
    EventTypeID INT NOT NULL,
    LeadPlannerEmployeeID INT NOT NULL,
    EventName VARCHAR(80) NOT NULL,
    EventDate DATE NOT NULL,
    GuestCount INT NOT NULL,
    Budget DECIMAL(12,2) NOT NULL,
    Status VARCHAR(15) NOT NULL,
    CONSTRAINT PK_Event PRIMARY KEY (EventID),
    CONSTRAINT FK_Event_Client FOREIGN KEY (ClientID)
        REFERENCES dbo.Client (ClientID),
    CONSTRAINT FK_Event_Venue FOREIGN KEY (VenueID)
        REFERENCES dbo.Venue (VenueID),
    CONSTRAINT FK_Event_EventType FOREIGN KEY (EventTypeID)
        REFERENCES dbo.EventType (EventTypeID),
    CONSTRAINT FK_Event_LeadPlanner FOREIGN KEY (LeadPlannerEmployeeID)
        REFERENCES dbo.Employee (EmployeeID),
    CONSTRAINT CK_Event_GuestCount CHECK (GuestCount > 0),
    CONSTRAINT CK_Event_Budget CHECK (Budget >= 0),
    CONSTRAINT CK_Event_Status CHECK (Status IN ('Planned', 'Confirmed', 'Completed')));
GO

CREATE TABLE dbo.EventVendor
(
    EventID INT NOT NULL,
    VendorID INT NOT NULL,
    ContractedFee DECIMAL(10,2) NOT NULL,
    ConfirmedFlag CHAR(1) NOT NULL,
    CONSTRAINT PK_EventVendor PRIMARY KEY (EventID, VendorID),
    CONSTRAINT FK_EventVendor_Event FOREIGN KEY (EventID)
        REFERENCES dbo.Event (EventID),
    CONSTRAINT FK_EventVendor_Vendor FOREIGN KEY (VendorID)
        REFERENCES dbo.Vendor (VendorID),
    CONSTRAINT CK_EventVendor_ContractedFee CHECK (ContractedFee >= 0),
    CONSTRAINT CK_EventVendor_ConfirmedFlag CHECK (ConfirmedFlag IN ('Y', 'N')));
GO

CREATE TABLE dbo.EventStaff
(
    EventID INT NOT NULL,
    EmployeeID INT NOT NULL,
    StaffRole VARCHAR(40) NOT NULL,
    HoursWorked DECIMAL(5,2) NOT NULL,
    CONSTRAINT PK_EventStaff PRIMARY KEY (EventID, EmployeeID),
    CONSTRAINT FK_EventStaff_Event FOREIGN KEY (EventID)
        REFERENCES dbo.Event (EventID),
    CONSTRAINT FK_EventStaff_Employee FOREIGN KEY (EmployeeID)
        REFERENCES dbo.Employee (EmployeeID),
    CONSTRAINT CK_EventStaff_HoursWorked CHECK (HoursWorked >= 0));
GO

-- Test data

INSERT INTO dbo.Client (ClientID, FirstName, LastName, Email, Phone, CompanyName)
VALUES
    (101, 'Amelia', 'Brooks', 'amelia.brooks@summitclient.com', '303-555-0101', NULL),
    (102, 'Noah', 'Parker', 'noah.parker@summitclient.com', '303-555-0102', NULL),
    (103, 'Olivia', 'Carter', 'olivia.carter@summitclient.com', '303-555-0103', NULL),
    (104, 'Liam', 'Reed', 'liam.reed@summitclient.com', '303-555-0104', NULL),
    (105, 'Sophia', 'Morgan', 'sophia.morgan@summitclient.com', '303-555-0105', NULL),
    (106, 'Ethan', 'Price', 'ethan.price@summitclient.com', '303-555-0106', NULL),
    (107, 'Ava', 'Bennett', 'ava.bennett@summitclient.com', '303-555-0107', NULL),
    (108, 'Mason', 'Collins', 'mason.collins@summitclient.com', '303-555-0108', NULL),
    (109, 'Isabella', 'Foster', 'isabella.foster@summitclient.com', '303-555-0109', NULL),
    (110, 'Lucas', 'Hayes', 'lucas.hayes@summitclient.com', '303-555-0110', NULL),
    (111, 'Charlotte', 'Ward', 'charlotte.ward@alpineventures.com', '303-555-0111', 'Alpine Ventures'),
    (112, 'James', 'Turner', 'james.turner@northpeakpartners.com', '303-555-0112', 'North Peak Partners'),
    (113, 'Mia', 'Gray', 'mia.gray@elevatefoundation.org', '303-555-0113', 'Elevate Foundation'),
    (114, 'Benjamin', 'Ross', 'benjamin.ross@harborlegal.com', '303-555-0114', 'Harbor Legal'),
    (115, 'Harper', 'Bell', 'harper.bell@larkcreative.co', '303-555-0115', 'Lark Creative'),
    (116, 'Elijah', 'Cooper', 'elijah.cooper@copperstateholdings.com', '303-555-0116', 'Copper State Holdings'),
    (117, 'Evelyn', 'Cook', 'evelyn.cook@wildflowerstudio.com', '303-555-0117', 'Wildflower Studio'),
    (118, 'Henry', 'Bailey', 'henry.bailey@westlineadvisors.com', '303-555-0118', 'Westline Advisors'),
    (119, 'Abigail', 'Flores', 'abigail.flores@civicimpact.org', '303-555-0119', 'Civic Impact'),
    (120, 'Alexander', 'Wood', 'alexander.wood@silveroakgroup.com', '303-555-0120', 'Silver Oak Group');
GO

INSERT INTO dbo.Venue (VenueID, VenueName, City, StateCode, Capacity, RentalFee)
VALUES
    (201, 'Aspen Ridge Ballroom', 'Denver', 'CO', 320, 8500.00),
    (202, 'Mile High Terrace', 'Denver', 'CO', 180, 5200.00),
    (203, 'Crystal Lake Lodge', 'Boulder', 'CO', 220, 6100.00),
    (204, 'Juniper Hall', 'Fort Collins', 'CO', 140, 4300.00),
    (205, 'Copper Sky Pavilion', 'Colorado Springs', 'CO', 400, 9800.00),
    (206, 'The Mercer Loft', 'Denver', 'CO', 160, 4900.00),
    (207, 'Riverside Conservatory', 'Boulder', 'CO', 250, 7200.00),
    (208, 'Granite Peak Club', 'Vail', 'CO', 180, 7600.00),
    (209, 'Aurora Garden House', 'Aurora', 'CO', 120, 3900.00),
    (210, 'Crescent View Estate', 'Castle Rock', 'CO', 275, 8100.00),
    (211, 'Sagebrush Barn', 'Longmont', 'CO', 200, 4700.00),
    (212, 'Union Station Gallery', 'Denver', 'CO', 150, 5600.00),
    (213, 'Bluebird Rooftop', 'Denver', 'CO', 130, 5100.00),
    (214, 'Pinecrest Manor', 'Estes Park', 'CO', 240, 6900.00),
    (215, 'Redstone Hall', 'Golden', 'CO', 210, 6300.00),
    (216, 'Summit View Tent Lawn', 'Breckenridge', 'CO', 350, 9200.00),
    (217, 'Foothill Forum', 'Lakewood', 'CO', 170, 4500.00),
    (218, 'Maple & Main Studios', 'Boulder', 'CO', 110, 3600.00),
    (219, 'The Observatory Room', 'Denver', 'CO', 145, 5400.00),
    (220, 'Silver Birch Atrium', 'Littleton', 'CO', 190, 5800.00);
GO

INSERT INTO dbo.EventType (EventTypeID, EventTypeName, StandardDurationHours, IndoorFlag)
VALUES
    (301, 'Wedding', 8.00, 'N'),
    (302, 'Corporate Gala', 5.00, 'Y'),
    (303, 'Fundraiser', 4.50, 'Y'),
    (304, 'Birthday Dinner', 3.00, 'N'),
    (305, 'Anniversary Party', 4.00, 'N'),
    (306, 'Holiday Party', 4.00, 'Y'),
    (307, 'Product Launch', 4.50, 'Y'),
    (308, 'Awards Banquet', 5.00, 'Y'),
    (309, 'Bridal Shower', 3.50, 'N'),
    (310, 'Baby Shower', 3.50, 'N'),
    (311, 'Retirement Reception', 3.00, 'Y'),
    (312, 'Conference Dinner', 4.00, 'Y'),
    (313, 'Networking Mixer', 2.50, 'Y'),
    (314, 'Charity Luncheon', 3.00, 'Y'),
    (315, 'Engagement Party', 4.00, 'N'),
    (316, 'Rehearsal Dinner', 3.00, 'N'),
    (317, 'Graduation Celebration', 3.00, 'N'),
    (318, 'Fashion Showcase', 4.50, 'Y'),
    (319, 'VIP Reception', 2.50, 'Y'),
    (320, 'Community Festival', 6.00, 'N');
GO

INSERT INTO dbo.Employee (EmployeeID, FirstName, LastName, JobTitle, HireDate, HourlyRate, Email)
VALUES
    (401, 'Grace', 'Mitchell', 'Senior Planner', '2020-03-15', 42.00, 'grace.mitchell@summitsignature.com'),
    (402, 'Daniel', 'Kim', 'Planner', '2021-06-10', 34.00, 'daniel.kim@summitsignature.com'),
    (403, 'Natalie', 'Young', 'Planner', '2022-01-08', 33.50, 'natalie.young@summitsignature.com'),
    (404, 'Owen', 'Scott', 'Venue Coordinator', '2021-09-20', 29.00, 'owen.scott@summitsignature.com'),
    (405, 'Ella', 'Diaz', 'Vendor Manager', '2020-11-02', 31.50, 'ella.diaz@summitsignature.com'),
    (406, 'Jack', 'Evans', 'Production Lead', '2019-07-01', 37.00, 'jack.evans@summitsignature.com'),
    (407, 'Lily', 'Rivera', 'Designer', '2022-04-18', 30.00, 'lily.rivera@summitsignature.com'),
    (408, 'William', 'Powell', 'Operations Coordinator', '2023-02-13', 28.00, 'william.powell@summitsignature.com'),
    (409, 'Scarlett', 'Long', 'Planner', '2023-05-09', 32.00, 'scarlett.long@summitsignature.com'),
    (410, 'Samuel', 'Peterson', 'Client Services', '2022-07-25', 27.50, 'samuel.peterson@summitsignature.com'),
    (411, 'Chloe', 'Sanders', 'Floral Specialist', '2021-03-29', 29.50, 'chloe.sanders@summitsignature.com'),
    (412, 'Logan', 'Bryant', 'AV Coordinator', '2020-08-17', 31.00, 'logan.bryant@summitsignature.com'),
    (413, 'Zoe', 'Jenkins', 'Planner', '2024-01-15', 30.50, 'zoe.jenkins@summitsignature.com'),
    (414, 'Aiden', 'Price', 'Warehouse Lead', '2019-10-07', 26.00, 'aiden.price@summitsignature.com'),
    (415, 'Hannah', 'Coleman', 'Planner', '2023-08-14', 31.00, 'hannah.coleman@summitsignature.com'),
    (416, 'Levi', 'Rogers', 'Setup Supervisor', '2020-05-11', 28.50, 'levi.rogers@summitsignature.com'),
    (417, 'Penelope', 'Simmons', 'Guest Experience Lead', '2021-12-06', 29.00, 'penelope.simmons@summitsignature.com'),
    (418, 'Julian', 'Hughes', 'Lighting Technician', '2022-09-19', 27.00, 'julian.hughes@summitsignature.com'),
    (419, 'Layla', 'Fisher', 'Planner', '2024-03-04', 30.00, 'layla.fisher@summitsignature.com'),
    (420, 'Gabriel', 'Watson', 'Staffing Coordinator', '2023-10-30', 28.00, 'gabriel.watson@summitsignature.com');
GO

INSERT INTO dbo.VendorCategory (VendorCategoryID, CategoryName, RequiresContract)
VALUES
    (501, 'Catering', 'Y'),
    (502, 'Floral', 'Y'),
    (503, 'Photography', 'Y'),
    (504, 'Videography', 'Y'),
    (505, 'Music', 'Y'),
    (506, 'Lighting', 'Y'),
    (507, 'Rentals', 'Y'),
    (508, 'Dessert', 'Y'),
    (509, 'Security', 'Y'),
    (510, 'Transportation', 'Y'),
    (511, 'Decor', 'Y'),
    (512, 'Audio Visual', 'Y'),
    (513, 'Entertainment', 'Y'),
    (514, 'Furniture', 'Y'),
    (515, 'Printing', 'N'),
    (516, 'Beauty', 'N'),
    (517, 'Staffing', 'Y'),
    (518, 'Valet', 'Y'),
    (519, 'Tent', 'Y'),
    (520, 'Bar Service', 'Y');
GO

INSERT INTO dbo.Vendor (VendorID, VendorCategoryID, VendorName, Phone, Email, BaseFee)
VALUES
    (601, 501, 'Peak Table Catering', '720-555-0601', 'contact@peaktable.com', 6200.00),
    (602, 502, 'Evergreen Floral Co', '720-555-0602', 'hello@evergreenfloral.com', 2400.00),
    (603, 503, 'Frame & Focus Photo', '720-555-0603', 'team@framefocus.com', 3100.00),
    (604, 504, 'Motion House Films', '720-555-0604', 'info@motionhousefilms.com', 3600.00),
    (605, 505, 'Alpine Sound Collective', '720-555-0605', 'booking@alpinesound.com', 2800.00),
    (606, 506, 'Luma Event Lighting', '720-555-0606', 'studio@lumaevents.com', 2600.00),
    (607, 507, 'Summit Party Rentals', '720-555-0607', 'sales@summitrentals.com', 4200.00),
    (608, 508, 'Sugar Peak Desserts', '720-555-0608', 'orders@sugarpeak.com', 1700.00),
    (609, 509, 'Mile High Event Security', '720-555-0609', 'dispatch@mhesecurity.com', 2200.00),
    (610, 510, 'Front Range Chauffeur', '720-555-0610', 'ops@frchauffeur.com', 1900.00),
    (611, 511, 'Canvas & Cedar Decor', '720-555-0611', 'team@canvascedar.com', 2500.00),
    (612, 512, 'Vertex AV Group', '720-555-0612', 'service@vertexav.com', 3400.00),
    (613, 513, 'North Star Performers', '720-555-0613', 'acts@northstarperformers.com', 3300.00),
    (614, 514, 'Loft Lounge Furnishings', '720-555-0614', 'bookings@loftlounge.com', 2900.00),
    (615, 515, 'Foil Press Studio', '720-555-0615', 'print@foilpress.com', 900.00),
    (616, 516, 'Glow Bridal Beauty', '720-555-0616', 'artists@glowbridal.com', 1500.00),
    (617, 517, 'Premier Event Staffing', '720-555-0617', 'hello@premierstaffing.com', 2300.00),
    (618, 518, 'Red Carpet Valet', '720-555-0618', 'service@redcarpetvalet.com', 1400.00),
    (619, 519, 'Skyline Tent Works', '720-555-0619', 'sales@skylinetent.com', 4800.00),
    (620, 520, 'Copper Bar Collective', '720-555-0620', 'events@copperbar.com', 2600.00);
GO

INSERT INTO dbo.Event (EventID, ClientID, VenueID, EventTypeID, LeadPlannerEmployeeID, EventName, EventDate, GuestCount, Budget, Status)
VALUES
    (701, 101, 201, 301, 401, 'Brooks Wedding Reception', '2026-05-16', 180, 28500.00, 'Confirmed'),
    (702, 102, 212, 302, 402, 'Parker Investor Gala', '2026-05-30', 120, 22000.00, 'Planned'),
    (703, 103, 207, 303, 403, 'Carter Foundation Benefit', '2026-06-06', 150, 24000.00, 'Confirmed'),
    (704, 104, 206, 304, 409, 'Reed 40th Birthday Dinner', '2026-06-12', 60, 8500.00, 'Planned'),
    (705, 105, 214, 305, 415, 'Morgan Anniversary Party', '2026-06-20', 110, 17500.00, 'Confirmed'),
    (706, 106, 205, 306, 419, 'Price Holiday Celebration', '2026-07-03', 240, 32000.00, 'Planned'),
    (707, 107, 213, 307, 401, 'Bennett Product Launch', '2026-07-10', 90, 19000.00, 'Confirmed'),
    (708, 108, 215, 308, 402, 'Collins Leadership Awards', '2026-07-18', 140, 21000.00, 'Confirmed'),
    (709, 109, 218, 309, 403, 'Foster Bridal Shower', '2026-07-25', 45, 6800.00, 'Completed'),
    (710, 110, 209, 310, 409, 'Hayes Baby Shower', '2026-08-01', 50, 7200.00, 'Confirmed'),
    (711, 111, 220, 311, 415, 'Ward Retirement Reception', '2026-08-08', 95, 11800.00, 'Planned'),
    (712, 112, 203, 312, 419, 'North Peak Conference Dinner', '2026-08-15', 160, 23000.00, 'Confirmed'),
    (713, 113, 219, 313, 401, 'Elevate Networking Mixer', '2026-08-22', 85, 9600.00, 'Completed'),
    (714, 114, 217, 314, 402, 'Harbor Legal Charity Luncheon', '2026-08-29', 130, 16500.00, 'Confirmed'),
    (715, 115, 204, 315, 403, 'Bell Engagement Party', '2026-09-05', 70, 9800.00, 'Planned'),
    (716, 116, 210, 316, 409, 'Copper State Rehearsal Dinner', '2026-09-12', 80, 12600.00, 'Confirmed'),
    (717, 117, 216, 317, 415, 'Wildflower Graduation Celebration', '2026-09-19', 200, 27000.00, 'Planned'),
    (718, 118, 208, 318, 419, 'Westline Fashion Showcase', '2026-09-26', 170, 29500.00, 'Confirmed'),
    (719, 119, 202, 319, 401, 'Civic Impact VIP Reception', '2026-10-03', 75, 11200.00, 'Confirmed'),
    (720, 120, 211, 320, 402, 'Silver Oak Community Festival', '2026-10-10', 260, 34000.00, 'Planned');
GO

INSERT INTO dbo.EventVendor (EventID, VendorID, ContractedFee, ConfirmedFlag)
VALUES
    (701, 601, 6500.00, 'Y'),
    (701, 602, 2600.00, 'Y'),
    (702, 612, 3600.00, 'Y'),
    (702, 620, 3000.00, 'N'),
    (703, 601, 6300.00, 'Y'),
    (703, 609, 2400.00, 'Y'),
    (704, 608, 1800.00, 'N'),
    (704, 611, 2600.00, 'Y'),
    (705, 602, 2500.00, 'Y'),
    (705, 605, 2900.00, 'Y'),
    (706, 601, 7000.00, 'Y'),
    (706, 620, 2800.00, 'N'),
    (707, 612, 3500.00, 'Y'),
    (707, 613, 3400.00, 'Y'),
    (708, 601, 6400.00, 'Y'),
    (708, 615, 950.00, 'Y'),
    (709, 602, 2450.00, 'Y'),
    (709, 608, 1750.00, 'Y'),
    (710, 608, 1850.00, 'Y'),
    (710, 616, 1550.00, 'N'),
    (711, 605, 2900.00, 'N'),
    (711, 617, 2600.00, 'Y'),
    (712, 601, 6600.00, 'Y'),
    (712, 612, 3550.00, 'Y'),
    (713, 620, 2750.00, 'Y'),
    (713, 618, 1450.00, 'Y'),
    (714, 601, 6100.00, 'Y'),
    (714, 615, 1000.00, 'N'),
    (715, 602, 2350.00, 'Y'),
    (715, 616, 1600.00, 'Y'),
    (716, 601, 6200.00, 'Y'),
    (716, 620, 2850.00, 'Y'),
    (717, 607, 4300.00, 'N'),
    (717, 619, 5000.00, 'Y'),
    (718, 605, 3100.00, 'Y'),
    (718, 606, 2750.00, 'Y'),
    (719, 620, 2700.00, 'Y'),
    (719, 618, 1500.00, 'Y'),
    (720, 601, 6800.00, 'Y'),
    (720, 617, 2500.00, 'N');
GO

INSERT INTO dbo.EventStaff (EventID, EmployeeID, StaffRole, HoursWorked)
VALUES
    (701, 406, 'Production Lead', 10.00),
    (701, 417, 'Guest Services Lead', 8.00),
    (702, 404, 'Venue Coordinator', 6.00),
    (702, 420, 'Staffing Coordinator', 5.50),
    (703, 405, 'Vendor Manager', 7.50),
    (703, 417, 'Guest Services Lead', 6.50),
    (704, 407, 'Design Lead', 5.00),
    (704, 408, 'Operations Coordinator', 4.50),
    (705, 411, 'Floral Specialist', 8.00),
    (705, 417, 'Guest Services Lead', 6.00),
    (706, 406, 'Production Lead', 9.00),
    (706, 416, 'Setup Supervisor', 8.00),
    (707, 412, 'AV Coordinator', 6.50),
    (707, 418, 'Lighting Technician', 5.50),
    (708, 408, 'Operations Coordinator', 7.00),
    (708, 420, 'Staffing Coordinator', 5.00),
    (709, 411, 'Floral Specialist', 5.00),
    (709, 417, 'Guest Services Lead', 4.00),
    (710, 410, 'Client Services', 5.50),
    (710, 417, 'Guest Services Lead', 4.50),
    (711, 404, 'Venue Coordinator', 6.00),
    (711, 420, 'Staffing Coordinator', 5.00),
    (712, 406, 'Production Lead', 8.00),
    (712, 412, 'AV Coordinator', 7.00),
    (713, 410, 'Client Services', 4.50),
    (713, 420, 'Staffing Coordinator', 4.00),
    (714, 405, 'Vendor Manager', 7.00),
    (714, 417, 'Guest Services Lead', 6.00),
    (715, 407, 'Design Lead', 5.50),
    (715, 410, 'Client Services', 4.50),
    (716, 406, 'Production Lead', 7.50),
    (716, 417, 'Guest Services Lead', 6.00),
    (717, 414, 'Warehouse Lead', 9.50),
    (717, 416, 'Setup Supervisor', 8.50),
    (718, 412, 'AV Coordinator', 7.00),
    (718, 418, 'Lighting Technician', 6.00),
    (719, 410, 'Client Services', 5.00),
    (719, 417, 'Guest Services Lead', 4.50),
    (720, 414, 'Warehouse Lead', 10.00),
    (720, 420, 'Staffing Coordinator', 9.00);
GO


-- View

CREATE VIEW dbo.vw_EventSummary
AS
SELECT
    E.EventID,
    E.EventName,
    E.EventDate,
    C.FirstName + ' ' + C.LastName AS ClientName,
    V.VenueName,
    ET.EventTypeName,
    E.GuestCount,
    E.Budget,
    E.Status
FROM dbo.Event AS E
JOIN dbo.Client AS C
    ON E.ClientID = C.ClientID
JOIN dbo.Venue AS V
    ON E.VenueID = V.VenueID
JOIN dbo.EventType AS ET
    ON E.EventTypeID = ET.EventTypeID;
GO

-- User-defined function

CREATE FUNCTION dbo.ufn_TotalConfirmedVendorCost
(
    @EventID INT
)
RETURNS DECIMAL(12,2)
AS
BEGIN
    DECLARE @Total DECIMAL(12,2);

    SELECT @Total = ISNULL(SUM(ContractedFee), 0.00)
    FROM dbo.EventVendor
    WHERE EventID = @EventID
      AND ConfirmedFlag = 'Y';

    RETURN @Total;
END;
GO

-- Procedure

CREATE PROCEDURE dbo.usp_AssignEmployeeToEvent
    @EventID INT,
    @EmployeeID INT,
    @StaffRole VARCHAR(40),
    @HoursWorked DECIMAL(5,2)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Event
        WHERE EventID = @EventID
    )
    BEGIN
        RAISERROR ('EventID not found.', 16, 1);
        RETURN;
    END;

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Employee
        WHERE EmployeeID = @EmployeeID
    )
    BEGIN
        RAISERROR ('EmployeeID not found.', 16, 1);
        RETURN;
    END;

    IF EXISTS (
        SELECT 1
        FROM dbo.EventStaff
        WHERE EventID = @EventID
          AND EmployeeID = @EmployeeID
    )
    BEGIN
        UPDATE dbo.EventStaff
        SET StaffRole = @StaffRole,
            HoursWorked = @HoursWorked
        WHERE EventID = @EventID
          AND EmployeeID = @EmployeeID;
    END
    ELSE
    BEGIN
        INSERT INTO dbo.EventStaff (EventID, EmployeeID, StaffRole, HoursWorked)
        VALUES (@EventID, @EmployeeID, @StaffRole, @HoursWorked);
    END;
END;
GO
