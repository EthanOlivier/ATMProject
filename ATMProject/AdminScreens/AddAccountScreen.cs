﻿using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Operations.Authorization;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;
using ATMProject.Banking;
using ATMProject.System;

namespace ATMProject.WindowsConsoleApplication.AdminScreens;
public class AddAccountScreen : IReceivableScreen
{
    private string UserId;

    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly IUserRepository _userRepository;
    private readonly ILogger _logger;
    private readonly IFindUser _findUser;
    private readonly IOperation<IAddAccount.Request, IResult> _addAccountOperation;

    public AddAccountScreen
    (
        IUserContextService userContextService, 
        IScreenManager screenManager, 
        IScreenGetter screenGetter, 
        IUserRepository userRepository, 
        ILogger logger, 
        IFindUser findUser, 
        IAddAccount addAccount
    )
    {
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenGetter;
        _userRepository = userRepository;
        _logger = logger;
        _findUser = findUser;

        _addAccountOperation = addAccount;
        _addAccountOperation = new LoggingOperationDecorator<IAddAccount.Request, 
            IResult>(_addAccountOperation, _userContextService, _logger);
        _addAccountOperation = new AuthorizationOperationDecorator<IAddAccount.Request, 
            IResult>(_addAccountOperation, _userContextService);
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
            string Balance,
            string CreationDate
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

        (AccountType accountType, double balance) = EnterAccountInfo();

        _addAccountOperation.Execute(new IAddAccount.Request(
            UserId, accountType, balance, _userContextService.GetUserContext().UserId
        ));

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
        while (UserId is null)
        {
            Console.WriteLine("Enter the User Id for the user whom you would like to add accounts to or type 'X' to leave the screen");
            string userId = Console.ReadLine() ?? "";

            if (!_findUser.DoesUserExist(userId))
            {
                if (userId.ToUpper() == "X")
                {
                    _screenManager.ShowScreen(ScreenNames.AdminOverview);
                }
                else
                {
                    Console.WriteLine("Id not found. Please try again");
                }
            }
            else
            {
                UserId = userId;
            }
        }
    }
    private ViewModel BuildViewModel()
    {
        var userInfo = _userRepository.GetUserInfoByUserId(UserId);
        var accountData = _userRepository.GetUserAccountsByUserId(UserId);

        return new ViewModel(
            Name: userInfo.Name,
            Address: userInfo.Address,
            PhoneNumber: userInfo.PhoneNumber,
            Email: userInfo.Email,
            UserRole: userInfo.UserRole.ToString(),
            CreationDate: userInfo.CreationDate.ToString(),
            Accounts: accountData?
                .Where(account => account?.UserId == UserId)
                .Select(account => new ViewModel.Account(
                    Id: account.AccountId,
                    Type: account.Type.ToString(),
                    Balance: account.Balance.ToString(),
                    CreationDate: account.CreationDate.ToString()
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
            Console.WriteLine(Environment.NewLine);
        }
        else
        {
            Console.WriteLine("None");
        }
    }
    private (AccountType, double) EnterAccountInfo()
    {
        AccountType accountType;
        string strBalance = "";
        double dblBalance = 0.0;
        string type = "";

        while (type.ToUpper() != "C" && 
            type.ToUpper() != "S" && 
            type.ToUpper() != "MMA" && 
            type.ToUpper() != "CD" && 
            type.ToUpper() != "X"
        )
        {
            if (type != "")
            {
                Console.WriteLine("Please enter a correct accout type");
            }
            Console.WriteLine("\nWhat type of account would you like to make this? Type 'C' for Checking, Type 'S' for Savings, Type 'MMA' for MMA, Type 'CD' for CD or type 'X' to leave the screen");
            type = Console.ReadLine() ?? "";
        }

        if (type.ToUpper() == "X")
        {
            _screenManager.ShowScreen(ScreenNames.AdminOverview);
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
                throw new Exception("Error: Account Type Entered could not be recognized as an Account Type");
        }

        bool isBalanceDouble = false;
        while (!isBalanceDouble)
        {
            Console.WriteLine("Enter the balance of this new account or type 'X' to leave the screen");
            strBalance = Console.ReadLine() ?? "0";

            if (strBalance.ToUpper() == "X")
            {
                _screenManager.ShowScreen(ScreenNames.AdminOverview);
            }
            else if (Double.TryParse(strBalance, out dblBalance))
            {
                if (dblBalance < 0)
                {
                    Console.WriteLine("Balance must be a number greater than 0. Please Try Again");
                }
                else
                {
                    isBalanceDouble = true;
                }
            }
            else
            {
                Console.WriteLine("Balance must be a number. Please Try Again");
            }
        }

        return (accountType, dblBalance);
    }
}
