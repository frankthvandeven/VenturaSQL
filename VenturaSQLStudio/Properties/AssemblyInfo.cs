using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using VenturaSQL;

// General Information about an assembly is controlled through the following 
// set of attributes.
[assembly: AssemblyTitle("VenturaSQL Studio, development tool for Windows")]
[assembly: AssemblyDescription("VenturaSQL Studio, development tool for Windows")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCulture("")]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]

[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
                                     //(used if a resource is not found in the page, 
                                     // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
                                              //(used if a resource is not found in the page, 
                                              // app, or any theme specific resource dictionaries)
)]

[assembly: ReleaseDate(2021, 6, 1, 3, 8 , 51)]

// Version information for an assembly consists of the following four values: Major Version, Minor Version, Build Number, Revision
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below: [assembly: AssemblyVersion("1.0.*")]
//
// VenturaSQL uses version as: Major.Minor.Patch The Revision part always remains fixed at zero.
//
[assembly: AssemblyVersion("4.0.141.0")]
[assembly: AssemblyFileVersion("4.0.141.0")]
[assembly: AssemblyInformationalVersion("4.0.141")] // <-- This is the 'general' product version, for example: "1.0.1 or 1.0.1-rc or 1.1.2-beta4 or 2.0.0-alpha3"

// General Information about an assembly is controlled through the following set of attributes.
[assembly: AssemblyCompany("Frank Th. van de Ven")]
[assembly: AssemblyProduct("VenturaSQL™")]
[assembly: AssemblyCopyright("© 2013-2021 Frank Th. van de Ven")]
[assembly: AssemblyTrademark("VenturaSQL and VenturaSQL Studio are trademarks of Frank Th. van de Ven")]

// Setting ComVisible to false makes the types in this assembly not visible to COM components.
[assembly: ComVisible(false)]


[assembly: SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "<Pending>", Scope = "module")]
