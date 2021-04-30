# VenturaSQL
The 3-tier SQL framework for C# projects.

Where an ORM binds to columns properties dynamically, VenturaSQL is static. The generated classes with column properties are tightly linked to the resultset of an SQL statement (or script). Change the SQL statement, and the column properties change too.

VenturaSQL studio generates recordset classes based on an SQL statement. A recordset can retrieve and update database data via the built in Web API, or it can connect to a database directly.

VenturaSQL is perfect for Blazor WebAssembly. The recordsets in the browser have build in change tracking and only modified data is transmitted back to the server via Web API calls. For the Blazor WebAssembly developer, it is just like you connect to the database directly.

VenturaSQL is lightweight, does not use reflection and very fast.




VenturaSQL has three parts:

+ The small NuGet package [VenturaSQL.NETStandard](https://www.nuget.org/packages/VenturaSQL.NETStandard) for the runtime
+ The tiny NuGet package [VenturaSQL.AspNetCore.Server](https://www.nuget.org/packages/VenturaSQL.AspNetCore.Server) to process incoming requests in the ASP.NET Core middle-tier
+ The VenturaSQLStudio WPF app that connects to your database and generates recordset source code for both client and server C# projects.

The runtime DLL is 95KB, and the middle-tier DLL id 13KB.

For each SQL statement (or script) your enter in VenturaSQL Studio a recordset class is generated.

Install VenturaSQL Studio by downloading the the installer. The installer comes with ready to run sample projects.

Online documentation: https://docs.sysdev.nl

The installer will be available early May. The online documentation is for version 3, and needs updating to version 4.
