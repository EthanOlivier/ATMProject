using ATMProject.Banking;

namespace ATMProject.Application;

public interface IUserContextService
{
    bool IsLoggedIn { get; }

    void SetUserContext(UserContext userContext);
    void Logout();
    UserContext GetUserContext();
}
