Create Database DbTimeManagmentApp

CREATE TABLE Students (
    StudentId INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NOT NULL,
    PasswordHash NVARCHAR(128) NOT NULL -- Store hashed passwords securely
);

CREATE TABLE Modules (
    ModuleId INT PRIMARY KEY IDENTITY(1,1),
    ModuleCourse NVARCHAR(50) NOT NULL,
    ModuleCode NVARCHAR(50) NOT NULL,
    ModuleName NVARCHAR(100) NOT NULL,
    ModuleCredits INT NOT NULL,
    ClassHoursPerWeek INT NOT NULL,
    StudentId INT, 
    FOREIGN KEY (StudentId) REFERENCES Students(StudentId) 
);

CREATE TABLE CalcStudyHours (
    Weeks INT PRIMARY KEY IDENTITY,
    StartDate DateTime,
    RemainingStudyTime INT,
    
);

ALTER TABLE CalcStudyHours
ADD ModuleName NVARCHAR(50),
    RequiredStudyHoursPerWeek INT;
 

drop table CalcStudyHours

SELECT * FROM Students

SELECT * FROM Modules

