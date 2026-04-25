using System.Drawing;

namespace OpenXRRuntimeSwitcher.Services;

public interface IRuntimeIconFactory
{
    Image GetIcon(string key);
}