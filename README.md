# VenturaSQL
The 3-tier SQL framework for C# projects.

The only requirement for using VenturaSQL is that the client must be able to run C# code. A browser with Blazor is supported. A browser only running JavaScript is not supported.

Where an ORM binds to columns properties dynamically, VenturaSQL is static and the mapping is already done at compile time. With a runtime DLL of less than 100KB you can use both an ORM and VenturaSQL in the same project. Both approaches have their unique benefits. Use VenturaSQL when:
+ Work with SQL statements that you enter in the VenturaSQL Studio editor
+ Access a database over Http and Web API without having to write controller code
+ Optimized for raw performance thanks to binary data transfer and static binding instead of reflection

The VenturaSQL Studio WPF app generates recordset classes based on SQL statements or scripts. The recordsets are automatically injected into your C# projects.

Each generated recordset has column properties that belong to the resultset of an SQL statement. Change the SQL statement, and the column properties change too.

A recordset can retrieve and update a database via the built in Web API. Data is transmitted in binary format. In desktop and server applications, a recordset can also connect to a database directly.

![Image of recordset editor](https://raw.githubusercontent.com/frankthvandeven/VenturaSQL/master/README_IMG1.png)

## Blazor WebAssembly
VenturaSQL is perfect for **Blazor WebAssembly**. The VenturaSQL C# recordsets running in the browser have built in change tracking and only modified data is transmitted back to the server via Web API calls.

For the Blazor WebAssembly developer, it feels just like you connect to the database directly.

[Here is a demo](https://blazordemo.com) of a Blazor WebAssembly app using VenturaSQL for data access.

## Web API
A single Web API controller with a POST method needs to be added to the ASP.NET Core project.

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
            return ServerConnector.BikeStores;
        }
```

## The VenturaSQL system
VenturaSQL has three parts:

+ The small NuGet package [VenturaSQL.NETStandard](https://www.nuget.org/packages/VenturaSQL.NETStandard) for the runtime
+ The tiny NuGet package [VenturaSQL.AspNetCore.Server](https://www.nuget.org/packages/VenturaSQL.AspNetCore.Server) to process incoming requests in the ASP.NET Core middle-tier
+ The VenturaSQLStudio WPF app that connects to your database and generates recordset source code for both client and server C# projects.

The runtime DLL is 95KB, and the middle-tier DLL is 13KB. VenturaSQL is lightweight, does not use reflection and is very fast.

## Installing VenturaSQL Studio
Download and run the VenturaSQL Studio installer. The installer comes with ready to run sample projects. [Open the downloads page](https://site.sysdev.nl/venturasql)

**or**

Download this GitHub repository and open the solution and run the 'VenturaSQLStudio' project. The GitHub repository does not include the sample projects.

## Documentation
Online documentation: https://docs.sysdev.nl

