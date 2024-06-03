using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;

namespace ATMProject.WindowsConsoleApplication.BasicScreens
{
    public class DepositScreen : IScreen
    {
        private readonly IUserRepository _userRepository;
        private readonly IUserContextService _userContextService;
        private readonly IScreenManager _screenManager;
        private readonly IScreenGetter _screenGetter;
        private readonly IDepositToAccountOperation _depositToAccountOp;

        public DepositScreen(IUserRepository userRepository, IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenFactory, IDepositToAccountOperation modifyDepositData)
        {
            _userRepository = userRepository;
            _userContextService = userContextService;
            _screenManager = screenManager;
            _screenGetter = screenFactory;
            _depositToAccountOp = modifyDepositData;
        }

        public record ViewModel
        (
            string Id,
            string Type,
            double Balance,
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

            string accountId = ChooseAccount(GetData());
            double amount = GetAmount();

            _depositToAccountOp.Execute(new IDepositToAccountOperation.Request(accountId, amount));
            _screenManager.ShowScreen(ScreenNames.BasicOverview);
        }
        private IEnumerable<ViewModel> GetData()
        {
            var accountData = _userRepository.GetUserAccountsByUserId(_userContextService.GetUserContext().UserId);

            return accountData.Select(accountData => new ViewModel(
                    Id: accountData.AccountId,
                    Type: accountData.Type.ToString(),
                    Balance: accountData.Balance,
                    CreationDate: accountData.CreationDate
            ));
        }

        private string ChooseAccount(IEnumerable<ViewModel> viewModel)
        {
            string accountEntered = "";

            switch (viewModel.Count())
            {
                case 0:
                    Console.WriteLine("Unable to use Screen due to a lack of accounts.");
                    _screenManager.ShowScreen(ScreenNames.BasicOverview);
                    break;
                case 1:
                    accountEntered = viewModel.FirstOrDefault().Id;
                    if (accountEntered is null)
                    {
                        _screenManager.ShowScreen(ScreenNames.BasicOverview);
                    }
                    break;
                default:
                    Console.WriteLine("Choose account: \n");
                    foreach (var account in viewModel)
                    {
                        Console.Write($"Type {account.Id} for Account with Type: {account.Type}, Balance: {account.Balance}\n");
                    }
                    accountEntered = Console.ReadLine() ?? "";
                    if (!viewModel.Any(acct => acct.Id == accountEntered))
                    {
                        Console.WriteLine("Account Entered was not a valid account");
                        ShowScreen();
                    }
                    break;
            }
            return accountEntered!;
        }

        private double GetAmount()
        {
            Console.WriteLine("Enter the amount that you want to deposit");
            return Convert.ToDouble(Console.ReadLine());
        }
    }
}
