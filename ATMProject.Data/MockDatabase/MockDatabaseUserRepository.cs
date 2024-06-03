using ATMProject.Application;
using ATMProject.Application.Users;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ATMProject.Data.MockDatabase.MockDatabase;

public class MockDatabaseUserRepository : IDataSource
{
    public UserModel GetUserInfoByUserId(string userId)
    {
        MockDatabaseUserModel bUser = MockDatabaseFileRead.Users.Where(user => user.UserId == userId).FirstOrDefault()!;

        if (bUser is null)
        {
            throw new Exception("Error: Could not find user with provided User Id");
        }

        return new UserModel(
                UserId: bUser.UserId,
                UserRole: bUser.UserRole,
                Name: bUser.Name,
                Address: bUser.Address,
                PhoneNumber: bUser.PhoneNumber,
                Email: bUser.Email,
                CreationDate: bUser.CreationDate,
                AccountIds: bUser.AccountIds
        );
    }
    public IEnumerable<AccountInfo> GetAccountsByUserId(string userId)
    {
        MockDatabaseUserModel user = MockDatabaseFileRead.Users.Where(acct => acct.UserId == userId).FirstOrDefault()!;

        if (user is null)
        {
            throw new Exception("Error: Could not find information with provided User Id");
        }

        var dbAccounts = MockDatabaseFileRead.Accounts.Where(acct => user.AccountIds.Contains(acct.AccountId));

        if (dbAccounts is null)
        {
            throw new Exception($"Could not find any account for User with Id: " + user.UserId);
        }

        return dbAccounts.Select(account => new AccountInfo(
            AccountId: account.AccountId,
            UserId: account.UserId,
            Type: account.Type,
            Balance: account.Balance,
            CreationDate: account.CreationDate,
            TransactionIds: account.Transactions
        ));
    }

    public IEnumerable<TransactionModel> GetTransactionsByAccountIds(string[] accountIds)
    {
        var transactions = from transaction in MockDatabaseFileRead.Transactions
                           join accountId in accountIds on transaction.AccountId equals accountId
                           select transaction;

        if (transactions is null)
        {
            throw new Exception("Error: Could not find information with provided Account Ids");
        }

        return transactions.Select(transaction => new TransactionModel(
            TransactionId: transaction.TranasctionId,
            AccountId: transaction.AccountId,
            Type: transaction.Type,
            Amount: transaction.Amount,
            PreviousBalance: transaction.PreviousBalance,
            NewBalance: transaction.NewBalance,
            DateTime: transaction.DateTime
        ));
    }

    public static string CreateHash(string salt, string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(salt + password));
            string strHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
            return strHash;
        }
    }

    public bool AreUserCredentialsCorrect(string userId, string password)
    {
        MockDatabaseUserModel user = MockDatabaseFileRead.Users.Where(acct => acct.UserId == userId).FirstOrDefault()!;

        if (user != null)
        {
            if (CreateHash(user.Salt, password) == user.Hash)
            {
                return true;
            }
        }
        return false;
    }
}
