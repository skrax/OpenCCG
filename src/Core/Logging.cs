using System.Linq;
using Godot;
using OpenCCG.Core.Serilog;
using Serilog;
using Serilog.Debugging;
using Serilog.Sinks.Grafana.Loki;

namespace OpenCCG.Core;

public partial class Logging : Node
{
    public override void _EnterTree()
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                              .Enrich.FromLogContext()
                                              .WriteTo.Godot()
                                              .WriteTo.GrafanaLoki("http://localhost:3100", new LokiLabel[]
                                                  {
                                                      new LokiLabel()
                                                      {
                                                          Key = "Godot", Value = "Godot Messages"
                                                      }
                                                  },
                                                  propertiesAsLabels: SessionContext.LogPropertyNames()
                                                      .Concat(new[] { "MessageId" }))
                                              .CreateLogger();

        SelfLog.Enable(GD.Print);
    }

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            Log.CloseAndFlush();
        }
    }

    public override void _ExitTree()
    {
        Log.CloseAndFlush();
    }

    public static class Templates
    {
        public const string ServiceIsRunning = "{Service} is running";

    }
}