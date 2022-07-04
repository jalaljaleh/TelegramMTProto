using TelegramMTProto;


var ping = Environment.GetEnvironmentVariable("ping");
if (ping != null) Ping(ping);

while (true)
{
    try
    {
        Console.WriteLine("Starting ..");

        var secret = Environment.GetEnvironmentVariable("secret") ?? throw new Exception("Secret can't be null.");
        var port = int.Parse(Environment.GetEnvironmentVariable("$PORT") ?? Environment.GetEnvironmentVariable("PORT") ?? Environment.GetEnvironmentVariable("port") ?? throw new Exception("Port can't be null."));
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
    Task.Delay(5000).GetAwaiter().GetResult();
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
                try
                {
                    Console.WriteLine("Ping {0}", host);
                    await client.GetStringAsync(host);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                await Task.Delay(timespan);

            }
        }
    });
    return Task.CompletedTask;
}