using TelegramMTProto;

Console.WriteLine("Starting ..");

var secret = Environment.GetEnvironmentVariable("secret") ?? throw new Exception("Secret can't be null.");
var port = int.Parse(Environment.GetEnvironmentVariable("port") ?? throw new Exception("Port can't be null."));
var ip = Environment.GetEnvironmentVariable("ip") ?? "default";

TelegramMTProtoServer protoServer = new TelegramMTProtoServer(secret, port, ip);
protoServer.Start();

Console.WriteLine("Started");
Console.ReadLine();