using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Operations.Authorization;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;
using ATMProject.System;

namespace ATMProject.WindowsConsoleApplication.AdminScreens;
public class ChangeUserPasswordScreen : IScreen
{
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;
    private readonly IFindUser _findUser;
    private readonly IOperation<IChangeBasicUserPassword.Request, IResult> _changeBasicUserPasswordOperation;
    public ChangeUserPasswordScreen(IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenGetter, IUserRepository userRepository, ILogger logger, IFindUser findUser, IChangeBasicUserPassword changeBasicUserPassword)
    {
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenGetter;
        _userRepository = userRepository;
        _logger = logger;
        _findUser = findUser;

        _changeBasicUserPasswordOperation = changeBasicUserPassword;
        _changeBasicUserPasswordOperation = new LoggingOperationDecorator<IChangeBasicUserPassword.Request, IResult>(_changeBasicUserPasswordOperation, _userContextService, _logger);
        _changeBasicUserPasswordOperation = new AuthorizationOperationDecorator<IChangeBasicUserPassword.Request, IResult>(_changeBasicUserPasswordOperation, _userContextService);
    }

    private record ViewModel
    (
        string Name,
        string Address,
        string PhoneNumber,
        string Email,
        string UserRole,
        DateTime CreationDate,
        IEnumerable<ViewModel.Account> Accounts
    )
    {
        public record Account
        (
            string Id,
            string Type,
            string Balance,
            DateTime CreationDate
        );
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

        string userId = SelectUser();
        ViewModel viewModel = BuildViewModel(userId);
        DisplayUser(viewModel);
        string newPassword = ChangePassword(userId);

        _changeBasicUserPasswordOperation.Execute(new IChangeBasicUserPassword.Request(userId, newPassword, _userContextService.GetUserContext().UserId));

        _screenManager.ShowScreen(ScreenNames.AdminOverview);
    }
    private string SelectUser()
    {
        Console.WriteLine("Enter the User Id for the user whose password you want to change");
        string? userId = Console.ReadLine();
        while (userId is null)
        {
            Console.WriteLine("Please enter an Identity Field");
            userId = Console.ReadLine();
        }

        if (!_findUser.DoesUserExist(userId))
        {
            Console.WriteLine("Id not found. Please try again");
            _screenManager.ShowScreen(ScreenNames.ChangeUserPassword);
        }

        return userId;
    }
    private ViewModel BuildViewModel(string userId)
    {
        var userInfo = _userRepository.GetUserInfoByUserId(userId);
        var accountData = _userRepository.GetUserAccountsByUserId(userId);

        return new ViewModel(
            Name: userInfo.Name,
            Address: userInfo.Address,
            PhoneNumber: userInfo.PhoneNumber,
            Email: userInfo.Email,
            UserRole: userInfo.UserRole.ToString(),
            CreationDate: userInfo.CreationDate,
            Accounts: accountData
                .Where(account => account?.UserId == userId)
                .Select(account => new ViewModel.Account(
                    Id: account.AccountId,
                    Type: account.Type.ToString(),
                    Balance: account.Balance.ToString(),
                    CreationDate: account.CreationDate
                )
            ));
    }
    private void DisplayUser(ViewModel viewModel)
    {
        Console.WriteLine($"\nName: {viewModel.Name}");
        Console.WriteLine($"Address: {viewModel.Address}");
        Console.WriteLine($"Phone Number: {viewModel.PhoneNumber}");
        Console.WriteLine($"Email: {viewModel.Email}");
        Console.WriteLine($"Role: {viewModel.UserRole}");
        Console.WriteLine($"Creation Time: {viewModel.CreationDate}");
        Console.WriteLine("\nAccounts: ");

        if (viewModel.Accounts.Count() > 0)
        {
            foreach (var account in viewModel.Accounts)
            {
                Console.WriteLine($"\nAccount Id: {account.Id}");
                Console.WriteLine($"Account Type: {account.Type}");
                Console.WriteLine($"Account Balance: ${account.Balance}");
                Console.WriteLine($"Account Time of Creation: {account.CreationDate}");
            }
        }
        else
        {
            Console.WriteLine("None");
        }
    }


    private string ChangePassword(string userId)
    {
        Console.WriteLine("\nEnter their new password");
        string password = Console.ReadLine()!;
        password = password == String.Empty ? "password" : password;

        Console.WriteLine($"Do you want to confirm change the user's password to [{password}]?\nType Y for yes, Type N for No");
        string confirm = Console.ReadLine() ?? "";
        if (confirm.ToUpper() != "Y")
        {
            Console.WriteLine("Password Change Canceled");
            _screenManager.ShowScreen(ScreenNames.AdminOverview);
        }
        return password;
    }
}