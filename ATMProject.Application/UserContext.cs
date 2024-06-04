using ATMProject.Application.Users;

namespace ATMProject.Application;
public record UserContext
(
    string UserId,
    UserRole UserRole
);
