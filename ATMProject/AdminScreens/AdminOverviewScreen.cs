using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;

namespace ATMProject.WindowsConsoleApplication.AdminScreens;
public class AdminOverviewScreen : IScreen
{
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly IGetUsersTotals _usersTotals;
    private readonly IGetAudits _getAudits;

    public AdminOverviewScreen(IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenGetter, IGetUsersTotals usersTotals, IGetAudits getAudits)
    {
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenGetter;
        _usersTotals = usersTotals;
        _getAudits = getAudits;
    }

    public void ShowScreen()
    {
        if (!_userContextService.IsLoggedIn ||
            _userContextService.GetUserContext().UserRole == UserRole.Basic
        )
        {
            _userContextService.Logout();
            _screenManager.ShowScreen(ScreenNames.Login);
        }

        RenderAdminOverviewScreen(CalculateDatabaseTotals(), _getAudits.GetAudits(_userContextService.GetUserContext().UserId));
    }

    private (int, int, double) CalculateDatabaseTotals()
    {
        int totalUsers = 0;
        int totalAccounts = 0;
        double totalBalance = 0;

        totalUsers = _usersTotals.GetTotalUsers();
        totalAccounts = _usersTotals.GetTotalAccounts();
        totalBalance = _usersTotals.GetTotalBalance();

        return (totalUsers, totalAccounts, totalBalance);
    }

    private void RenderAdminOverviewScreen((int, int, double) totals, List<string> audits)
    {
        Console.WriteLine("\nTotal Number of Users: " + totals.Item1);
        Console.WriteLine("Total Number of Accounts: " + totals.Item2);
        Console.WriteLine("Total Balance across all Accounts: " + totals.Item3.ToString("C2"));

        Console.WriteLine("\nAudits made:");
        if (audits.Count() != 0)
        {
            foreach (var audit in audits)
            {
                Console.WriteLine(audit);
            }
        }
        else
        {
            Console.WriteLine("None");
        }

        Console.WriteLine("\nType 'F' to Find User Id");
        Console.WriteLine("Type 'R' to Reset User Password");
        Console.WriteLine("Type 'AU' to Add another User");
        Console.WriteLine("Type 'DU' to Delete a current User");
        Console.WriteLine("Type 'AA' to Add another Account to a User");
        Console.WriteLine("Type 'DA' to Delete an Account from a current User");
        if (_userContextService.IsLoggedIn)
        {
            Console.WriteLine("Type 'C' to Change your Admin Password");
        }
        Console.WriteLine("Type 'L' to Logout");

        ReadInput();
    }

    private void ReadInput()
    {
        string? input = Console.ReadLine();
        while (input is null)
        {
            Console.WriteLine("Please Enter a Screen");
            input = Console.ReadLine();
        }
        switch (input.ToUpper())
        {
            case "F":
                _screenManager.ShowScreen(ScreenNames.LookupUser);
                break;
            case "R":
                _screenManager.ShowScreen(ScreenNames.ChangeUserPassword);
                break;
            case "AU":
                _screenManager.ShowScreen(ScreenNames.AddUser, "");
                break;
            case "DU":
                _screenManager.ShowScreen(ScreenNames.DeleteUser);
                break;
            case "AA":
                _screenManager.ShowScreen(ScreenNames.AddAccount, "");
                break;
            case "DA":
                _screenManager.ShowScreen(ScreenNames.DeleteAccount);
                break;
            case "C":
                if (_userContextService.IsLoggedIn)
                {
                    _screenManager.ShowScreen(ScreenNames.ChangePassword, "Admin");
                }
                else
                {
                    Console.WriteLine("Incorrect Screen Entered. Please Try Again.");
                    ShowScreen();
                }
                break;
            case "L":
                _userContextService.Logout();
                _screenManager.ShowScreen(ScreenNames.Login);
                break;
            default:
                Console.WriteLine("Incorrect Screen Entered. Please Try Again.");
                ShowScreen();
                break;
        }
    }
}
