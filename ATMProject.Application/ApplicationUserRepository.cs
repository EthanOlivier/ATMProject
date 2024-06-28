using ATMProject.Application.Users;
using ATMProject.System;

namespace ATMProject.Application;
public class ApplicationUserRepository : IUserRepository
{
    private readonly IDataSource _dataSource;

    public ApplicationUserRepository(IDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public IEnumerable<AccountModel> GetUserAccountsByUserId(string userId)
    {
        return _dataSource.GetAccountsByUserId(userId);
    }

    public UserModel GetUserInfoByUserId(string userId)
    {
        return _dataSource.GetUserInfoByUserId(userId);
    }

    public Result<UserContext> LoginUser(string userId, string password)
    {
        if (_dataSource.AreUserCredentialsCorrect(userId, password))
        {
            return Result<UserContext>
                .Succeeded(new UserContext(
                    userId, _dataSource.GetUserInfoByUserId(userId).UserRole)
                );
        }
        else
        {
            return Result<UserContext>
                .Failed("Incorrect User ID or Password entered");
        }
    }
}
