using System;
using System.Collections.Generic;

namespace OpenXRRuntimeSwitcher.Models;

public sealed class Config
{
    public required Dictionary<string, string> Mappings { get; init; }
}
