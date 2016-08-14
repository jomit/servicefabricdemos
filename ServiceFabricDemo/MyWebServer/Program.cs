// To test outside Service Fabric: 
// From Admin CMD: netsh http add    urlacl url=http://+:8080/ user=MicrosoftAccount\JeffRichter@live.com listen=yes
//                 netsh http delete urlacl url=http://+:8080/

using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

internal static class MyWebServer
{
    private const String c_br = "<br/>", c_nbsp = "&nbsp;";
    private static readonly string s_nodeId =
       Environment.GetEnvironmentVariable("HostedServiceName") ?? "Not running on Service Fabric";
    private static Int32 s_delay = 0;
#if V1
   private static Int32 s_version = 1;
#else
    private static Int32 s_version = 2;
#endif

    private static void Main(String[] args)
    {
        Int32 port = Int32.Parse(args[0]);
        HttpListener m_listener = new HttpListener();
        m_listener.Prefixes.Add($"http://+:{port}/");
        m_listener.Start();  // Requires Admin privileges

        while (true)
        {
            HttpListenerContext context = m_listener.GetContext();   // Wait for client request
            String requestUri = context.Request.Url.ToString();      // Sometimes we get requests for \favicon.ico, See http://en.wikipedia.org/wiki/Favicon
            if (requestUri.Contains("favicon.ico")) continue;
            ProcessRequestAsync(context).ContinueWith(t => t.Exception, TaskContinuationOptions.OnlyOnFaulted); // Swallow exception
        }
    }
    private static async Task ProcessRequestAsync(HttpListenerContext context)
    {
        String cmd = context.Request.QueryString["cmd"];
        if (cmd != null)
        {
            switch (cmd.ToLowerInvariant())
            {
                case "delay":
                    Int32.TryParse(context.Request.QueryString["delay"], out s_delay);
                    break;
                case "crash":
                    Environment.Exit(-1);// throw new InvalidOperationException("Forced crash");
                    break;
            }
        }
        await Task.Delay(TimeSpan.FromSeconds(s_delay)); // Insert artificial delay
        using (HttpListenerResponse response = context.Response)
        {
            String html =
               $"<H1>Node Id: {s_nodeId}<H1>" +
               $"<H2>Version={s_version}</H2>" +
               $"Delay={s_delay} seconds.";

            Byte[] responseData = Encoding.UTF8.GetBytes(html);
            context.Response.ContentLength64 = responseData.Length;
            using (var stream = context.Response.OutputStream)
            {
                await stream.WriteAsync(responseData, 0, responseData.Length);
            }
        }
    }
}
