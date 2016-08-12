# .Net Core - Minimal components
Minimal tests for .net core functionality. This is a tracer bullet project to check for any issues in porting my apps to .net core. 

##  [ ]  Database access
- [x] SQL Server
- [x] Postgresql
- [ ] SQLite
- [ ] Oracle

To run the code, a local instance of the various DBs is required, with a table name "Projects" containing at least two fields "Id" as Numeric and "Name" as VarChar or Text. The create scripts are provided as comments in the code. Change the connect strings as necessary. 

##  [x] Database functionality
- [x] Dapper
- [x] Blob / Clob

To run dapper successfully and get output, the "Projects" table must contain an item with the Id 1.
The blob tests require the Data field in the table.  

## [x] Redis

Checking basic Redis functionality and round trip. Needs a local redis instance. You can change the connect string. 

## [x] Json (Newtonsoft)

Checking basic round tripping on newtonsoft's json. 

## [ ] Windows Service
- [ ] Service code
- [ ] IsInteractive detection (Run from command line / Run as service) 
- [ ] Service installer
This is the next big part, most of my components are services that run on the full framework and use WCF. I use the IsInteractive to allow the Service to run in command line mode or in service mode.

## [ ] Communication
- [ ] WCF Server
- [ ] WCF Client
- [ ] Other communication frameworks (gRPC, ...) while I wait for WCF

## [x] IO
- [x] Base code location
- [x] Struct Binary IO (Fortran Interface)
- [x] Unsafe
- [x] Fixed
- [x] Marshal, Pin
- [x] Compression of byte streams

Testing the interaction with a Fortran calculation kernel that expects binary structs. Requires /unsafe. 

## [ ] Extensibility
- [x] Dynamic DLL Loading
- [ ] Workflow foundation

In the full framework, I load dlls that are stored as Blob in the Database and execute Workflow foundation workflows activities located in those libraries. 

DLL loading works, One issue remains with the current release version but should be fixed in the next release of .net core. 

## [x] Unit Testing
XUnit works. Can create and run tests - Usable in Visual studio testrunner

## [ ] X-plat GUI

Looking at Avalonia (https://github.com/AvaloniaUI/Avalonia), any other suggestions?
