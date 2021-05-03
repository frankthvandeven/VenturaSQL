# VenturaSQL
The 3-tier SQL framework for C# projects.

Where an ORM binds to columns properties dynamically, VenturaSQL is static.

The VenturaSQL Studio WPF app generates recordset classes based on SQL statements or scripts. The recordsets are automatically injected into your C# projects.

Each generated recordset has column properties that belong to the resultset of an SQL statement. Change the SQL statement, and the column properties change too.

A recordset can retrieve and update a database via a built in Web API. It can also connect to a database directly.

<img src="https://raw.githubusercontent.com/frankthvandeven/VenturaSQL/master/screenshot_rs_editor.png" width="50%" >
<img src="https://raw.githubusercontent.com/frankthvandeven/VenturaSQL/master/screenshot_rs_editor.png" width="1104" >




## Blazor WebAssembly
VenturaSQL is perfect for **Blazor WebAssembly**. The recordsets in the browser have built in change tracking and only modified data is transmitted back to the server via Web API. 

For the Blazor WebAssembly developer, it feels just like you connect to the database directly.

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
Download this GitHub repository and open the solution and run the 'VenturaSQLStudio' project.

**or**

Download and run the VenturaSQL Studio installer. The installer comes with ready to run sample projects. The installer will be available early May, and the download link will be here.

## Documentation
Online documentation: https://docs.sysdev.nl

The online documentation is for version 3, and needs updating to version 4. That will be done in May.

