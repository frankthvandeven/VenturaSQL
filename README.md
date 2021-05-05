# VenturaSQL
The 3-tier SQL framework for C# projects.

Where an ORM binds to columns properties dynamically, VenturaSQL is static and the mapping is already done at compile time. With a runtime DLL of less than 100KB you can use both an ORM and VenturaSQL in the same project. Both approaches have their unique benefits. Use VenturaSQL when:

+ Raw performance is needed. VenturaSQL uses optimized binary data transfer and static binding instead of reflection. When saving changes, only modified column data is sent to the server.
+ Entering SQL statements (or complete scripts) in the VenturaSQL Studio editor is an acceptable starting point for implementing your data access API.
+ You don't want to spend development time writing Web API controller code.

The only requirement for using VenturaSQL is that the client must be able to run C# code. A browser running C# code with Blazor WebAssembly is supported, but a browser only running JavaScript is not supported.

## VenturaSQL Studio generates recordsets
The VenturaSQL Studio WPF app generates recordset classes based on SQL statements. The recordsets are automatically injected into your C# projects.

Each generated recordset has column properties based on the resultset of an SQL statement. Change the SQL statement, and the column properties change too.

A recordset retrieves data and updates the database via a single static Web API Controller. There is no need to write controller code. Data is transmitted in binary format. In desktop and server applications, a recordset can also connect to a database directly.

![Image of recordset editor](https://raw.githubusercontent.com/frankthvandeven/VenturaSQL/master/README_IMG1.png)

## Blazor WebAssembly
VenturaSQL is perfect for **Blazor WebAssembly**. The VenturaSQL C# recordsets running in the browser are optimized for speed and have built in change tracking. Only modified data is transmitted back to the server via Web API calls.

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
## The client

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
Download and run the VenturaSQL Studio installer. The installer comes with ready to run template projects. [Open the downloads page](https://dotnet.sysdev.nl/venturasql)

**or**

Download this GitHub repository and open the solution and run the 'VenturaSQLStudio' project. The GitHub repository does not include the template projects.

## Documentation
Online documentation: https://docs.sysdev.nl

## Advanced Features
+ A recordset can hold multiple resultsets.
+ Updating multiple tables using multiple recordsets can easily be bundled into a single database transaction (for rollback) using the Transactional.SaveChanges() method.
+ Calculated columns.
+ Column properties generate data binding events.
+ VenturaSQL Studio automatically generates basic recordsets with "SELECT * WHERE <prikey>" statements.
