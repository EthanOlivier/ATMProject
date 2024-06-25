using ATMProject.Application;
using ATMProject.Application.Screens;
using ATMProject.System;

namespace ATMProject.WindowsConsoleApplication;
public class LoginScreen : IScreen
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;

    public LoginScreen(
        IUserRepository userRepository,
        IUserContextService userContextService,
        IScreenManager screenManager,
        IScreenGetter screenFactory
    )
    {
        _userRepository = userRepository;
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenFactory;
    }
    public void ShowScreen()
    {
        do
        {
            (string userId, string password) = GetCredentials();
            Result<UserContext> result = _userRepository.LoginUser(userId, password);

            if (result.Success)
            {
                _userContextService.SetUserContext(result.ResultData);

                if (_userContextService.GetUserContext().UserRole == Application.Users.UserRole.Basic)
                {
                    _screenManager.ShowScreen(ScreenNames.BasicOverview);
                }
                else if (_userContextService.GetUserContext().UserRole == Application.Users.UserRole.Admin)
                {
                    _screenManager.ShowScreen(ScreenNames.AdminOverview);
                }
                else
                {
                    throw new Exception("Incorrect UserRole given to User");
                }

                break;
            }
            else
            {
                Console.WriteLine($"Failed to login: {result.ErrorMessage}");
            }
        }
        while (true);
    }

    private (string, string) GetCredentials()
    {
        string? userId = null;
        string? password = null;
        do
        {
            Console.WriteLine("Please enter your User Id and Password");

            Console.WriteLine("Enter User ID");
            userId = Console.ReadLine() ?? "";

            Console.WriteLine("Enter Password");
            password = Console.ReadLine() ?? "";

        } while (!(userId != null && password != null));


        return (userId, password);
    }
}
