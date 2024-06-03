using ATMProject.Data.MockDatabase;
using ATMProject.Data.MockDatabase.MockDatabase;
using System.Security.Principal;

namespace ATMProject.Data.ModifyData
{
    public class MockDatabaseFileWrite : IWriteToFile
    {
        public void UpdateUsersFile(string givenUserId, MockDatabaseUserModel newUser)
        {
            const string FILE_DIRECTORY = "C:/Users/eolivieri/source/repos/MockDatabase/Users.txt";
            string[] usersFileContents = File.ReadAllLines(FILE_DIRECTORY);

            if (givenUserId is not null)
            {
                var updatedFileContent = usersFileContents.Select(line =>
                {
                    string fileUserId = line.Substring(0, line.IndexOf("|"));
                    if (fileUserId == givenUserId)
                    {
                        if (newUser is null)
                        {
                            line = null;
                        }
                        else
                        {
                            string newIds = "";
                            if (newUser.AccountIds is not null && newUser.AccountIds.Count() != 0)
                            {
                                newIds = String.Join(" ", newUser.AccountIds);
                            }
                            return (newUser.UserId + "|" + newUser.Hash + "|" + newUser.Salt + "|" + newUser.UserRole + "|" + newUser.Name + "|" + newUser.Address + "|" + newUser.PhoneNumber + "|" + newUser.Email + "|" + newUser.CreationDate + "|" + newIds).ToString();
                        }
                    }
                    return line;
                }).Where(line => line != null);

                File.WriteAllLines(FILE_DIRECTORY, updatedFileContent);
            }
            else
            {
                string newIds = "";
                if (newUser.AccountIds is not null && newUser.AccountIds.Count() != 0)
                {
                    newIds = String.Join(" ", newUser.AccountIds);
                }
                string userToAdd = (newUser.UserId + "|" + newUser.Hash + "|" + newUser.Salt + "|" + newUser.UserRole + "|" + newUser.Name + "|" + newUser.Address + "|" + newUser.PhoneNumber + "|" + newUser.Email + "|" + newUser.CreationDate + "|" + newIds).ToString();
                File.WriteAllLines(FILE_DIRECTORY, usersFileContents);
                File.AppendAllLines(FILE_DIRECTORY, new[] { userToAdd });
            }
        }
        public void UpdateAccountsFile(string givenAccountId, MockDatabaseAccountModel newAccount)
        {
            const string FILE_DIRECTORY = "C:/Users/eolivieri/source/repos/MockDatabase/Accounts.txt";
            string[] accountsFileContents = File.ReadAllLines(FILE_DIRECTORY);

            if (givenAccountId is not null)
            {
                var updatedFileContent = accountsFileContents.Select(line =>
                {
                    string fileAccountId = line.Substring(0, line.IndexOf("|"));
                    if (fileAccountId == givenAccountId)
                    {
                        if (newAccount is null)
                        {
                            line = null;
                        }
                        else
                        {
                            string newTransactions = "";
                            if (newAccount.Transactions is not null && newAccount.Transactions.Count() != 0)
                            {
                                newTransactions = String.Join(" ", newAccount.Transactions);
                            }
                            return (newAccount.AccountId + "|" + newAccount.UserId + "|" + newAccount.Type + "|" + newAccount.Balance + "|" + newAccount.CreationDate + "|" + newTransactions);
                        }
                    }
                    return line;
                }).Where(line => line != null);

                File.WriteAllLines(FILE_DIRECTORY, updatedFileContent);
            }
            else
            {
                string newTransactions = "";
                if (newAccount.Transactions is not null && newAccount.Transactions.Count() != 0)
                {
                    newTransactions = String.Join(" ", newAccount.Transactions);
                }
                string accountToAdd = (newAccount.AccountId + "|" + newAccount.UserId + "|" + newAccount.Type + "|" + newAccount.Balance + "|" + newAccount.CreationDate + "|" + newTransactions);
                File.WriteAllLines(FILE_DIRECTORY, accountsFileContents);
                File.AppendAllLines(FILE_DIRECTORY, new[] { accountToAdd });
            }
        }
        public void UpdateTransactionsAndAuditsFile(MockDatabaseTransactionModel newTransaction, MockDatabaseAuditModel newAudit, string[] accountIds)
        {
            const string FILE_DIRECTORY = "C:/Users/eolivieri/source/repos/MockDatabase/TransactionsAndAudits.txt";
            string[] transactionsAndAuditsFileContents = File.ReadAllLines(FILE_DIRECTORY);

            string recordToAdd = "";

            if (newTransaction is not null)
            {
                recordToAdd = newTransaction.TranasctionId + "|" + newTransaction.AccountId + "|" + newTransaction.Type + "|" + newTransaction.Amount + "|" + newTransaction.PreviousBalance + "|" + newTransaction.NewBalance + "|" + newTransaction.DateTime;
            }
            else if (newAudit is not null)
            {
                recordToAdd = newAudit.AuditId + "|" + newAudit.AdminId + "|" + newAudit.BasicId + "|" + newAudit.InteractionType + "|" + newAudit.DateTime;
            }
            else
            {
                var updatedFileContent = transactionsAndAuditsFileContents.Select(line =>
                {
                    string fileAccountId = line.Substring(0, line.IndexOf("|"));
                    if (accountIds.Contains(fileAccountId))
                    {
                        line = null;
                    }
                    return line;
                }).Where(line => line != null);
            }
            File.WriteAllLines(FILE_DIRECTORY, transactionsAndAuditsFileContents);
            File.AppendAllLines(FILE_DIRECTORY, new[] { recordToAdd });
        }
    }
}
