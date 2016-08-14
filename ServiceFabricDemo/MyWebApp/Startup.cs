using System.Web.Http;
using Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Microsoft.Owin;

namespace MyWebApp
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            var config = new HttpConfiguration();
            var physicalFileSystem = new PhysicalFileSystem(@".\wwwroot");
            var fileOptions = new FileServerOptions();

            fileOptions.EnableDefaultFiles = true;
            fileOptions.RequestPath = PathString.Empty;
            fileOptions.FileSystem = physicalFileSystem;
            fileOptions.DefaultFilesOptions.DefaultFileNames = new[] { "index.html" };
            fileOptions.StaticFileOptions.FileSystem = fileOptions.FileSystem = physicalFileSystem;
            fileOptions.StaticFileOptions.ServeUnknownFileTypes = true;
            fileOptions.EnableDirectoryBrowsing = true;

            config.MapHttpAttributeRoutes();

            appBuilder.UseWebApi(config);

            appBuilder.UseFileServer(fileOptions);
        }
    }
}
