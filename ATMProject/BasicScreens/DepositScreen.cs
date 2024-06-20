using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Operations.Authorization;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;
using ATMProject.System;

namespace ATMProject.WindowsConsoleApplication.BasicScreens;
public class DepositScreen : IScreen
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly ILogger _logger;
    private readonly IOperation<IDepositToAccountOperation.Request, IResult> _depositToAccountOperation;

    public DepositScreen(IUserRepository userRepository, IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenFactory, ILogger logger, IDepositToAccountOperation modifyDepositData)
    {
        _userRepository = userRepository;
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenFactory;
        _logger = logger;

        _depositToAccountOperation = modifyDepositData;
        _depositToAccountOperation = new LoggingOperationDecorator<IDepositToAccountOperation.Request, IResult>(_depositToAccountOperation, _userContextService, _logger);
        _depositToAccountOperation = new AuthorizationOperationDecorator<IDepositToAccountOperation.Request, IResult>(_depositToAccountOperation, _userContextService);
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
        if (!_userContextService.IsLoggedIn ||
            _userContextService.GetUserContext().UserRole == UserRole.Admin
        )
        {
            _userContextService.Logout();
            _screenManager.ShowScreen(ScreenNames.Login);
        }

        string accountId = ChooseAccount(GetData());
        double amount = GetAmount();

        _depositToAccountOperation.Execute(new IDepositToAccountOperation.Request(accountId, amount));

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

    private string ChooseAccount(IEnumerable<ViewModel> viewModel)
    {
        string accountEntered = "";

        switch (viewModel.Count())
        {
            case 0:
                _logger.Log("Warning: Unable to find any usable accounts.");
                Console.WriteLine("Unable to use Screen due to a lack of accounts.");
                _screenManager.ShowScreen(ScreenNames.BasicOverview);
                break;
            case 1:
                accountEntered = viewModel.FirstOrDefault()!.Id;
                if (accountEntered is null)
                {
                    throw new Exception("Error: Unable to find given account");
                }
                break;
            default:
                Console.WriteLine("Choose account or type 'X' to leave the screen\n");
                foreach (var account in viewModel)
                {
                    Console.Write($"Type {account.Id} for Account with Type: {account.Type}, Balance: ${account.Balance}\n");
                }
                accountEntered = Console.ReadLine() ?? "";
                if (accountEntered.ToUpper() == "X")
                {
                    _screenManager.ShowScreen(ScreenNames.BasicOverview);
                }
                if (!viewModel.Any(acct => acct.Id == accountEntered))
                {
                    Console.WriteLine("\nAccount Entered was not a valid account");
                    ShowScreen();
                }
                break;
        }
        return accountEntered!;
    }

    private double GetAmount()
    {
        string strAmount = "";
        double dblAmount = 0.0;
        bool isAmountDouble = false;
        while (!isAmountDouble)
        {
            Console.WriteLine("Enter the amount you want to deposit or type 'X' to leave the screen");
            strAmount = Console.ReadLine() ?? "";
            if (strAmount.ToUpper() == "X")
            {
                _screenManager.ShowScreen(ScreenNames.BasicOverview);
            }
            if (Double.TryParse(strAmount, out dblAmount))
            {
                if (dblAmount > 0)
                {
                    isAmountDouble = true;
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
