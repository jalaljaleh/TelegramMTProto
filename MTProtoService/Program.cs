using TelegramMTProto;

var ping = Environment.GetEnvironmentVariable("ping");
if (ping != null) Ping(ping);

while (true)
{
    try
    {
        Console.WriteLine("Starting ..");

        var secret = Environment.GetEnvironmentVariable("secret") ?? throw new Exception("Secret can't be null.");
        var port = int.Parse(Environment.GetEnvironmentVariable("port") ?? throw new Exception("Port can't be null."));
        var ip = Environment.GetEnvironmentVariable("ip") ?? "default";

        TelegramMTProtoServer protoServer = new TelegramMTProtoServer(secret, port, ip);
        protoServer.Start();

        Console.WriteLine("Started");
        Console.ReadLine();
    }
    catch (Exception e)
    {
        Console.WriteLine(e);
    }
}


static Task Ping(string host)
{
    _ = Task.Run(async () =>
    {
        var timespan = TimeSpan.FromSeconds(60);
        using (HttpClient client = new HttpClient())
        {
            while (true)
            {
                Console.WriteLine("Ping {0}", host);
                await client.GetStringAsync(host);
                await Task.Delay(timespan);
            }
        }
    });
    return Task.CompletedTask;
}