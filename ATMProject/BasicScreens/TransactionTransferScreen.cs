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
    public record AccountViewModel
    (
        string Id,
        string Type,
        double Balance,
        DateTime CreationDate
    );


    public void ShowScreen()
    {
        string depositAccount, withdrawalAccount;
        double amount;
        if (!_userContextService.IsLoggedIn ||
            _userContextService.GetUserContext().UserRole == UserRole.Admin
        )
        {
            _userContextService.Logout();
            _screenManager.ShowScreen(ScreenNames.Login);
        }

        (withdrawalAccount, depositAccount) = ChooseAccounts(GetData());
        amount = GetAmount();

        _transferBetweenAccountsOperation.Execute(new ITransferBetweenAccountsOperation.Request(withdrawalAccount, depositAccount, amount));
        
        _screenManager.ShowScreen(ScreenNames.BasicOverview);
    }
    private IEnumerable<AccountViewModel> GetData()
    {
        var accountData = _userRepository.GetUserAccountsByUserId(_userContextService.GetUserContext().UserId);

        return accountData.Select(accountData => new AccountViewModel(
            Id: accountData.AccountId,
            Type: accountData.Type.ToString(),
            Balance: accountData.Balance,
            CreationDate: accountData.CreationDate
        ));
    }
    private (string, string) ChooseAccounts(IEnumerable<AccountViewModel> viewModel)
    {
        string withdrawalAccount, depositAccount;


        foreach (AccountViewModel account in viewModel)
        {
            Console.WriteLine($"Type {account.Id} to transfer from Account with Type: {account.Type}, Balance: {account.Balance}");
        }
        withdrawalAccount = Console.ReadLine() ?? "";

        if (!viewModel.Any(acct => acct.Id == withdrawalAccount))
        {
            Console.WriteLine("Account Entered was not a valid account");
            ShowScreen();
        }


        if (viewModel.Count() > 2)
        {
            foreach (AccountViewModel account in viewModel)
            {
                if (account.Id != withdrawalAccount)
                {
                    Console.WriteLine($"Type {account.Id} to transfer into Account with Type: {account.Type}, Balance: {account.Balance}");
                }
            }
            depositAccount = Console.ReadLine() ?? "";

            if (!viewModel.Any(acct => acct.Id == withdrawalAccount))
            {
                Console.WriteLine("Account Entered was not a valid account");
                ShowScreen();
            }
        }
        else
        {
            depositAccount = viewModel.FirstOrDefault(acct => acct.Id != withdrawalAccount).Id;
        }

        return (withdrawalAccount, depositAccount);
    }
    private double GetAmount()
    {
        Console.WriteLine("Enter the amount that you want to transfer");
        return Convert.ToDouble(Console.ReadLine());
    }
}
