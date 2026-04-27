namespace OpenXRRuntimeSwitcher.Services.Abstractions;

public interface IRuntimeIconFactory
{
    Image GetIcon(string key);
}