using ATMProject.Application.Screens;

namespace ATMProject.Composition;

public record ServiceLocatorScreenGetterServiceConfiguration(
    Dictionary<ScreenNames, Type> ScreenTypeLookup
);

public class ServiceLocatorScreenGetter : IScreenGetter
{
    private Dictionary<ScreenNames, Type> _screenTypeLookup;
    private IServiceProvider _serviceProvider;

    public ServiceLocatorScreenGetter(
        ServiceLocatorScreenGetterServiceConfiguration serviceConfig,
        IServiceProvider serviceProvider
        )
    {
        _screenTypeLookup = serviceConfig.ScreenTypeLookup;
        _serviceProvider = serviceProvider;
    }

    public IScreen GetScreen(ScreenNames screenName)
    {
        if (!_screenTypeLookup.ContainsKey(screenName))
        {
            throw new Exception("Invalid screen");
        }
        var screenType = _screenTypeLookup[screenName];
        var screen = _serviceProvider.GetService(screenType) as IScreen
            ?? throw new Exception($"Screen Type `{screenType}` not found.");

        return screen;
    }
}
