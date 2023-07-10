using Godot;
using OpenCCG.Core.Serilog;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace OpenCCG.Core;

public partial class Logging : Node
{
    public override void _EnterTree()
    {
        Log.Logger = new LoggerConfiguration().MinimumLevel.Debug()
                                              .WriteTo.Godot()
                                              .WriteTo.GrafanaLoki("http://localhost:3100")
                                              .CreateLogger();
    }

    public override void _ExitTree()
    {
        Log.CloseAndFlush();
    }
}