using ATMProject.Data.FileProcesses.FileModels;
using ATMProject.Data.MockDatabase;

namespace ATMProject.Data.FileProcesses;
public class FileWrite : IWriteFile
{
    public void UpdateUsersFile(string givenUserId, FileUserModel newUser)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Users.txt";

        if (givenUserId is null && newUser is not null)
        {
            FileRead.Users.Add(newUser);

            File.AppendAllLines(FILE_DIRECTORY, new[] { newUser.UserId + "|" + newUser.Hash + "|" + newUser.Salt + "|" + newUser.UserRole + "|" + newUser.Name + "|" + newUser.Address + "|" + newUser.PhoneNumber + "|" + newUser.Email + "|" + newUser.CreationDate + "|" });
        }
        else if (givenUserId is not null || newUser is not null)
        {
            List<string> updatedFileContents = new List<string> { };
            FileUserModel userToRemove = null, userToAdd = null;

            foreach (FileUserModel user in FileRead.Users)
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

                    string newIds = "";
                    if (fileUser.AccountIds is not null && fileUser.AccountIds.Count() != 0)
                    {
                        if (fileUser.AccountIds[0] == "")
                        {
                            fileUser.AccountIds.RemoveAt(0);
                        }
                        newIds = string.Join(";", fileUser.AccountIds);
                    }
                    updatedFileContents.Add(fileUser.UserId + "|" + fileUser.Hash + "|" + fileUser.Salt + "|" + fileUser.UserRole + "|" + fileUser.Name + "|" + fileUser.Address + "|" + fileUser.PhoneNumber + "|" + fileUser.Email + "|" + fileUser.CreationDate + "|" + newIds);
                }
                else
                {
                    userToRemove = user;
                }
            }

            if (userToRemove is not null)
            {
                FileRead.Users.Remove(userToRemove);
            }
            if (userToAdd is not null)
            {
                FileRead.Users.Add(userToAdd);
            }

            File.WriteAllLines(FILE_DIRECTORY, updatedFileContents);
        }
    }
    public void UpdateAccountsFile(string[] givenAccountIds, FileAccountModel newAccount)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Accounts.txt";

        if (givenAccountIds is null && newAccount is not null)
        {
            FileRead.Accounts.Add(newAccount);

            File.AppendAllLines(FILE_DIRECTORY, new[] { newAccount.AccountId + "|" + newAccount.UserId + "|" + newAccount.Type + "|" + newAccount.Balance + "|" + newAccount.CreationDate + "|" });
        }
        else if (givenAccountIds is not null || newAccount is not null)
        {
            List<string> updatedFileContents = new List<string> { };
            FileAccountModel accountToRemove = null, accountToAdd = null;

            foreach (FileAccountModel account in FileRead.Accounts)
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

                    string newTransactions = "";
                    if (fileAccount.Transactions is not null && fileAccount.Transactions.Count() != 0)
                    {
                        if (fileAccount.Transactions[0] == "")
                        {
                            fileAccount.Transactions.RemoveAt(0);
                        }
                        newTransactions = string.Join(";", fileAccount.Transactions);
                    }
                    updatedFileContents.Add(fileAccount.AccountId + "|" + fileAccount.UserId + "|" + fileAccount.Type + "|" + fileAccount.Balance + "|" + fileAccount.CreationDate + "|" + newTransactions);
                }
                else
                {
                    accountToRemove = account;
                }
            }

            if (accountToRemove is not null)
            {
                FileRead.Accounts.Remove(accountToRemove);
            }
            if (accountToAdd is not null)
            {
                FileRead.Accounts.Add(accountToAdd);
            }

            File.WriteAllLines(FILE_DIRECTORY, updatedFileContents);
        }
    }
    public void UpdateTransactionsFile(FileTransactionModel newTransaction, string[] accountIds)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Transactions.txt";

        if (newTransaction is not null)
        {
            FileRead.Transactions.Add(newTransaction);
            File.AppendAllLines(FILE_DIRECTORY, new[] { newTransaction.TranasctionId + "|" + newTransaction.AccountId + "|" + newTransaction.Type + "|" + newTransaction.Amount + "|" + newTransaction.PreviousBalance + "|" + newTransaction.NewBalance + "|" + newTransaction.DateTime });
        }
        else
        {
            List<string> updatedFileContents = new List<string> { };
            FileTransactionModel transactionToRemove = null;

            foreach (FileTransactionModel transaction in FileRead.Transactions)
            {
                if (!accountIds.Contains(transaction.AccountId))
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
                FileRead.Transactions.Remove(transactionToRemove);
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
