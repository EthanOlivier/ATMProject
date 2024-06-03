using ATMProject.Application;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;

namespace ATMProject.WindowsConsoleApplication.BasicScreens
{
    public class HistoryScreen : IScreen
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserContextService _userContextService;
        private readonly IScreenManager _screenManager;
        private readonly IScreenGetter _screenGetter;

        public HistoryScreen(IUserRepository userRepository, IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenFactory)
        {
            _userRepository = userRepository;
            _userContextService = userContextService;
            _screenManager = screenManager;
            _screenGetter = screenFactory;
        }
        public record AccountViewModel
        (
            string Id,
            string Type,
            double Balance,
            DateTime CreationDate,
            IEnumerable<AccountViewModel.TransactionsViewModel> Transactions
        )
        {
            public record TransactionsViewModel
            (
                string Id,
                string Type,
                string Amount,
                string PreviousBalance,
                string NewBalance,
                DateTime DateTime
            );
        }


        public void ShowScreen()
        {
            if (!_userContextService.IsLoggedIn ||
                _userContextService.GetUserContext().UserRole == UserRole.Admin
            )
            {
                _userContextService.Logout();
                _screenManager.ShowScreen(ScreenNames.Login);
            }

            ShowHistory(GetData());
            _screenManager.ShowScreen(ScreenNames.BasicOverview);
        }
        private IEnumerable<AccountViewModel> GetData()
        {
            var accountData = _userRepository.GetUserAccountsByUserId(_userContextService.GetUserContext().UserId);
            var transactionData = _userRepository.GetAccountTransactionsByAccountIds(accountData.Select(acct => acct.AccountId).ToArray());

            return accountData.Select(accountData => new AccountViewModel(
                Id: accountData.AccountId,
                Type: accountData.Type.ToString(),
                Balance: accountData.Balance,
                CreationDate: accountData.CreationDate,
                Transactions: transactionData?
                    .Where(tran => tran.AccountId == accountData.AccountId)
                    .Select(tran => new AccountViewModel.TransactionsViewModel(
                        Id: tran.TransactionId,
                        Type: tran.Type.ToString(),
                        Amount: tran.Amount.ToString(),
                        PreviousBalance: tran.PreviousBalance.ToString(),
                        NewBalance: tran.NewBalance.ToString(),
                        DateTime: tran.DateTime
                    ))
            ));
        }
        private void ShowHistory(IEnumerable<AccountViewModel> viewModel)
        {
            AccountViewModel account;
            if (viewModel.Count() > 1)
            {
                string accountEntered;
                foreach (AccountViewModel accnt in viewModel)
                {
                    Console.WriteLine($"Type {accnt.Id} to show Transaction History from Account with Type: {accnt.Type}, Balance: {accnt.Balance}");
                }
                accountEntered = Console.ReadLine() ?? "";

                if (!viewModel.Any(acct => acct.Id == accountEntered))
                {
                    Console.WriteLine("Account Entered was not a valid account");
                    ShowScreen();
                }

                account = viewModel.FirstOrDefault(acct => acct.Id == accountEntered);
            }
            else
            {
                account = viewModel.FirstOrDefault()!;
                if (account is null)
                {
                    Console.WriteLine("No Accounts Found");
                    _screenManager.ShowScreen(ScreenNames.BasicOverview);
                }
            }


            if (account.Transactions.Count() > 0)
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
}
