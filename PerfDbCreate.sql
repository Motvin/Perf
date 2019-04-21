--USE [master]
--GO

Drop Database Perf
Go

/****** Object:  Database [Perf]    Script Date: 4/1/2019 11:10:34 PM ******/
CREATE DATABASE [Perf]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Perf', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\Perf.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'Perf_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\Perf_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO

ALTER DATABASE [Perf] SET COMPATIBILITY_LEVEL = 140
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Perf].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [Perf] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [Perf] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [Perf] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [Perf] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [Perf] SET ARITHABORT OFF 
GO

ALTER DATABASE [Perf] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [Perf] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [Perf] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [Perf] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [Perf] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [Perf] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [Perf] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [Perf] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [Perf] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [Perf] SET  DISABLE_BROKER 
GO

ALTER DATABASE [Perf] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [Perf] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [Perf] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [Perf] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [Perf] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [Perf] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [Perf] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [Perf] SET RECOVERY FULL 
GO

ALTER DATABASE [Perf] SET  MULTI_USER 
GO

ALTER DATABASE [Perf] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [Perf] SET DB_CHAINING OFF 
GO

ALTER DATABASE [Perf] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [Perf] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [Perf] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [Perf] SET QUERY_STORE = OFF
GO

ALTER DATABASE [Perf] SET  READ_WRITE 
GO

use Perf;
GO

CREATE TABLE [dbo].[BenchmarkMethod](
	BenchmarkMethodID int not null identity primary key,
	MethodName nvarchar(100) NOT NULL,
	MethodDescription nvarchar(1000) NOT NULL default(''),
	--Params nvarchar(4000) NOT NULL default(''), -- passed to the executable as cmd line arguments
	ParamsType nvarchar(100) NOT NULL,
	ExecutableFileName nvarchar(260),
	
	-- the following should be the first set of cmd line params along after the db connection string, RunID and BenchmarkMethodID, then Params after these
	-- DoJitRun bit not null default(1), -- 1 means there should be a jit run
	-- WarmupRunIterations int not null default(0), -- the # of warmup run iterations that should happen
	-- ActualRunIterations int not null default(0), -- the # of actual run iterations that should happen
	-- LoopUnrollCount int not null default(0), -- the # of Loop Unroll Counts that should be present
    -- CONSTRAINT [UQ_MethodName] UNIQUE NONCLUSTERED
    -- (
        -- MethodName, Params, ExecutableFileName, RunCount
    -- )
) ON [PRIMARY]-- TEXTIMAGE_ON [PRIMARY]
GO


CREATE TABLE [dbo].[Job](
	JobID int not null identity primary key,
	JobName [nchar](200) NOT NULL,
	JobDescription nvarchar(1000) NOT NULL default(''),
    CONSTRAINT [UQ_JobName] UNIQUE NONCLUSTERED
    (
        JobName
    )
) ON [PRIMARY]
GO

-- this is the benchmarkmethods to run for each job
CREATE TABLE [dbo].[JobBenchmark](
	JobBenchmarkID int not null identity primary key,
	JobID int not null FOREIGN KEY REFERENCES Job(JobID),
	BenchmarkMethodID int NOT NULL FOREIGN KEY REFERENCES BenchmarkMethod(BenchmarkMethodID),
	Params nvarchar(4000) NOT NULL default(''), -- passed to the executable as cmd line arguments
	RunCount int not null, -- the # of times the executable should be run
	Active bit NOT NULL default(1),
    -- CONSTRAINT [UQ_JobID_BenchmarkMethodID] UNIQUE NONCLUSTERED
    -- (
        -- JobID, BenchmarkMethodID
    -- )
) ON [PRIMARY]
GO

