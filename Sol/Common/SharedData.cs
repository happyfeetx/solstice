#region USING DIRECTIVES

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.Concurrent;

#endregion USING DIRECTIVES

namespace Sol.Common
{
    public sealed class SharedData : IDisposable
    {
        public BotConfig Configuration { get; internal set; }
        public CancellationTokenSource MainLoopCts { get; internal set; }
        public ConcurrentDictionary<ulong, int> Messages { get; internal set; }

        public SharedData()
        {
            this.Configuration = BotConfig.Default;
            this.MainLoopCts = new CancellationTokenSource();
            this.Messages = new ConcurrentDictionary<ulong, int>();
        }

        public void Dispose()
        {
            this.MainLoopCts.Dispose();
        }
    }
}
