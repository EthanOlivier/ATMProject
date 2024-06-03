using ATMProject.System;
using ATMProject.Application.Users;

namespace ATMProject.Application;

public interface IUserRepository
{
    Result<UserContext> LoginUser(string userId, string password);
    UserModel GetUserInfoByUserId(string userId);
    IEnumerable<AccountModel> GetUserAccountsByUserId(string userId);
    IEnumerable<TransactionModel> GetAccountTransactionsByAccountIds(string[] accountIds);
}
