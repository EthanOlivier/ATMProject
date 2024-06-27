using ATMProject.Application;
using ATMProject.Application.Users;
using ATMProject.Banking;
using ATMProject.Data.FileProcesses;
using ATMProject.Data.FileProcesses.FileModels;
using System.Security.Principal;
using System.Transactions;

namespace ATMProject.Data.MockDatabase;
public class FileRead : IReadFile
{
    private readonly IDataStoreService<FileUserModel> _users;
    private readonly IDataStoreService<FileAccountModel> _accounts;
    private readonly IDataStoreService<FileTransactionModel> _transactions;
    private readonly IDataStoreService<FileAuditModel> _audits;
    private readonly ILogger _logger;
    public FileRead(IDataStoreService<FileUserModel> users, IDataStoreService<FileAccountModel> accounts, IDataStoreService<FileTransactionModel> transactions, IDataStoreService<FileAuditModel> audits, ILogger logger)
    {
        _users = users;
        _accounts = accounts;
        _transactions = transactions;
        _audits = audits;
        _logger = logger;
    }
    public void ReadAllFilesContents()
    {
        string FOLDER_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\";

        string USERS_FILE = Path.Combine(FOLDER_DIRECTORY, "Users.txt");
        string ACCOUNTS_FILE = Path.Combine(FOLDER_DIRECTORY, "Accounts.txt");
        string TRANSACTIONS_FILE = Path.Combine(Path.Combine(FOLDER_DIRECTORY, "Transactions.txt"));
        string AUDITS_FILE = Path.Combine(FOLDER_DIRECTORY, "Audits.txt");


        if (!File.Exists(USERS_FILE) || !File.Exists(ACCOUNTS_FILE) || !File.Exists(TRANSACTIONS_FILE) || !File.Exists(AUDITS_FILE))
        {
            throw new Exception("Error: Unable to locate all necessary files");
        }

        using (StreamReader usersReader = new StreamReader(USERS_FILE))
        {
            string line;
            while ((line = usersReader.ReadLine()!) is not null)
            {
                string[] userProperties = line.Split('|');
                if (userProperties.Length == 9 && int.TryParse(userProperties[0], out _))
                {
                    UserRole userRole = (UserRole)Enum.Parse(typeof(UserRole), userProperties[3]);
                    DateTime creationDate = DateTime.Parse(userProperties[8]);

                    _users.AddItem(new FileUserModel(userProperties[0], userProperties[1], userProperties[2], userRole, userProperties[4], userProperties[5], userProperties[6], userProperties[7], creationDate));
                }
            }
        }



        using (StreamReader accountsReader = new StreamReader(ACCOUNTS_FILE))
        {
            string line;
            while ((line = accountsReader.ReadLine()!) is not null)
            {
                string[] accountProperties = line.Split('|');
                if (accountProperties.Length == 5 && int.TryParse(accountProperties[0], out _))
                {
                    AccountType type = (AccountType)Enum.Parse(typeof(AccountType), accountProperties[2]);
                    double balance = Convert.ToDouble(accountProperties[3]);
                    DateTime creationDate = DateTime.Parse(accountProperties[4]);

                    _accounts.AddItem(new FileAccountModel(accountProperties[0], accountProperties[1], type, balance, creationDate));
                }
            }
        }



        using (StreamReader transactionsReader = new StreamReader(TRANSACTIONS_FILE))
        {
            string line;
            while ((line = transactionsReader.ReadLine()!) is not null)
            {
                string[] transactionProperties = line.Split('|');
                if (transactionProperties.Length == 7 && int.TryParse(transactionProperties[0], out _))
                {
                    TransactionType type = (TransactionType)Enum.Parse(typeof(TransactionType), transactionProperties[2]);
                    double amount = Convert.ToDouble(transactionProperties[3]);
                    double previousBalance = Convert.ToDouble(transactionProperties[4]);
                    double newBalance = Convert.ToDouble(transactionProperties[5]);
                    DateTime transactionDate = DateTime.Parse(transactionProperties[6]);

                    _transactions.AddItem(new FileTransactionModel(transactionProperties[0], transactionProperties[1], type, amount, previousBalance, newBalance, transactionDate));
                }
            }
        }



        using (StreamReader auditsReader = new StreamReader(AUDITS_FILE))
        {
            string line;
            while ((line = auditsReader.ReadLine()!) is not null)
            {
                string[] auditsProperties = line.Split('|');
                if (auditsProperties.Length == 5 && int.TryParse(auditsProperties[0], out _))
                {
                    AdminInteraction interaction = (AdminInteraction)Enum.Parse(typeof(AdminInteraction), auditsProperties[3]);
                    DateTime auditDate = DateTime.Parse(auditsProperties[4]);

                    _audits.AddItem(new FileAuditModel(auditsProperties[0], auditsProperties[1], auditsProperties[2], interaction, auditDate));
                }
            }
        }
    }
}
