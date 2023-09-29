using Celnet.Domain;
using Google.Protobuf;
using OpenCCG.Proto;

namespace OpenCCG.Gameserver.Controllers;

public class CardsController
{
    public CardsController(Backend<IMessage, IMessage> backend, ILogger<CardsController> _logger)
    {
        backend.MapPut("cards/playById", PlayById);
    }

    private IMessage PlayById(IMessage arg)
    {
        if (arg is not ById byId)
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