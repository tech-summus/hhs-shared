using Newtonsoft.Json;

namespace Hhs.Shared.Hosting.Microservices.Models;

public interface IRequestResponseLogModelCreator
{
    RequestResponseLogModel LogModel { get; }
    string LogString();
}

public sealed class RequestResponseLogModelCreator : IRequestResponseLogModelCreator
{
    public RequestResponseLogModel LogModel { get; private set; }

    public RequestResponseLogModelCreator()
    {
        LogModel = new RequestResponseLogModel();
    }

    public string LogString()
    {
        var jsonString = JsonConvert.SerializeObject(LogModel);
        return jsonString;
    }
}