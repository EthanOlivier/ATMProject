namespace ATMProject.Application;
public class UserContextService : IUserContextService
{
    public bool IsLoggedIn => _userContext is not null;

    private UserContext? _userContext;

    public UserContext GetUserContext()
    {
        if (_userContext is null)
        {
            throw new Exception("Cannot get UserContext; User is not logged in.");
        }
        return _userContext;
    }

    public void SetUserContext(UserContext userContext)
    {
        _userContext = userContext;
    }

    public void Logout()
    {
        if (_userContext is null)
        {
            throw new Exception("Cannot log out as the user was not logged in.");
        }
        _userContext = null;
    }
}
