using System.Collections.Generic;
using System.Drawing;

namespace OpenXRRuntimeSwitcher.Services;

public interface IRuntimeInfoProvider
{
    bool TryGetInfo(string key, out RuntimeInfo info);
    IReadOnlyDictionary<string, RuntimeInfo> GetAll();
}

public sealed record RuntimeInfo(string FriendlyName, Image Icon);