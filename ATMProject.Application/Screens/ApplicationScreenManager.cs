namespace ATMProject.Application.Screens;

public class ApplicationScreenManager : IScreenManager
{
    private readonly IScreenGetter _screenGetter;
    private readonly ILogger _logger;

    private IScreen _currentScreen;

    public ApplicationScreenManager(IScreenGetter screenGetter, ILogger logger)
    {
        _screenGetter = screenGetter;
        _logger = logger;
    }

    public void ShowScreen<T>(ScreenNames screen, T data) where T : class
    {
        _currentScreen = _screenGetter.GetScreen(screen);
        if (_currentScreen is IReceivableScreen receivableScreen)
        {
            receivableScreen.ReceiveData(data);
        }
        else
        {
            _logger.Log("WARNING: Attempting to send data to screen {screen} for a screen that is not a IReceivableScreen");
        }
        _currentScreen.ShowScreen();
    }

    public void ShowScreen(ScreenNames screen)
    {
        _currentScreen = _screenGetter.GetScreen(screen);
        _currentScreen.ShowScreen();
    }
}
