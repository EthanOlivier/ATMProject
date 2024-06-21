using ATMProject.Application;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;

namespace ATMProject.WindowsConsoleApplication.BasicScreens;
public class HistoryScreen : IScreen
{
    private readonly IUserRepository _userRepository;
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly ILogger _logger;

    public HistoryScreen(IUserRepository userRepository, IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenFactory, ILogger logger)
    {
        _userRepository = userRepository;
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenFactory;
        _logger = logger;
    }
    public record ViewModel
    (
        string Id,
        string Type,
        double Balance,
        string CreationDate,
        IEnumerable<ViewModel.TransactionsViewModel> Transactions
    )
    {
        public record TransactionsViewModel
        (
            string Id,
            string Type,
            string Amount,
            string PreviousBalance,
            string NewBalance,
            string DateTime
        );
    }


    public void ShowScreen()
    {
        ShowHistory(GetData());
        _screenManager.ShowScreen(ScreenNames.BasicOverview);
    }
    private IEnumerable<ViewModel> GetData()
    {
        var accountData = _userRepository.GetUserAccountsByUserId(_userContextService.GetUserContext().UserId);

        return accountData.Select(accountData => new ViewModel(
            Id: accountData.AccountId,
            Type: accountData.Type.ToString(),
            Balance: accountData.Balance,
            CreationDate: accountData.CreationDate.ToString(),
            Transactions: accountData.TransactionIds?
                .Where(tran => tran.AccountId == accountData.AccountId)
                .Select(tran => new ViewModel.TransactionsViewModel(
                    Id: tran.TransactionId,
                    Type: tran.Type.ToString(),
                    Amount: tran.Amount.ToString(),
                    PreviousBalance: tran.PreviousBalance.ToString(),
                    NewBalance: tran.NewBalance.ToString(),
                    DateTime: tran.DateTime.ToString()
                ))
        ));
    }
    private void ShowHistory(IEnumerable<ViewModel> viewModel)
    {
        ViewModel account = null;

        switch (viewModel.Count())
        {
            case 0:
                _logger.Log("Warning: Unable to find any usable accounts.");
                Console.WriteLine("Unable to use Screen due to a lack of accounts.");
                _screenManager.ShowScreen(ScreenNames.BasicOverview);
                break;
            case 1:
                account = viewModel.FirstOrDefault()!;
                if (account is null)
                {
                    throw new Exception("Error: Unable to find given account");
                }
                break;
            default:
                string accountEntered;
                Console.WriteLine("Enter an Account or type 'X' to leave the screen\n");
                foreach (var accnt in viewModel)
                {
                    Console.WriteLine($"Type {accnt.Id} to show Transaction History from Account with Type: {accnt.Type}, Balance: ${accnt.Balance.ToString("N2")}");
                }
                accountEntered = Console.ReadLine() ?? "";

                if (accountEntered.ToUpper() == "X")
                {
                    _screenManager.ShowScreen(ScreenNames.BasicOverview);
                }

                if (!viewModel.Any(acct => acct.Id == accountEntered))
                {
                    Console.WriteLine("Account Entered was not a valid account");
                    ShowScreen();
                }

                account = viewModel.FirstOrDefault(acct => acct.Id == accountEntered)!;
                if (account is null)
                {
                    throw new Exception("Error: Unable to find given account");
                }
                break;
        }


        if (account!.Transactions.Any())
        {
            foreach (var transaction in account.Transactions)
            {
                Console.Write($"{transaction.Type} of ${transaction.Amount} on {transaction.DateTime}. Previous Balance: ${transaction.PreviousBalance}. New Balance: ${transaction.NewBalance}\n");
            }
        }
        else
        {
            Console.WriteLine("No Transactions Found");
        }
    }
}
