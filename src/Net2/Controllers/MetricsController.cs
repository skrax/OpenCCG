using Celnet.Domain.Interfaces;
using Google.Protobuf;
using OpenCCG.Core;
using OpenCCG.Proto;
using Serilog;

namespace OpenCCG.Net2.Controllers;

public class MetricsController
{
    public MetricsController(IApiBuilder<IMessage, IMessage> builder)
    {
        builder.MapPut("metrics", OnUpdateMetrics);
        
        Log.Information(Logging.Templates.ServiceIsRunning, nameof(MetricsController));
    }
    
    private IMessage OnUpdateMetrics(IMessage arg)
    {
        if (arg is not Metrics metrics)
        {
            return new GenericResponse
            {
                StatusCode = StatusCode.BadRequest
            };
        }
        
        
        return new GenericResponse
        {
            StatusCode = StatusCode.Ok
        };
    }
}