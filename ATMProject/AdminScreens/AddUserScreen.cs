using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Operations.Authorization;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;
using ATMProject.Data.MockDatabase.MockDatabase;
using ATMProject.System;

namespace ATMProject.WindowsConsoleApplication.AdminScreens;
public class AddUserScreen : IReceivableScreen
{
    private string UserId;

    private readonly IUserContextService _userContextService;
    private readonly IUserRepository _userRepository;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly ILogger _logger;
    private readonly IGetUserIdentifyInfo _identityInfo;
    private readonly ICreateUserId _createUserId;
    private readonly IOperation<IAddUser.Request, IResult> _addUserOperation;
    public AddUserScreen(IUserContextService userContextService, IUserRepository userRepository, IScreenManager screenManager, IScreenGetter screenGetter, ILogger logger, IGetUserIdentifyInfo getUserIdentifyInfo, ICreateUserId createUserId, IAddUser addUser)
    {
        _userContextService = userContextService;
        _userRepository = userRepository;
        _screenManager = screenManager;
        _screenGetter = screenGetter;
        _identityInfo = getUserIdentifyInfo;
        _createUserId = createUserId;
        _logger = logger;

        _addUserOperation = addUser;
        _addUserOperation = new LoggingOperationDecorator<IAddUser.Request, IResult>(_addUserOperation, _userContextService, _logger);
        _addUserOperation = new AuthorizationOperationDecorator<IAddUser.Request, IResult>(_addUserOperation, _userContextService);
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

        (string, string, string, string, string, string) userInfo;
        string userId = "";
        if (UserId is null)
        {
            userInfo = EnterUserInfo();
        }
        else
        {
            userInfo = _identityInfo.GetUserIdentifyInfo(UserId);
        }
        DisplayUserInfo(userInfo.Item1, userInfo.Item2, userInfo.Item3, userInfo.Item4);

        if (UserId is null)
        {
            ConfirmEditCancel();

            UserId = _createUserId.CreateUserId();

            _addUserOperation.Execute(new IAddUser.Request(userInfo.Item1, userInfo.Item2, userInfo.Item3, userInfo.Item4, userInfo.Item5, userInfo.Item6, UserId, _userContextService.GetUserContext().UserId));

            _screenManager.ShowScreen(ScreenNames.AddAccount, UserId);
            UserId = null;
        }
        else
        {
            AddAccountsToUser(userId);
        }
        _screenManager.ShowScreen(ScreenNames.AdminOverview);
    }
    private (string, string, string, string, string, string) EnterUserInfo()
    {
        string password;
        string confirm = "";
        string name, address, phoneNumber, email;
        string salt = "", hash = "";
        do
        {
            Console.WriteLine("Enter a password for this new user or type 'X' to leave the screen");
            password = Console.ReadLine()!;
            if (password != "X")
            {
                password = password == String.Empty ? "password" : password;
                Console.WriteLine($"Do you want to confirm your password to be [{password}]?\nType Y for yes, Type N for No");
                confirm = Console.ReadLine() ?? "";
                if (confirm.ToUpper() == "Y")
                {
                    Console.WriteLine("Password Added");
                    salt = Guid.NewGuid().ToString();
                    hash = MockDatabaseUserRepository.CreateHash(salt, password);
                }
            }
            else
            {
                _screenManager.ShowScreen(ScreenNames.AdminOverview);
            }
        } while (confirm.ToUpper() != "Y");

        Console.WriteLine("Enter a Name for this new user.");
        name = Console.ReadLine() ?? "";
        Console.WriteLine("Enter an Address for this new user");
        address = Console.ReadLine() ?? "";
        Console.WriteLine("Enter a Phone Number for this new user");
        phoneNumber = Console.ReadLine() ?? "";
        Console.WriteLine("Enter an Email for this new user");
        email = Console.ReadLine() ?? "";

        return (name, address, phoneNumber, email, salt, hash);
    }

    private void DisplayUserInfo(string name, string address, string phoneNumber, string email)
    {
        IEnumerable<AccountModel> accountData;

        if (UserId is not null)
        {
            accountData = _userRepository.GetUserAccountsByUserId(UserId);
        }
        else
        {
            accountData = null;
        }

        Console.WriteLine("\nThe new users information will be: ");
        Console.WriteLine("Name: " + name);
        Console.WriteLine("Address: " + address);
        Console.WriteLine("Phone Number: " + phoneNumber);
        Console.WriteLine("Email: " + email);
        Console.WriteLine("Creation Date: " + DateTime.Now);
        Console.WriteLine("\nAccounts:");
        if (accountData is not null)
        {
            foreach (var account in accountData)
            {
                Console.WriteLine($"Account Id: {account.AccountId}");
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
    private void ConfirmEditCancel()
    {
        string confirm;
        Console.WriteLine("\nType 'C' to Confirm this information");
        Console.WriteLine("Type 'E' to Edit this information");
        Console.WriteLine("Type 'X' to Cancel and Close out of this screen");

        confirm = Console.ReadLine() ?? "";

        switch (confirm.ToUpper())
        {
            case "C":
                break;
            case "E":
                _screenManager.ShowScreen(ScreenNames.AddUser);
                break;
            default:
                UserId = null;
                _screenManager.ShowScreen(ScreenNames.AdminOverview);
                break;
        }
    }
    private void AddAccountsToUser(string userId)
    {
        ; Console.WriteLine("\nWould you like to add more accounts to this user?\nType Y for yes, Type N for No");
        string confirm = Console.ReadLine() ?? "";
        if (confirm.ToUpper() == "Y")
        {
            _screenManager.ShowScreen(ScreenNames.AddAccount, UserId);
        }
    }
}
