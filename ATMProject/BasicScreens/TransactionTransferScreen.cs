using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Operations.Authorization;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;
using ATMProject.System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace ATMProject.WindowsConsoleApplication.BasicScreens;
public class TransactionTransferScreen : IScreen
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly ILogger _logger;
    private readonly IOperation<ITransferBetweenAccountsOperation.Request, IResult> _transferBetweenAccountsOperation;

    public TransactionTransferScreen(IUserRepository userRepository, IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenFactory, ILogger logger, ITransferBetweenAccountsOperation modifyTransferData)
    {
        _userRepository = userRepository;
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenFactory;
        _logger = logger;

        _transferBetweenAccountsOperation = modifyTransferData;
        _transferBetweenAccountsOperation = new LoggingOperationDecorator<ITransferBetweenAccountsOperation.Request, IResult>(_transferBetweenAccountsOperation, _userContextService, _logger);
        _transferBetweenAccountsOperation = new AuthorizationOperationDecorator<ITransferBetweenAccountsOperation.Request, IResult>(_transferBetweenAccountsOperation, _userContextService);
    }
    public record ViewModel
    (
        string Id,
        string Type,
        string Balance,
        string CreationDate
    );


    public void ShowScreen()
    {
        string depositAccount, withdrawalAccount;
        double amount, withdrawalAccountBalance;
        if (!_userContextService.IsLoggedIn ||
            _userContextService.GetUserContext().UserRole == UserRole.Admin
        )
        {
            _userContextService.Logout();
            _screenManager.ShowScreen(ScreenNames.Login);
        }

        (withdrawalAccount, depositAccount, withdrawalAccountBalance) = ChooseAccounts(GetData());
        amount = GetAmount(withdrawalAccountBalance);

        _transferBetweenAccountsOperation.Execute(new ITransferBetweenAccountsOperation.Request(withdrawalAccount, depositAccount, amount));
        
        _screenManager.ShowScreen(ScreenNames.BasicOverview);
    }
    private IEnumerable<ViewModel> GetData()
    {
        var accountData = _userRepository.GetUserAccountsByUserId(_userContextService.GetUserContext().UserId);

        return accountData.Select(accountData => new ViewModel(
            Id: accountData.AccountId,
            Type: accountData.Type.ToString(),
            Balance: accountData.Balance.ToString("N2"),
            CreationDate: accountData.CreationDate.ToString()
        ));
    }
    private (string, string, double) ChooseAccounts(IEnumerable<ViewModel> viewModel)
    {
        string withdrawalAccount = "", depositAccount = "";
        double withdrawalAccountBalance = 0.0;

        var withdrawalableAccounts = viewModel.Where(acct => Convert.ToDouble(acct.Balance) > 0);

        Console.WriteLine("Enter an Account or type 'X' to leave the screen\n");

        switch (withdrawalableAccounts.Count())
        {
            case 0:
                _logger.Log("Warning: Unable to find any usable accounts.");
                Console.WriteLine("Unable to use Screen due to a lack of accounts.");
                _screenManager.ShowScreen(ScreenNames.BasicOverview);
                break;
            case 1:
                withdrawalAccount = withdrawalableAccounts.FirstOrDefault()!.Id;
                withdrawalAccountBalance = Convert.ToDouble(withdrawalableAccounts.FirstOrDefault()!.Balance);
                if (withdrawalAccount is null || withdrawalAccountBalance == 0.0)
                {
                    throw new Exception("Error: Unable to find given account");
                }
                break;
            default:
                foreach (var account in withdrawalableAccounts)
                {
                    Console.WriteLine($"Type {account.Id} to transfer from Account with Type: {account.Type}, Balance: ${account.Balance}");
                }
                withdrawalAccount = Console.ReadLine() ?? "";

                if (withdrawalAccount.ToUpper() == "X")
                {
                    _screenManager.ShowScreen(ScreenNames.BasicOverview);
                }

                if (!viewModel.Any(acct => acct.Id == withdrawalAccount))
                {
                    Console.WriteLine("\nAccount Entered was not a valid account");
                    ShowScreen();
                }
                else
                {
                    withdrawalAccountBalance = Convert.ToDouble(withdrawalableAccounts.Where(acct => acct.Id == withdrawalAccount).FirstOrDefault()!.Balance);
                    if (withdrawalAccountBalance == 0.0)
                    {
                        throw new Exception("Error: Unable to find given account");
                    }
                }
                break;
        }
        


        switch (viewModel.Count())
        {
            case 0:
                _logger.Log("Warning: Unable to find any usable accounts.");
                Console.WriteLine("Unable to use Screen due to a lack of accounts.");
                _screenManager.ShowScreen(ScreenNames.BasicOverview);
                break;
            case 1:
                depositAccount = viewModel.FirstOrDefault(acct => acct.Id != depositAccount)!.Id;
                if (depositAccount is null)
                {
                    throw new Exception("Error: Unable to find given account");
                }
                break;
            default:
                foreach (var account in viewModel)
                {
                    if (account.Id != withdrawalAccount)
                    {
                        Console.WriteLine($"Type {account.Id} to transfer into Account with Type: {account.Type}, Balance: ${account.Balance}");
                    }
                }
                depositAccount = Console.ReadLine() ?? "";

                if (depositAccount.ToUpper() == "X")
                {
                    _screenManager.ShowScreen(ScreenNames.BasicOverview);
                }

                if (!viewModel.Any(acct => acct.Id == depositAccount) || depositAccount == withdrawalAccount)
                {
                    Console.WriteLine("\nAccount Entered was not a valid account");
                    ShowScreen();
                }
                break;
        }

        return (withdrawalAccount, depositAccount, withdrawalAccountBalance);
    }
    private double GetAmount(double withdrawalAccountBalance)
    {
        string strAmount = "";
        double dblAmount = 0.0;
        bool isAmountDouble = false;
        while (!isAmountDouble)
        {
            Console.WriteLine($"Enter the amount you want to transfer up to ${withdrawalAccountBalance.ToString("N2")}");
            strAmount = Console.ReadLine() ?? "";
            if (Double.TryParse(strAmount, out dblAmount))
            {
                if (dblAmount > 0)
                {
                    if (dblAmount <= withdrawalAccountBalance)
                    {
                        isAmountDouble = true;
                    }
                    else
                    {
                        Console.WriteLine("Cannot transfer more than the balance of withdrawing account. Please Try Again");
                    }
                }
                else
                {
                    Console.WriteLine("Amount must be a number greater than 0. Please Try Again");
                }
            }
            else
            {
                Console.WriteLine("Amount must be a number. Please Try Again");
            }
        }
        return dblAmount;
    }
}
