using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Application.Screens;
using ATMProject.Composition;
using ATMProject.Data.FileProcesses;
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
        var services = new ServiceCollection();
        AddServices(services);
        AddServiceConfigurations(services);

      services.BuildServiceProvider()
            .GetService<ApplicationRunner>()!
            .RunApplication();
    }

    private static void AddServices(ServiceCollection services)
    {
        services.AddSingleton<IReadFile, FileRead>();
        services.AddSingleton<IWriteFile, FileWrite>();
        services.AddSingleton<IDataSource, FileUserRepository>();
        services.AddSingleton<IUserRepository, ApplicationUserRepository>();
        services.AddSingleton<IUserContextService, UserContextService>();
        services.AddSingleton<IBasicOperationRepository, BasicOperationRepository>();
        services.AddSingleton<IAdminOperationsRepository, AdminOperationsRepository>();
        services.AddSingleton<ILogger, ConsoleLogger>();
        services.AddSingleton<IScreenGetter, ServiceLocatorScreenGetter>();
        services.AddSingleton<IScreenManager, ApplicationScreenManager>();
        services.AddSingleton<ApplicationRunner>();

        typeof(LoginScreen).Assembly.GetTypes()
            .Where((t) => t.GetInterfaces().Any(ti => ti == typeof(IScreen)))
            .ToList()
            .ForEach(screenType =>
            {
                services.AddSingleton(screenType);
            });

        typeof(IBasicOperationRepository).GetInterfaces()
            .Where(t => !typeof(IBasicOperationRepository).GetInterfaces().Any(anyBorFace => anyBorFace.GetInterfaces().Contains(t)))
            .ToList()
            .ForEach(screenType =>
            {
                services.AddSingleton(screenType, typeof(BasicOperationRepository));
            });

        typeof(IAdminOperationsRepository).GetInterfaces()
            .Where(t => !typeof(IAdminOperationsRepository).GetInterfaces().Any(anyAorFace => anyAorFace.GetInterfaces().Contains(t)))
            .ToList()
            .ForEach(screenType =>
            {
                services.AddSingleton(screenType, typeof(AdminOperationsRepository));
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
