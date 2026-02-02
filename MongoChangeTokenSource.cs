using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Collections.Concurrent;

namespace MongoOptions
{
    public class MongoChangeTokenSource<TOptions> : IOptionsChangeTokenSource<TOptions>
    {
        // Tracks a separate signal for every name (e.g., "Default", "TenantA", "TenantB")
        private readonly ConcurrentDictionary<string, CancellationTokenSource> _signals = new();

        // Returning null or an empty string here tells the system 
        // this source can handle various names.
        public string? Name => null;

        public IChangeToken GetChangeToken()
        {
            // This is called by the Options system when it wants to "watch"
            // We usually return a token for the DefaultName here or a global one
            return GetTokenForName(Options.DefaultName);
        }

        public IChangeToken GetTokenForName(string name)
        {
            var cts = _signals.GetOrAdd(name, _ => new CancellationTokenSource());
            return new CancellationChangeToken(cts.Token);
        }

        public void OnMongoChanged(string name)
        {
            name ??= Options.DefaultName;
            if (_signals.TryRemove(name, out var cts))
            {
                cts.Cancel(); // This triggers the OnChanged for this specific name
                cts.Dispose();
            }
        }
    }
}