using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;
using ATMProject.Data.MockDatabase;
using ATMProject.Data.MockDatabase.MockDatabase;

namespace ATMProject.WindowsConsoleApplication.AdminScreens
{
    public class LookupUserScreen : IScreen
    {
        private readonly IUserContextService _userContextService;
        private readonly IScreenManager _screenManager;
        private readonly IScreenGetter _screenGetter;
        private readonly ILookupUserOperations _lookupUserOperations;

        public LookupUserScreen(IUserContextService userContextService, IScreenManager screenManager, IScreenGetter screenGetter, ILookupUserOperations lookupUserOperations)
        {
            _userContextService = userContextService;
            _screenManager = screenManager;
            _screenGetter = screenGetter;
            _lookupUserOperations = lookupUserOperations;
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

            DisplayUserInfo(
                FindUserInfo(
                    SelectIdentityField()
                )
            );

            _screenManager.ShowScreen(ScreenNames.AdminOverview);
        }
        private IdentityFields SelectIdentityField()
        {
            do
            {
                Console.WriteLine("Select the Identity Field you would like to use to find your user.");
                Console.WriteLine("Type 'N' to use the User's Name");
                Console.WriteLine("Type 'A' to use the User's Address");
                Console.WriteLine("Type 'P' to use the User's Phone Number");
                Console.WriteLine("Type 'E' to use the User's Email");

                string? input = Console.ReadLine();
                while (input is null)
                {
                    Console.WriteLine("Please enter an Identity Field");
                    input = Console.ReadLine();
                }
                switch (input.ToUpper())
                {
                    case "N":
                        return IdentityFields.Name;
                    case "A":
                        return IdentityFields.Address;
                    case "P":
                        return IdentityFields.PhoneNumber;
                    case "E":
                        return IdentityFields.Email;
                    default:
                        Console.WriteLine("Incorrect Identity Field Entered. Please Try Again.\n");
                        break;
                }
            }
            while (true);
        }
        private string[] FindUserInfo(IdentityFields field)
        {
            Console.WriteLine($"Enter the {field} of the User whose User Id you would like to find.");
            string? input = Console.ReadLine();
            while (input is null)
            {
                Console.WriteLine($"Please enter a(n) {field}");
                input = Console.ReadLine();
            }

            return _lookupUserOperations.LookupUserInfo(field, input, _userContextService.GetUserContext().UserId);
        }
        private void DisplayUserInfo(string[] users)
        {
            if (users.Length == 0)
            {
                Console.WriteLine("Unable to find any users with entered field.");
            }
            else if (users.Length > 1)
            {
                Console.WriteLine("More than one user with entered field");
                Console.WriteLine("User Ids:");
                foreach (string singleUser in users)
                {
                    Console.WriteLine($"{singleUser}");
                }
            }
            else
            {
                Console.WriteLine("User Id:");
                Console.WriteLine(users.First());
            }
        }
    }
}
