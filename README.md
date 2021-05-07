# VenturaSQL
The 3-tier SQL framework for C# projects.

Where an ORM binds to columns properties dynamically, VenturaSQL is static and the mapping is already done at compile time. With a runtime DLL of less than 100KB you can use both an ORM and VenturaSQL in the same project. Both approaches have their unique benefits. Use VenturaSQL when:

+ Raw performance is needed. VenturaSQL uses optimized binary data transfer and static binding instead of reflection. When saving changes, only modified column data is sent to the server.
+ Entering SQL statements (or complete scripts) in the VenturaSQL Studio editor is an acceptable starting point for implementing your data access API.
+ You don't want to spend time writing Web API controller code.

Read [Making the case for VenturaSQL](https://sysdev.nl/making-the-case-for-venturasql/) article for a comparison between VenturaSQL and ORM. The article explains the difference between approaches, and what solution to choose depending on the scenario.

The only requirement for using VenturaSQL is that the client must be able to run C# code. A browser running C# code with Blazor WebAssembly is supported, but a browser only running JavaScript is not supported.

VenturaSQL can use any ADO.NET data provider, but currently the only tested providers are:
+ System.Data.SqlClient (Microsoft SQL Server)
+ System.Data.SQLite (SQLite.org)

## VenturaSQL Studio generates recordsets
The VenturaSQL Studio WPF app generates recordset classes based on SQL statements. The recordsets are automatically injected into your C# projects.

Each generated recordset has column properties based on the resultset of an SQL statement. Change the SQL statement, and the column properties change too.

A recordset retrieves data and updates the database via a single Web API Controller. You only need add controller code once. Data is transmitted in binary format. In desktop and server applications, a recordset can also connect to a database directly.

![Image of recordset editor](https://raw.githubusercontent.com/frankthvandeven/VenturaSQL/master/README_IMG1.png)

## Blazor WebAssembly
VenturaSQL is perfect for Blazor WebAssembly. The VenturaSQL C# recordsets running in the browser are optimized for speed and have built in change tracking. Only modified data is transmitted back to the server via Web API calls.

For the Blazor WebAssembly developer, it feels just like you are connected to the database directly.

[Here is a demo](https://blazordemo.com) of a Blazor WebAssembly app using VenturaSQL for data access.

## Web API
A single static Web API controller with a POST method needs to be added to the ASP.NET Core project.

```csharp
    [ApiController]
    public class VenturaSqlController : ControllerBase
    {
        [Route("api/venturasql")]
        [HttpPost]
        public async Task<IActionResult> Index(byte[] requestData)
        {
            var processor = new VenturaSqlServerEngine();
            processor.CallBacks.LookupAdoConnector = LookupAdoConnector;
            await processor.ExecAsync(requestData);
            Response.ContentType = "application/octet-stream";
            await Response.Body.WriteAsync(processor.ResponseBuffer, 0, processor.ResponseLength);
            return Ok();
        }

        private AdoConnector LookupAdoConnector(string requestedName)
        {
            return new AdoConnector(SqlClientFactory.Instance, "Server=tcp:xxx,1433;Initial Catalog=VanArsdel;User ID=yyy;Password=zzz;");
        }
```
## Client code
The client must be able to run C# code.

```csharp
VenturaSqlConfig.DefaultConnector = new HttpConnector("DefaultConnector", "api/venturasql");

PriKey_CountryCodes_Recordset rs = new PriKey_CountryCodes_Recordset();

rs.ExecSql("us");

if (rs.RecordCount != 0)
{
    MessageBox.Show($"County code 'us' already exists.");
    return;
}

rs.Append();
rs.CountryCodeID = "us";
rs.Name = "United States";

rs.SaveChanges();
```

## The VenturaSQL system
VenturaSQL has three parts:

+ The small NuGet package [VenturaSQL.NETStandard](https://www.nuget.org/packages/VenturaSQL.NETStandard) for the runtime
+ The tiny NuGet package [VenturaSQL.AspNetCore.Server](https://www.nuget.org/packages/VenturaSQL.AspNetCore.Server) to process incoming requests in the ASP.NET Core middle-tier
+ The VenturaSQLStudio WPF app that connects to your database and generates recordset source code for both client and server C# projects.

The runtime DLL is 95KB, and the middle-tier DLL is 13KB. VenturaSQL is lightweight, does not use reflection and is very fast.

## Installing VenturaSQL Studio
[Download](https://dotnet.sysdev.nl/venturasql) and run the VenturaSQL Studio installer. The installer comes with ready to run template projects.

**or**

Download this GitHub repository and open the solution and run the VenturaSQLStudio project. The GitHub repository does not include the template projects.

## See it run on your PC in minutes
The easiest way to get started with VenturaSQL is to run the installer, create a new project with the Blazor WebAssembly template and run it. Open the [Getting started with VenturaSQL](https://sysdev.nl/getting-started-with-venturasql/) article for illustrated instructions.

## Resources
+ Online documentation: https://docs.sysdev.nl
+ Frank's blog: https://sysdev.nl
+ VenturaSQL home page: https://sysdev.nl/info-venturasql/
+ Download the latest version: https://dotnet.sysdev.nl/venturasql

## Advanced Features
+ A recordset can hold multiple resultsets.
+ Updating multiple tables using multiple recordsets can easily be bundled into a single database transaction (for rollback) using the Transactional.SaveChanges() method.
+ Calculated columns.
+ Column properties generate data binding events (INotifyPropertyChanged and INotifyCollectionChanged).
+ VenturaSQL Studio automatically generates basic recordsets with "SELECT <all columns> WHERE <prikey>" statements.
+ VenturaSQL Studio has a code snippet generator, for example for filling viewmodels.

## Upcoming Feature: Data Sentry
The Data Sentry validates the data that is received from the client. This is a security layer to avoid tampering with data. The skeleton C# class for the Data Sentry is generated by VenturaSQL Studio. The Data Sentry is run by the Web API Controller for VenturaSQL's middle-tier.

The developer only needs to add validation code for the data received. When validation fails, simply throw an Exception. The request is then aborted and the error message is sent back to the client.

For example: User John is only allowed to update customer records that are assigned to him. When you detect that John tries to update a customer record that is not assigned to him, you throw an Exception and request is aborted all together.
