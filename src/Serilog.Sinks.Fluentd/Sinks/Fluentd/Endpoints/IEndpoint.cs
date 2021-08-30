using System;
using System.IO;
using System.Threading.Tasks;

namespace AlpineComputence.Serilog.Sinks.Fluentd.Endpoints
{
    interface IEndpoint : IDisposable
    {
        Stream GetStream();
        Task ConnectAsync();
        bool IsConnected();
    }
}
