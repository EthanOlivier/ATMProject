using ATMProject.Application.Users;
using ATMProject.Banking;
using ATMProject.Data.FileProcesses;
using ATMProject.Data.FileProcesses.FileModels;

namespace ATMProject.Data.MockDatabase;
public class FileRead : IReadFile
{
    public static HashSet<FileUserModel> Users = new HashSet<FileUserModel>();
    public static HashSet<FileAccountModel> Accounts = new HashSet<FileAccountModel>();
    public static HashSet<FileTransactionModel> Transactions = new HashSet<FileTransactionModel>();
    public static HashSet<FileAuditModel> Audits = new HashSet<FileAuditModel>();

    public void ReadAllFilesContents()
    {
        string FOLDER_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\";

        string[] usersFileContents = File.ReadAllLines(Path.Combine(FOLDER_DIRECTORY, "Users.txt"));

        foreach (string user in usersFileContents)
        {
            string[] userProperties = user.Split('|');
            if (userProperties.Length == 10)
            {
                if (int.TryParse(userProperties[0], out _))
                {
                    UserRole userRole = (UserRole)Enum.Parse(typeof(UserRole), userProperties[3]);
                    DateTime creationDate = DateTime.Parse(userProperties[8]);
                    List<string> accountIds = new List<string>(userProperties[9].Split(';'));

                    Users.Add(new FileUserModel(userProperties[0], userProperties[1], userProperties[2], userRole, userProperties[4], userProperties[5], userProperties[6], userProperties[7], creationDate, accountIds));
                }
            }
        }



        string[] accountsFileContents = File.ReadAllLines(Path.Combine(FOLDER_DIRECTORY, "Accounts.txt"));

        foreach (string account in accountsFileContents)
        {
            string[] accountProperties = account.Split('|');
            if (accountProperties.Length == 6)
            {
                if (int.TryParse(accountProperties[0], out _))
                {
                    AccountType type = (AccountType)Enum.Parse(typeof(AccountType), accountProperties[2]);
                    double balance = Convert.ToDouble(accountProperties[3]);
                    DateTime creationDate = DateTime.Parse(accountProperties[4]);
                    List<string> transactionsAndAuditsIds = new List<string>(accountProperties[5].Split(';'));

                    Accounts.Add(new FileAccountModel(accountProperties[0], accountProperties[1], type, balance, creationDate, transactionsAndAuditsIds));
                }
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

                Transactions.Add(new FileTransactionModel(transactionProperties[0], transactionProperties[1], type, amount, previousBalance, newBalance, transactionDate));
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

                Audits.Add(new FileAuditModel(auditsProperties[0], auditsProperties[1], auditsProperties[2], interaction, auditDate));
            }
        }
    }
}