--JobID, BenchmarkMethodID is unique key
-- put params in the run instead of each benchmark method
CREATE TABLE [dbo].[Run](
	RunID int not null identity primary key,
	--MachineEnvironmentID int not null,
	--SoftwareEnvironmentID int not null,
	JobID int not null FOREIGN KEY REFERENCES Job(JobID),
	Params nvarchar(4000) NOT NULL default(''), -- passed to the executable as cmd line arguments	
	RunName [nchar](200) NOT NULL default(''), -- set this to the job's JobName - incase it changes we have a record of it
	RunDescription nvarchar(1000) NOT NULL default(''), -- set this to the job's JobDescription - incase it changes we have a record of it
	RunDescriptionExtra nvarchar(1000) NOT NULL default(''),
	
	MachineName nvarchar(100) NOT NULL, --Environment.MachineName;
	CPUDescription nvarchar(200) NOT NULL,
	OSVersion nvarchar(100) NOT NULL, -- Environment.OSVersion.ToString()
	IsOS64Bit bit NOT NULL, --Environment.Is64BitOperatingSystem;
	
	BenchmarkingProgram nvarchar(100) NOT NULL,
	BenchmarkingProgramVersion nvarchar(100) NOT NULL,
	ClrType nvarchar(100) NOT NULL,
	ClrVersion nvarchar(100) NOT NULL,
	
	ArePrograms64Bit bit NOT NULL, --Environment.Is64BitProcess for each benchmark method program - not sure how to do this
	
	Errors nvarchar(4000) NOT NULL default(''), -- errors in the runner program

	StartTime datetime NOT NULL,
	EndTime datetime NULL,
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Error](
	ErrorID  int not null identity primary key,
	RunID int NOT NULL FOREIGN KEY REFERENCES Run(RunID),
	BenchmarkMethodID int NOT NULL FOREIGN KEY REFERENCES BenchmarkMethod(BenchmarkMethodID),
	ErrorString nvarchar(max) NOT NULL,
) ON [PRIMARY]
GO

-- CREATE TABLE [dbo].[MachineEnvironment](
	-- MachineEnvironmentID int not null identity primary key,
	-- MachineName nvarchar(100) NOT NULL, --Environment.MachineName;
	-- CPUDescription nvarchar(200) NOT NULL,
	-- OSVersion nvarchar(100) NOT NULL, -- Environment.OSVersion.ToString()
-- )-- ON [PRIMARY]
-- GO

-- CREATE TABLE [dbo].[SoftwareEnvironment](
	-- SoftwareEnvironmentID int not null identity primary key,
	-- BenchmarkingProgram nvarchar(100) NOT NULL,
	-- BenchmarkingProgramVersion nvarchar(100) NOT NULL,
	-- ClrType nvarchar(100) NOT NULL,
	-- ClrVersion nvarchar(100) NOT NULL,
	-- --JitType [nchar](10) NULL
-- )-- ON [PRIMARY]
-- GO

CREATE TABLE [dbo].[Measurement](
	MeasurementID int not null identity primary key,
	RunID int NOT NULL FOREIGN KEY REFERENCES Run(RunID),
	BenchmarkMethodID int NOT NULL FOREIGN KEY REFERENCES BenchmarkMethod(BenchmarkMethodID),
	N bigint NOT NULL, -- should be 0 if there are no N params
	Iterations bigint NOT NULL,
	NanoSeconds float NOT NULL,
	StartTime datetime NOT NULL,
	EndTime datetime NOT NULL,
) ON [PRIMARY]
GO

CREATE TABLE [dbo].[Summary](
	SummaryID int not null identity primary key,
	RunID int NOT NULL FOREIGN KEY REFERENCES Run(RunID),
	BenchmarkMethodID int NOT NULL FOREIGN KEY REFERENCES BenchmarkMethod(BenchmarkMethodID),
	N bigint NOT NULL,
	TotalNanoSeconds float NOT NULL,
	TotalIterations bigint NOT NULL,
	Mean float NOT NULL,	
	Median float NOT NULL,
	Minimum float NOT NULL,
	Maximum float NOT NULL,
	Error float NULL,
	StandardDev float NULL,
	-- have others like Q1, Q3, etc.
) ON [PRIMARY]
GO


