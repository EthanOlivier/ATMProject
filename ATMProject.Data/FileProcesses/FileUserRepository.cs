using ATMProject.Application;
using ATMProject.Application.Users;
using ATMProject.Data.FileProcesses;
using ATMProject.Data.FileProcesses.FileModels;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace ATMProject.Data.MockDatabase.MockDatabase;

public class FileUserRepository : IDataSource
{
    private readonly HashSet<FileUserModel> _users;
    private readonly HashSet<FileAccountModel> _accounts;
    private readonly HashSet<FileTransactionModel> _transactions;
    public FileUserRepository(IDataStoreService<FileUserModel> users, IDataStoreService<FileAccountModel> accounts, IDataStoreService<FileTransactionModel> transactions)
    {
        _users = users.GetModels();
        _accounts = accounts.GetModels();
        _transactions = transactions.GetModels();
    }
    public UserModel GetUserInfoByUserId(string userId)
    {
        FileUserModel bUser = _users.Where(user => user.UserId == userId).FirstOrDefault()!;

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
                CreationDate: bUser.CreationDate
        );
    }
    public IEnumerable<AccountModel> GetAccountsByUserId(string userId)
    {
        var dbAccounts = _accounts.Where(acct => acct.UserId == userId).ToArray();

        if (dbAccounts is null)
        {
            return null;
        }

        var dbTransactions = _transactions.Where(tran => dbAccounts.Any(acct => acct.AccountId == tran.AccountId)).ToArray();

        return dbAccounts.Select(account => new AccountModel(
            AccountId: account.AccountId,
            UserId: account.UserId,
            Type: account.Type,
            Balance: account.Balance,
            CreationDate: account.CreationDate,
            TransactionIds: dbTransactions.Select(tran => new AccountModel.TransactionModel(
                TransactionId: tran.TranasctionId,
                AccountId: tran.AccountId,
                Type: tran.Type,
                Amount: tran.Amount,
                PreviousBalance: tran.PreviousBalance,
                NewBalance: tran.NewBalance,
                DateTime: tran.DateTime
            )).ToList()
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
        FileUserModel user = _users.Where(user => user.UserId == userId).FirstOrDefault()!;

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
