using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;

namespace ATMProject.WindowsConsoleApplication.BasicScreens
{
    public class WithdrawalScreen : IScreen
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserContextService _userContextService;
        private readonly IScreenManager _screenManager;
        private readonly IScreenGetter _screenGetter;
        private readonly IWithdrawFromAccountOperation _modifyWithdrawalData;
        public WithdrawalScreen(IUserRepository userRepository, IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenFactory, IWithdrawFromAccountOperation modifyWithdrawalData)
        {
            _userRepository = userRepository;
            _userContextService = userContextService;
            _screenManager = screenManager;
            _screenGetter = screenFactory;
            _modifyWithdrawalData = modifyWithdrawalData;
        }
        public record AccountViewModel
        (
            string Id,
            string Type,
            string Balance,
            DateTime CreationDate
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

            ;
            string account = ChooseAccount(GetData());
            double amount = GetAmount();
            _modifyWithdrawalData.Execute(new IWithdrawFromAccountOperation.Request(account, amount));
            _screenManager.ShowScreen(ScreenNames.BasicOverview);
        }
        private IEnumerable<AccountViewModel> GetData()
        {
            var accountData = _userRepository.GetUserAccountsByUserId(_userContextService.GetUserContext().UserId);

            return accountData.Select(accountData => new AccountViewModel(
                Id: accountData.AccountId,
                Type: accountData.Type.ToString(),
                Balance: accountData.Balance.ToString(),
                CreationDate: accountData.CreationDate
            ));
        }
        private string ChooseAccount(IEnumerable<AccountViewModel> viewModel)
        {
            string accountEntered;
            if (viewModel.Count() >= 2)
            {
                Console.WriteLine("Choose account: \n");
                foreach (AccountViewModel account in viewModel)
                {
                    Console.Write($"Type {account.Id} for Account with Type: {account.Type}, Balance: {account.Balance}\n");
                }
                accountEntered = Console.ReadLine() ?? "";
                if (!viewModel.Any(acct => acct.Id == accountEntered))
                {
                    Console.WriteLine("Account Entered was not a valid account");
                    ShowScreen();
                }
            }
            else
            {
                accountEntered = viewModel.FirstOrDefault().Id;
                if (accountEntered is null)
                {
                    _screenManager.ShowScreen(ScreenNames.BasicOverview);
                }
            }
            return accountEntered!;
        }
        private double GetAmount()
        {
            Console.WriteLine("Enter the amount that you want to withdrawal");
            return Convert.ToDouble(Console.ReadLine());
        }
    }
}
