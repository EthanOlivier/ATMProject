﻿using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;
using ATMProject.System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace ATMProject.WindowsConsoleApplication.AdminScreens;
public class DeleteAccountScreen : IScreen
{
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly IUserRepository _userRepository;
    private readonly IFindUser _findUser;
    private readonly ILogger _logger;
    private readonly IOperation<IDeleteAccount.Request, IResult> _deleteAccountOperation;
    public DeleteAccountScreen(IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenGetter, IUserRepository userRepository, ILogger logger, IFindUser findUser, IDeleteAccount deleteAccount)
    {
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenGetter;
        _userRepository = userRepository;
        _logger = logger;
        _findUser = findUser;

        _deleteAccountOperation = deleteAccount;
        _deleteAccountOperation = new LoggingOperationDecorator<IDeleteAccount.Request, IResult>(_deleteAccountOperation, _userContextService, _logger);
        _deleteAccountOperation = new LoggingOperationDecorator<IDeleteAccount.Request, IResult>(_deleteAccountOperation, _userContextService, _logger);
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
        string accountId = SelectAccount(viewModel);
        ConfirmDeleteAccount(userId, accountId);

        _deleteAccountOperation.Execute(new IDeleteAccount.Request(accountId, _userContextService.GetUserContext().UserId));

        _screenManager.ShowScreen(ScreenNames.AdminOverview);
    }

    private string SelectUser()
    {
        Console.WriteLine("Enter the User Id for the user whose account you want to delete");
        string? userId = Console.ReadLine();
        while (userId is null)
        {
            Console.WriteLine("Please enter a User Id");
            userId = Console.ReadLine();
        }

        if (!_findUser.DoesUserExist(userId))
        {
            Console.WriteLine("Id not found. Please try again.");
            _screenManager.ShowScreen(ScreenNames.DeleteAccount);
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
                ))
            );
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
    private string SelectAccount(ViewModel viewModel)
    {
        bool doesAccountWithZeroBalanceExist = false;
        string accountId = "";
        Console.WriteLine();
        foreach (var account in viewModel.Accounts)
        {
            if (account.Balance == "0")
            {
                Console.WriteLine($"Type {account.Id} to delete account with that same Id");
                doesAccountWithZeroBalanceExist = true;
            }
        }
        if (!doesAccountWithZeroBalanceExist)
        {
            Console.WriteLine("Unable to find an account that is able to be deleted.");
            Console.WriteLine("Remember that an account must have a balance of $0 to be able to be deleted.");
            _screenManager.ShowScreen(ScreenNames.AdminOverview);
        }

        accountId = Console.ReadLine() ?? "";

        if (!viewModel.Accounts.Any(acct => acct.Id == accountId))
        {
            Console.WriteLine("Account Entered was not a valid account");
            _screenManager.ShowScreen(ScreenNames.DeleteAccount);
        }
        else if (!viewModel.Accounts.Any(acct => acct.Id == accountId && acct.Balance == "0"))
        {
            Console.WriteLine("Account Entered did not have a balance of zero.");
            _screenManager.ShowScreen(ScreenNames.DeleteAccount);
        }

        return accountId;
    }
    private void ConfirmDeleteAccount(string userId, string accountId)
    {
        Console.WriteLine("\nAre you sure you want to delete this account?\nType Y for yes, Type N for No");
        string confirm = Console.ReadLine() ?? "";
        if (confirm.ToUpper() != "Y")
        {
            Console.WriteLine("Account Removal Canceled");
            _screenManager.ShowScreen(ScreenNames.AdminOverview);
        }
    }
}
