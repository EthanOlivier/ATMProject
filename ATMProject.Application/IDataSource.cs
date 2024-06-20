using ATMProject.Application.Users;

namespace ATMProject.Application;
public interface IDataSource
{
    bool AreUserCredentialsCorrect(string userId, string password);
    UserModel GetUserInfoByUserId(string userId);
    IEnumerable<AccountModel> GetAccountsByUserId(string userId);
}