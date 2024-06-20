using ATMProject.Data.MockDatabase;
using ATMProject.Data.MockDatabase.MockDatabase;
using System.Reflection;
using System.Security.Principal;

namespace ATMProject.Data.ModifyData;
public class MockDatabaseFileWrite : IWriteToFile
{
    public void UpdateUsersFile(string givenUserId, MockDatabaseUserModel newUser)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Users.txt";

        if (givenUserId is null && newUser is not null)
        {
            MockDatabaseFileRead.Users.Add(newUser);

            File.AppendAllLines(FILE_DIRECTORY, new[] { newUser.UserId + "|" + newUser.Hash + "|" + newUser.Salt + "|" + newUser.UserRole + "|" + newUser.Name + "|" + newUser.Address + "|" + newUser.PhoneNumber + "|" + newUser.Email + "|" + newUser.CreationDate + "|" });
        }
        else if (givenUserId is not null || newUser is not null)
        {
            List<string> updatedFileContents = new List<string> { };
            MockDatabaseUserModel userToRemove = null, userToAdd = null;

            foreach (MockDatabaseUserModel user in MockDatabaseFileRead.Users)
            {
                if (user.UserId != givenUserId || newUser is not null)
                {
                    MockDatabaseUserModel fileUser = user;

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
                        newIds = String.Join(";", fileUser.AccountIds);
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
                MockDatabaseFileRead.Users.Remove(userToRemove);
            }
            if (userToAdd is not null)
            {
                MockDatabaseFileRead.Users.Add(userToAdd);
            }

            File.WriteAllLines(FILE_DIRECTORY, updatedFileContents);
        }
    }
    public void UpdateAccountsFile(string[] givenAccountIds, MockDatabaseAccountModel newAccount)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Accounts.txt";

        if (givenAccountIds is null && newAccount is not null)
        {
            MockDatabaseFileRead.Accounts.Add(newAccount);

            File.AppendAllLines(FILE_DIRECTORY, new[] { newAccount.AccountId + "|" + newAccount.UserId + "|" + newAccount.Type + "|" + newAccount.Balance + "|" + newAccount.CreationDate + "|" });
        }
        else if (givenAccountIds is not null || newAccount is not null)
        {
            List<string> updatedFileContents = new List<string> { };
            MockDatabaseAccountModel accountToRemove = null, accountToAdd = null;

            foreach (MockDatabaseAccountModel account in MockDatabaseFileRead.Accounts)
            {
                if (!givenAccountIds.Contains(account.AccountId) || newAccount is not null)
                {
                    MockDatabaseAccountModel fileAccount = account;

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
                        newTransactions = String.Join(";", fileAccount.Transactions);
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
                MockDatabaseFileRead.Accounts.Remove(accountToRemove);
            }
            if (accountToAdd is not null)
            {
                MockDatabaseFileRead.Accounts.Add(accountToAdd);
            }

            File.WriteAllLines(FILE_DIRECTORY, updatedFileContents);
        }
    }
    public void UpdateTransactionsFile(MockDatabaseTransactionModel newTransaction, string[] accountIds)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Transactions.txt";

        if (newTransaction is not null)
        {
            MockDatabaseFileRead.Transactions.Add(newTransaction);
            File.AppendAllLines(FILE_DIRECTORY, new[] { newTransaction.TranasctionId + "|" + newTransaction.AccountId + "|" + newTransaction.Type + "|" + newTransaction.Amount + "|" + newTransaction.PreviousBalance + "|" + newTransaction.NewBalance + "|" + newTransaction.DateTime });
        }
        else
        {
            List<string> updatedFileContents = new List<string> { };
            MockDatabaseTransactionModel transactionToRemove = null;

            foreach (MockDatabaseTransactionModel transaction in MockDatabaseFileRead.Transactions)
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
                MockDatabaseFileRead.Transactions.Remove(transactionToRemove);
            }

            File.WriteAllLines(FILE_DIRECTORY, updatedFileContents);
        }
    }
    public void UpdateAuditsFile(MockDatabaseAuditModel newAudit)
    {
        string FILE_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\Audits.txt";
        string[] transactionsAndAuditsFileContents = File.ReadAllLines(FILE_DIRECTORY);

        string recordToAdd = newAudit.AuditId + "|" + newAudit.AdminId + "|" + newAudit.BasicId + "|" + newAudit.InteractionType + "|" + newAudit.DateTime;
        File.WriteAllLines(FILE_DIRECTORY, transactionsAndAuditsFileContents);
        File.AppendAllLines(FILE_DIRECTORY, new[] { recordToAdd });
    }
}
