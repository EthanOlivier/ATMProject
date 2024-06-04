using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;

namespace ATMProject.WindowsConsoleApplication.AdminScreens;
public class DeleteUserScreen : IScreen
{
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly IUserRepository _userRepository;
    private readonly IDeleteUser _deleteUserOperations;
    private readonly IFindUser _findUser;
    public DeleteUserScreen(IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenGetter, IUserRepository userRepository, IDeleteUser deleteUser, IFindUser findUser)
    {
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenGetter;
        _userRepository = userRepository;
        _deleteUserOperations = deleteUser;
        _findUser = findUser;
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
        DeleteUser(viewModel, userId);

        _screenManager.ShowScreen(ScreenNames.AdminOverview);
    }
    private string SelectUser()
    {
        Console.WriteLine("Enter the User Id for the user you want to delete");
        string? userId = Console.ReadLine();
        while (userId is null)
        {
            Console.WriteLine("Please enter a User Id");
            userId = Console.ReadLine();
        }

        if (!_findUser.DoesUserExist(userId))
        {
            Console.WriteLine("Id not found. Please try again.");
            _screenManager.ShowScreen(ScreenNames.DeleteUser);
        }
        return userId;
    }
    private ViewModel BuildViewModel(string userId)
    {
        var userInfo = _userRepository.GetUserInfoByUserId(userId);
        var accounts = _userRepository.GetUserAccountsByUserId(userId);

        return new ViewModel(
            Name: userInfo.Name,
            Address: userInfo.Address,
            PhoneNumber: userInfo.PhoneNumber,
            Email: userInfo.Email,
            UserRole: userInfo.UserRole.ToString(),
            CreationDate: userInfo.CreationDate,
            Accounts: accounts
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
    private void DeleteUser(ViewModel viewModel, string userId)
    {
        double totalBalance = 0;

        foreach (var account in viewModel.Accounts)
        {
            totalBalance += Convert.ToDouble(account.Balance);
        }

        if (totalBalance == 0)
        {
            Console.WriteLine("Are you sure you want to delete this user?\nType Y for yes, Type N for No");
            string confirm = Console.ReadLine() ?? "";
            if (confirm.ToUpper() == "Y")
            {
                _deleteUserOperations.DeleteUser(userId, _userContextService.GetUserContext().UserId);
                Console.WriteLine("User Deleted Successfully");
            }
            else
            {
                Console.WriteLine("User Removal Canceled");
            }
        }
        else
        {
            Console.WriteLine($"\nUnable to delete user: User's accounts still contain a cumulative balance of {totalBalance.ToString("C2")}. Must withdrawal all money from accounts before continuing");
        }
    }
}
