#pragma warning disable WFO5001
namespace OpenXRRuntimeSwitcher.Services.UI
{
    public interface IColorModeSetter
    {
        void Set(SystemColorMode mode);
    }

    public interface IDarkModeProvider
    {
        bool IsDarkMode();
    }

    public sealed class ColorModeProvider : IColorModeSetter, IDarkModeProvider
    {
        public void Set(SystemColorMode mode)
        {
            Application.SetColorMode(mode);
        }

        public bool IsDarkMode()
        {
            return Application.IsDarkModeEnabled;
        }
    }
}
