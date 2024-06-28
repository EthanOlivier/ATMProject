using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Application.Users;

namespace ATMProject.WindowsConsoleApplication.AdminScreens;
public class LookupUserScreen : IScreen
{
    private readonly IUserContextService _userContextService;
    private readonly IScreenManager _screenManager;
    private readonly IScreenGetter _screenGetter;
    private readonly ILookupUser _lookupUser;

    public LookupUserScreen
    (
        IUserContextService userContextService, 
        IScreenManager screenManager, 
        IScreenGetter screenGetter, 
        ILookupUser lookupUserOperations
    )
    {
        _userContextService = userContextService;
        _screenManager = screenManager;
        _screenGetter = screenGetter;
        _lookupUser = lookupUserOperations;
    }


    public void ShowScreen()
    {
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
            Console.WriteLine("Select the Identity Field you would like to use to find your user or type 'X' to leave the screen");
            Console.WriteLine("Type 'N' to use the User's Name");
            Console.WriteLine("Type 'A' to use the User's Address");
            Console.WriteLine("Type 'P' to use the User's Phone Number");
            Console.WriteLine("Type 'E' to use the User's Email");

            string input = Console.ReadLine() ?? "";
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
                case "X":
                    _screenManager.ShowScreen(ScreenNames.AdminOverview);
                    break;
                default:
                    Console.WriteLine("Incorrect Identity Field Entered. Please Try Again.\n");
                    break;
            }
        }
        while (true);
    }
    private string[] FindUserInfo(IdentityFields field)
    {
        Console.WriteLine($"Enter the {field} of the User whose User Id you would like to find or type 'X' to leave the screen");
        string input = Console.ReadLine() ?? "";
        while (input == "")
        {
            Console.WriteLine($"Please Enter a {field}");
            input = Console.ReadLine() ?? "";
        }
        if (input.ToUpper() == "X")
        {
            _screenManager.ShowScreen(ScreenNames.AdminOverview);
        }

        return _lookupUser.LookupUserInfo(field, input, _userContextService.GetUserContext().UserId);
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
            Console.WriteLine(users.FirstOrDefault());
        }
    }
}
