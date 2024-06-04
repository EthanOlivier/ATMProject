using ATMProject.Application.Screens;

namespace ATMProject.Application;
public interface IScreenManager
{
    void ShowScreen<T>(ScreenNames screen, T data) where T : class;
    void ShowScreen(ScreenNames screen);
}
