namespace OpenXRRuntimeSwitcher.Services.UI
{
    public static class UIInitialization
    {
        public static void ApplySystemColorMode(IColorModeSetter setter)
        {
#pragma warning disable WFO5001
            setter.Set(SystemColorMode.System);
        }
    }
}