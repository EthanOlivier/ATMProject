using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;
using ATMProject.Banking;

namespace ATMProject.WindowsConsoleApplication.AdminScreens;
public class AddAccountScreen : IReceivableScreen
{
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly IUserRepository _userRepository;
    private readonly IFindUser _findUser;
    private readonly IAddAccount _addAccount;
    private string UserId;
    public AddAccountScreen(IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenGetter, IUserRepository userRepository, IFindUser findUser, IAddAccount addAccount)
    {
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenGetter;
        _userRepository = userRepository;
        _findUser = findUser;
        _addAccount = addAccount;
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


    public void ReceiveData<T>(T data) where T : class
    {
        if (data is not null)
        {
            if (data.ToString() != "")
            {
                UserId = data.ToString();
            }
            else
            {
                UserId = null;
            }
        }
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

        bool wasSupplied;
        if (UserId is null)
        {
            EnterUserId();
            wasSupplied = false;
        }
        else
        {
            wasSupplied = true;
        }
        ViewModel viewModel = BuildViewModel();
        DisplayUserInfoFromUserId(viewModel);

        EnterAccountInfo();

        if (wasSupplied)
        {
            UserId = null;
            _screenManager.ShowScreen(ScreenNames.AddUser, UserId);
        }
        else
        {
            _screenManager.ShowScreen(ScreenNames.AdminOverview);
        }
    }
    private void EnterUserId()
    {
        Console.WriteLine("Enter the User Id for the user whom you would like to add accounts to");
        string userId = Console.ReadLine() ?? "";

        if (!_findUser.DoesUserExist(userId))
        {
            Console.WriteLine("Id not found. Please try again");
            _screenManager.ShowScreen(ScreenNames.AddAccount);
        }
        else
        {
            UserId = userId;
        }
    }
    private ViewModel BuildViewModel()
    {
        var userInfo = _userRepository.GetUserInfoByUserId(UserId);
        IEnumerable<AccountModel> accountData;
        if (userInfo.AccountIds.Count() > 0)
        {
            accountData = _userRepository.GetUserAccountsByUserId(UserId);
        }
        else
        {
            accountData = null;
        }

        return new ViewModel(
            Name: userInfo.Name,
            Address: userInfo.Address,
            PhoneNumber: userInfo.PhoneNumber,
            Email: userInfo.Email,
            UserRole: userInfo.UserRole.ToString(),
            CreationDate: userInfo.CreationDate,
            Accounts: accountData?
                .Where(account => account?.UserId == UserId)
                .Select(account => new ViewModel.Account(
                    Id: account.AccountId,
                    Type: account.Type.ToString(),
                    Balance: account.Balance.ToString(),
                    CreationDate: account.CreationDate
                )
            ));
    }
    private void DisplayUserInfoFromUserId(ViewModel viewModel)
    {
        Console.WriteLine($"\nName: {viewModel.Name}");
        Console.WriteLine($"Address: {viewModel.Address}");
        Console.WriteLine($"Phone Number: {viewModel.PhoneNumber}");
        Console.WriteLine($"Email: {viewModel.Email}");
        Console.WriteLine($"Role: {viewModel.UserRole}");
        Console.WriteLine($"Creation Time: {viewModel.CreationDate}");
        Console.WriteLine("\nAccounts: ");

        if (viewModel.Accounts is not null)
        {
            foreach (var account in viewModel.Accounts)
            {
                Console.WriteLine($"Account Id: {account.Id}");
                Console.WriteLine($"Account Type: {account.Type}");
                Console.WriteLine($"Account Balance: ${account.Balance}");
                Console.WriteLine($"Account Time of Creation: {account.CreationDate}\n");
            }
        }
        else
        {
            Console.WriteLine("None");
        }
    }
    private void EnterAccountInfo()
    {
        AccountType accountType;
        double balance;
        Console.WriteLine("\nWhat type of account would you like to make this? Type 'C' for Checking, Type 'S' for Savings, Type 'MMA' for MMA, Type 'CD' for CD");
        string type = Console.ReadLine() ?? "";
        while (type.ToUpper() != "C" && type.ToUpper() != "S" && type.ToUpper() != "MMA" && type.ToUpper() != "CD")
        {
            Console.WriteLine("Please enter a correct accout type");
            type = Console.ReadLine() ?? "";
        }
        switch (type.ToUpper())
        {
            case "C":
                accountType = AccountType.Checking;
                break;
            case "S":
                accountType = AccountType.Savings;
                break;
            case "MMA":
                accountType = AccountType.MMA;
                break;
            case "CD":
                accountType = AccountType.CD;
                break;
            default:
                throw new Exception("Incorrect Account Type Entered");
        }

        Console.WriteLine("What will the balance of this new account be?");
        balance = Convert.ToDouble(Console.ReadLine() ?? "0");

        _addAccount.AddAccount(UserId, accountType, balance, _userContextService.GetUserContext().UserId);
    }
}
