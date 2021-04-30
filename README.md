# VenturaSQL
The 3-tier SQL framework for C# projects.

Where an ORM binds to columns properties dynamically, VenturaSQL is static.

The VenturaSQL Studio WPF app generates recordset classes based on SQL statements or scripts. The recordsets are automatically injected into your C# projects.

Each generated recordset has column properties that belong to the resultset of an SQL statement. Change the SQL statement, and the column properties change too.

A recordset can retrieve and update a database via a built in Web API. It can also connect to a database directly.

# Blazor WebAssembly

VenturaSQL is perfect for **Blazor WebAssembly**. The recordsets in the browser have built in change tracking and only modified data is transmitted back to the server via Web API. 

# Web API

Only one single Web API controller with one POST method is needed.

```csharp
    public class VenturaSqlController : ControllerBase
    {
        [Route("api/venturasql")]
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] byte[] requestData) // HttpRequestMessage request
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

For the Blazor WebAssembly developer, it feels just like you connect to the database directly.

VenturaSQL is lightweight, does not use reflection and is very fast.

VenturaSQL has three parts:

+ The small NuGet package [VenturaSQL.NETStandard](https://www.nuget.org/packages/VenturaSQL.NETStandard) for the runtime
+ The tiny NuGet package [VenturaSQL.AspNetCore.Server](https://www.nuget.org/packages/VenturaSQL.AspNetCore.Server) to process incoming requests in the ASP.NET Core middle-tier
+ The VenturaSQLStudio WPF app that connects to your database and generates recordset source code for both client and server C# projects.

The runtime DLL is 95KB, and the middle-tier DLL id 13KB.

For each SQL statement (or script) your enter in VenturaSQL Studio a recordset class is generated.

Install VenturaSQL Studio by downloading the the installer. The installer comes with ready to run sample projects.

Online documentation: https://docs.sysdev.nl

The installer will be available early May. The online documentation is for version 3, and needs updating to version 4.
