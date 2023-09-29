using Celnet.Domain;
using Celnet.Infrastructure.ENet;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using OpenCCG.Gameserver;
using OpenCCG.Gameserver.Controllers;
using OpenCCG.Proto;

var host = Host.CreateDefaultBuilder(args)
               .ConfigureServices(services =>
               {
                   services.AddSingleton<ENetService>();
                   services.AddSingleton<Server>();
                   services.AddSingleton(Backend<IMessage, IMessage>.MakeConcurrent());
                   services.AddSingleton(TypeRegistry.FromFiles(ProtocolReflection.Descriptor));
                   services.AddSingleton<Transport>();
                   services.AddSingleton<CardsController>();
                   services.AddHostedService<Worker>();
               })
               .Build();

host.Run();