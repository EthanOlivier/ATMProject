using ATMProject.Data.FileProcesses.FileModels;

namespace ATMProject.Data.FileProcesses;
public class FileWrite : IWriteFile
{
    private readonly IDataStoreService<FileUserModel> _users;
    private readonly IDataStoreService<FileAccountModel> _accounts;
    private readonly IDataStoreService<FileTransactionModel> _transactions;
    private readonly IDataStoreService<FileAuditModel> _audits;
    public FileWrite(IDataStoreService<FileUserModel> users, IDataStoreService<FileAccountModel> accounts, IDataStoreService<FileTransactionModel> transactions, IDataStoreService<FileAuditModel> audits)
    {
        _users = users;
        _accounts = accounts;
        _transactions = transactions;
        _audits = audits;
    }
    public void UpdateUsersFile(string givenUserId, FileUserModel newUser)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Users.txt";

        if (givenUserId is null && newUser is not null)
        {
            _users.AddItem(newUser);

            File.AppendAllLines(FILE_DIRECTORY, new[] { newUser.UserId + "|" + newUser.Hash + "|" + newUser.Salt + "|" + newUser.UserRole + "|" + newUser.Name + "|" + newUser.Address + "|" + newUser.PhoneNumber + "|" + newUser.Email + "|" + newUser.CreationDate + "|" });
        }
        else if (givenUserId is not null || newUser is not null)
        {
            List<string> updatedFileContents = new List<string> { };
            FileUserModel userToRemove = null, userToAdd = null;

            foreach (FileUserModel user in _users.GetModels())
            {
                if (user.UserId != givenUserId || newUser is not null)
                {
                    FileUserModel fileUser = user;

                    if (user.UserId == givenUserId)
                    {
                        userToRemove = user;
                        fileUser = newUser;
                        userToAdd = fileUser;
                    }

                    updatedFileContents.Add(fileUser.UserId + "|" + fileUser.Hash + "|" + fileUser.Salt + "|" + fileUser.UserRole + "|" + fileUser.Name + "|" + fileUser.Address + "|" + fileUser.PhoneNumber + "|" + fileUser.Email + "|" + fileUser.CreationDate);
                }
                else
                {
                    userToRemove = user;
                }
            }

            if (userToRemove is not null)
            {
                _users.RemoveItem(userToRemove);
            }
            if (userToAdd is not null)
            {
                _users.AddItem(userToAdd);
            }

            File.WriteAllLines(FILE_DIRECTORY, updatedFileContents);
        }
    }
    public void UpdateAccountsFile(string[] givenAccountIds, FileAccountModel newAccount)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Accounts.txt";

        if (givenAccountIds is null && newAccount is not null)
        {
            _accounts.AddItem(newAccount);

            File.AppendAllLines(FILE_DIRECTORY, new[] { newAccount.AccountId + "|" + newAccount.UserId + "|" + newAccount.Type + "|" + newAccount.Balance + "|" + newAccount.CreationDate });
        }
        else if (givenAccountIds is not null || newAccount is not null)
        {
            List<string> updatedFileContents = new List<string> { };
            FileAccountModel accountToRemove = null, accountToAdd = null;

            foreach (FileAccountModel account in _accounts.GetModels())
            {
                if (!givenAccountIds.Contains(account.AccountId) || newAccount is not null)
                {
                    FileAccountModel fileAccount = account;

                    if (givenAccountIds.Contains(account.AccountId))
                    {
                        accountToRemove = account;
                        fileAccount = newAccount;
                        accountToAdd = fileAccount;
                    }

                    updatedFileContents.Add(fileAccount.AccountId + "|" + fileAccount.UserId + "|" + fileAccount.Type + "|" + fileAccount.Balance + "|" + fileAccount.CreationDate);
                }
                else
                {
                    accountToRemove = account;
                }
            }

            if (accountToRemove is not null)
            {
                _accounts.RemoveItem(accountToRemove);
            }
            if (accountToAdd is not null)
            {
                _accounts.AddItem(accountToAdd);
            }

            File.WriteAllLines(FILE_DIRECTORY, updatedFileContents);
        }
    }
    public void UpdateTransactionsFile(string[] transactionIds, FileTransactionModel newTransaction)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Transactions.txt";

        if (newTransaction is not null)
        {
            _transactions.AddItem(newTransaction);
            File.AppendAllLines(FILE_DIRECTORY, new[] { newTransaction.TranasctionId + "|" + newTransaction.AccountId + "|" + newTransaction.Type + "|" + newTransaction.Amount + "|" + newTransaction.PreviousBalance + "|" + newTransaction.NewBalance + "|" + newTransaction.DateTime });
        }
        else
        {
            List<string> updatedFileContents = new List<string> { };
            FileTransactionModel transactionToRemove = null;

            foreach (FileTransactionModel transaction in _transactions.GetModels())
            {
                if (!transactionIds.Contains(transaction.TranasctionId))
                {
                    updatedFileContents.Add(transaction.TranasctionId + "|" + transaction.AccountId + "|" + transaction.Type + "|" + transaction.Amount + "|" + transaction.PreviousBalance + "|" + transaction.NewBalance + "|" + transaction.DateTime);
                }
                else
                {
                    transactionToRemove = transaction;
                }
            }

            if (transactionToRemove is not null)
            {
                _transactions.RemoveItem(transactionToRemove);
            }

            File.WriteAllLines(FILE_DIRECTORY, updatedFileContents);
        }
    }
    public void UpdateAuditsFile(FileAuditModel newAudit)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Audits.txt";
        string[] transactionsAndAuditsFileContents = File.ReadAllLines(FILE_DIRECTORY);

        string recordToAdd = newAudit.AuditId + "|" + newAudit.AdminId + "|" + newAudit.BasicId + "|" + newAudit.InteractionType + "|" + newAudit.DateTime;
        File.WriteAllLines(FILE_DIRECTORY, transactionsAndAuditsFileContents);
        File.AppendAllLines(FILE_DIRECTORY, new[] { recordToAdd });
    }
}
