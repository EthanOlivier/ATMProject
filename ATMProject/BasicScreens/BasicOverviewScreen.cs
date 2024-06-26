using ATMProject.Application;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;

namespace ATMProject.WindowsConsoleApplication.BasicScreens;
public class BasicOverviewScreen : IScreen
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;

    public BasicOverviewScreen(IUserRepository userRepository, IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenFactory)
    {
        _userRepository = userRepository;
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenFactory;
    }

    private record ViewModel
    (
        string Name,
        string Address,
        string PhoneNumber,
        string Email,
        string UserRole,
        string CreationDate,
        IEnumerable<ViewModel.Account> Accounts
    )
    {
        public record Account
        (
            string Id,
            string Type,
            double Balance,
            string CreationDate
        );
    }

    public void ShowScreen()
    {
        if (!_userContextService.IsLoggedIn ||
            _userContextService.GetUserContext().UserRole == UserRole.Admin
        )
        {
            _userContextService.Logout();
            _screenManager.ShowScreen(ScreenNames.Login);
        }

        var viewModel = BuildViewModel();
        RenderViewModel(viewModel);
        ReadInput(viewModel);
    }

    private ViewModel BuildViewModel()
    {
        var userInfo = _userRepository.GetUserInfoByUserId(_userContextService.GetUserContext().UserId);
        var accounts = _userRepository.GetUserAccountsByUserId(_userContextService.GetUserContext().UserId);

        return new ViewModel(
            Name: userInfo.Name,
            Address: userInfo.Address,
            PhoneNumber: userInfo.PhoneNumber,
            Email: userInfo.Email,
            UserRole: userInfo.UserRole.ToString(),
            CreationDate: userInfo.CreationDate.ToString(),
            Accounts: accounts
                .Where(account => account?.UserId == _userContextService.GetUserContext()?.UserId)
                .Select(account => new ViewModel.Account(
                    Id: account.AccountId,
                    Type: account.Type.ToString(),
                    Balance: account.Balance,
                    CreationDate: account.CreationDate.ToString()
            )
        ));
    }

    private void RenderViewModel(ViewModel viewModel)
    {
        Console.WriteLine($"\nName: {viewModel.Name}");
        Console.WriteLine($"Address: {viewModel.Address}");
        Console.WriteLine($"Phone Number: {viewModel.PhoneNumber}");
        Console.WriteLine($"Email: {viewModel.Email}");
        Console.WriteLine($"Role: {viewModel.UserRole}");
        Console.WriteLine($"Creation Time: {viewModel.CreationDate}");
        Console.WriteLine("\nAccounts: ");

        if (viewModel.Accounts.Any())
        {
            foreach (var account in viewModel.Accounts)
            {
                Console.WriteLine($"\nAccount Id: {account.Id}");
                Console.WriteLine($"Account Type: {account.Type}");
                Console.WriteLine($"Account Balance: ${account.Balance.ToString("N2")}");
                Console.WriteLine($"Account Time of Creation: {account.CreationDate}");
            }
        }
        else
        {
            Console.WriteLine("None");
        }

        Console.WriteLine("\n\nType 'D' to Deposit");
        if (viewModel.Accounts.Where(acct => acct.Balance > 0).Count() >= 1)
        {
            Console.WriteLine("Type 'W' to Withdraw");
        }
        if (viewModel.Accounts.Count() >= 2 && viewModel.Accounts.Where(acct => acct.Balance > 0).Count() >= 1)
        {
            Console.WriteLine("Type 'T' to Transfer");
        }
        Console.WriteLine("Type 'H' for Transaction History");
        Console.WriteLine("Type 'C' to Change Password");
        Console.WriteLine("Tpye 'L' to Logout");
    }

    private void ReadInput(ViewModel viewModel)
    {
        string input = Console.ReadLine() ?? "";
        while (true)
        {
            switch (input.ToUpper())
            {
                case "L":
                    _userContextService.Logout();
                    _screenManager.ShowScreen(ScreenNames.Login);
                    break;
                case "D":
                    ShowScreen(ScreenNames.Deposit);
                    break;
                case "W":
                    if (viewModel.Accounts.Where(acct => acct.Balance > 0).Count() >= 1)
                    {
                        ShowScreen(ScreenNames.Withdrawal);
                    }
                    break;
                case "T":
                    if (viewModel.Accounts.Count() >= 2 && viewModel.Accounts.Where(acct => acct.Balance > 0).Count() >= 1)
                    {
                        ShowScreen(ScreenNames.Transfer);
                    }
                    break;
                case "H":
                    ShowScreen(ScreenNames.History);
                    break;
                case "C":
                    ShowScreen(ScreenNames.ChangePassword);
                    break;
                default:
                    Console.WriteLine("Incorrect Screen Entered. Please Try Again");
                    break;
            }

            Console.WriteLine("Please Enter a Screen");
            input = Console.ReadLine() ?? "";
        }
    }
    private void ShowScreen(ScreenNames screen)
    {
        if (!_userContextService.IsLoggedIn ||
            _userContextService.GetUserContext().UserRole == UserRole.Admin
        )
        {
            _userContextService.Logout();
            _screenManager.ShowScreen(ScreenNames.Login);
        }
        else
        {
            _screenManager.ShowScreen(screen);
        }
    }
}
