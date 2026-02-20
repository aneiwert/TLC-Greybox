using System;
using System.Threading;
using System.Threading.Tasks;

namespace Unity.AssetManager.UI.Editor
{
    interface ISidebarContentEnabler
    {
        event Action AllInvalidated;
        
        bool Enabled { get; set; }

        Task<bool> IsEntryEnabledAsync(string id, CancellationToken cancellationToken);
    }
}
