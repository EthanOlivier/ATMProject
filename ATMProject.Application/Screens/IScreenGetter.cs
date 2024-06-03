namespace ATMProject.Application.Screens
{
    public interface IScreenGetter
    {
        IScreen GetScreen(ScreenNames screenName);
    }
}
