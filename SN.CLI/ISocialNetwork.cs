using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SN.CLI
{
    public interface ISocialNetwork
    {
        Task StartAsync(CancellationToken ct = default(CancellationToken));
    }
}
