using ATMProject.Application.Users;
using ATMProject.Banking;
using ATMProject.Data.FileProcesses;
using ATMProject.Data.FileProcesses.FileModels;

namespace ATMProject.Data.MockDatabase;
public class FileRead : IReadFile
{
    private readonly IDataStoreService<FileUserModel> _users;
    private readonly IDataStoreService<FileAccountModel> _accounts;
    private readonly IDataStoreService<FileTransactionModel> _transactions;
    private readonly IDataStoreService<FileAuditModel> _audits;
    public FileRead(IDataStoreService<FileUserModel> users, IDataStoreService<FileAccountModel> accounts, IDataStoreService<FileTransactionModel> transactions, IDataStoreService<FileAuditModel> audits)
    {
        _users = users;
        _accounts = accounts;
        _transactions = transactions;
        _audits = audits;
    }
    public void ReadAllFilesContents()
    {
        string FOLDER_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\";

        string[] usersFileContents = File.ReadAllLines(Path.Combine(FOLDER_DIRECTORY, "Users.txt"));

        foreach (string user in usersFileContents)
        {
            string[] userProperties = user.Split('|');
            if (userProperties.Length == 9 && int.TryParse(userProperties[0], out _))
            {
                UserRole userRole = (UserRole)Enum.Parse(typeof(UserRole), userProperties[3]);
                DateTime creationDate = DateTime.Parse(userProperties[8]);

                _users.AddItem(new FileUserModel(userProperties[0], userProperties[1], userProperties[2], userRole, userProperties[4], userProperties[5], userProperties[6], userProperties[7], creationDate));
            }
        }



        string[] accountsFileContents = File.ReadAllLines(Path.Combine(FOLDER_DIRECTORY, "Accounts.txt"));

        foreach (string account in accountsFileContents)
        {
            string[] accountProperties = account.Split('|');
            if (accountProperties.Length == 5 && int.TryParse(accountProperties[0], out _))
            {
                AccountType type = (AccountType)Enum.Parse(typeof(AccountType), accountProperties[2]);
                double balance = Convert.ToDouble(accountProperties[3]);
                DateTime creationDate = DateTime.Parse(accountProperties[4]);

                _accounts.AddItem(new FileAccountModel(accountProperties[0], accountProperties[1], type, balance, creationDate));
            }
        }



        string[] transactionsFileContents = File.ReadAllLines(Path.Combine(FOLDER_DIRECTORY, "Transactions.txt"));

        foreach (string transaction in transactionsFileContents)
        {
            string[] transactionProperties = transaction.Split('|');
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



        string[] AuditsFileContents = File.ReadAllLines(Path.Combine(FOLDER_DIRECTORY, "Audits.txt"));

        foreach (string audits in AuditsFileContents)
        {
            string[] auditsProperties = audits.Split('|');
            if (auditsProperties.Length == 5 && int.TryParse(auditsProperties[0], out _))
            {
                AdminInteraction interaction = (AdminInteraction)Enum.Parse(typeof(AdminInteraction), auditsProperties[3]);
                DateTime auditDate = DateTime.Parse(auditsProperties[4]);

                _audits.AddItem(new FileAuditModel(auditsProperties[0], auditsProperties[1], auditsProperties[2], interaction, auditDate));
            }
        }
    }
}
