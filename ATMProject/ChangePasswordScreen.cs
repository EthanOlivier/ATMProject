using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Operations.Authorization;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;
using ATMProject.System;

namespace ATMProject.WindowsConsoleApplication;
public class ChangePasswordScreen : IReceivableScreen
{
    private UserRole UserRole;

    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly ILogger _logger;
    private readonly IOperation<IChangeUserPasswordOperation.Request, IResult> _changePasswordOperation;

    public ChangePasswordScreen(IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenFactory, ILogger logger, IChangeUserPasswordOperation modifyChangePasswordData)
    {
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenFactory;
        _logger = logger;

        _changePasswordOperation = modifyChangePasswordData;
        _changePasswordOperation = new LoggingOperationDecorator<IChangeUserPasswordOperation.Request, IResult>(_changePasswordOperation, _userContextService, _logger);
        _changePasswordOperation = new AuthorizationOperationDecorator<IChangeUserPasswordOperation.Request, IResult>(_changePasswordOperation, _userContextService);
    }

    public void ReceiveData<T>(T data) where T : class
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
        string newPassword = EnterNewPasswordScreen();

        _changePasswordOperation.Execute(new IChangeUserPasswordOperation.Request(_userContextService.GetUserContext(), newPassword));

        _userContextService.Logout();
        _screenManager.ShowScreen(ScreenNames.Login);
    }
    private string EnterNewPasswordScreen()
    {
        string newPassword;
        string confirm;
        Console.WriteLine("Please enter your new password or type 'X' to leave the screen");
        newPassword = Console.ReadLine()!;

        newPassword = newPassword == String.Empty ? "password" : newPassword;
        if (newPassword.ToUpper() == "X")
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
