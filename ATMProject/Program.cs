using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Composition;
using ATMProject.Data.MockDatabase;
using ATMProject.Data.MockDatabase.MockDatabase;
using ATMProject.Data.ModifyData;
using ATMProject.WindowsConsoleApplication.AdminScreens;
using ATMProject.WindowsConsoleApplication.BasicScreens;
using Microsoft.Extensions.DependencyInjection;

namespace ATMProject.WindowsConsoleApplication;

public static class Program
{
    public static void Main(string[] args)
    {
        string dateTime = (new DateTime(2024, 2, 2)).ToString();
        // Creating a ServiceCollection,
        // whenever everything has been added to it
        // build a service provider
        var services = new ServiceCollection();
        AddServices(services);
        AddServiceConfigurations(services);

        // Create Service Provider 
        // get the ApplicationRunner service
        // bootstrap all the services and
        // then run the application.
        services.BuildServiceProvider()
            .GetService<ApplicationRunner>()!
            .RunApplication();
    }

    private static void AddServices(ServiceCollection services)
    {
        // When the constructor asks for an interface, it gives a new class
        services.AddSingleton<IReadFile, MockDatabaseFileRead>();
        services.AddSingleton<IWriteToFile, MockDatabaseFileWrite>();
        services.AddSingleton<IDataSource, MockDatabaseUserRepository>();
        services.AddSingleton<IUserRepository, ApplicationUserRepository>();
        services.AddSingleton<IUserContextService, UserContextService>();
        services.AddSingleton<IBasicOperationRepository, BasicOperationRepository>();
        services.AddSingleton<IAdminOperationsRepository, AdminOperationsRepository>();
        services.AddSingleton<ILogger, ConsoleLogger>();
        services.AddSingleton<IScreenGetter, ServiceLocatorScreenGetter>();
        services.AddSingleton<IScreenManager, ApplicationScreenManager>();
        services.AddSingleton<ApplicationRunner>();



        // go through all types implemented within the assembly where
        // LoginScreen is located.
        // For each of those types check if any of them implement IScreen
        // if they do for each of them register them as their own singleton.

        // LoginScreen is chosen because it is able to access all of the other screens
        // and therefore when finding all the types looks through all screens

        // GetInterfaces gets all interfaces the screen imlpements, and sees if any of them are IScreen

        // ToList is done because without it
        // there are some edge cases in which the linq wont run
        typeof(LoginScreen).Assembly.GetTypes()
            .Where((t) => t.GetInterfaces().Any(ti => ti == typeof(IScreen)))
            .ToList()
            .ForEach(screenType =>
            {
                services.AddSingleton(screenType);
            });
    }

    private static void AddServiceConfigurations(ServiceCollection services)
    {
        services.AddSingleton(
            serviceType: typeof(ServiceLocatorScreenGetterServiceConfiguration),
            implementationInstance: new ServiceLocatorScreenGetterServiceConfiguration(new Dictionary<ScreenNames, Type>{
                { ScreenNames.Login, typeof(LoginScreen) },
                { ScreenNames.ChangePassword, typeof(ChangePasswordScreen) },

                { ScreenNames.BasicOverview, typeof(BasicOverviewScreen) },
                { ScreenNames.Deposit, typeof(DepositScreen) },
                { ScreenNames.Withdrawal, typeof(WithdrawalScreen) },
                { ScreenNames.Transfer, typeof(TransactionTransferScreen) },
                { ScreenNames.History, typeof(HistoryScreen) },

                { ScreenNames.AdminOverview, typeof(AdminOverviewScreen) },
                { ScreenNames.LookupUser, typeof(LookupUserScreen) },
                { ScreenNames.ChangeUserPassword, typeof(ChangeUserPasswordScreen) },
                { ScreenNames.AddUser, typeof(AddUserScreen) },
                { ScreenNames.DeleteUser, typeof(DeleteUserScreen) },
                { ScreenNames.AddAccount, typeof(AddAccountScreen) },
                { ScreenNames.DeleteAccount, typeof(DeleteAccountScreen) },
            })
        );
    }
}
