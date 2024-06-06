using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;

namespace ATMProject.WindowsConsoleApplication;
public class ChangePasswordScreen : IScreen
{
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly IChangeUserPasswordOperation _changePasswordOp;
    private UserRole UserRole;

    public ChangePasswordScreen(IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenFactory, IChangeUserPasswordOperation modifyChangePasswordData)
    {
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenFactory;
        _changePasswordOp = modifyChangePasswordData;
    }

    public void Recieve<T>(T data) where T : class
    {
        if (data.ToString() == "Basic")
        {
            UserRole = UserRole.Basic;
        }
        else if (data.ToString() == "Admin")
        {
            UserRole = UserRole.Admin;
        }
    }
    public void ShowScreen()
    {
        if (!_userContextService.IsLoggedIn)
        {
            if (UserRole == UserRole.Basic)
            {
                _screenManager.ShowScreen(ScreenNames.BasicOverview);
            }
            else if (UserRole == UserRole.Admin)
            {
                _screenManager.ShowScreen(ScreenNames.AdminOverview);
            }
        }
        string newPassword = EnterNewPasswordScreen();

        _changePasswordOp.Execute(new IChangeUserPasswordOperation.Request(_userContextService.GetUserContext(), newPassword));

        _userContextService.Logout();
        _screenManager.ShowScreen(ScreenNames.Login);
    }
    private string EnterNewPasswordScreen()
    {
        string newPassword;
        string confirm;
        Console.WriteLine("Please enter your new password");
        newPassword = Console.ReadLine()!;
        newPassword = newPassword == String.Empty ? "password" : newPassword;
        Console.WriteLine($"Do you want to confirm change your password to [{newPassword}]?\nType Y for yes, Type N for No");
        confirm = Console.ReadLine() ?? "";
        if (confirm.ToUpper() != "Y")
        {
            Console.WriteLine("Password Change Canceled");
            if (UserRole == UserRole.Basic)
            {
                _screenManager.ShowScreen(ScreenNames.BasicOverview);
            }
            else if (UserRole == UserRole.Admin)
            {
                _screenManager.ShowScreen(ScreenNames.AdminOverview);
            }
            throw new Exception("Invalid User Role given when Cancelling Password Change");
        }
        else
        {
            return newPassword;
        }
    }
}
