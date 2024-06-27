namespace ATMProject.Application.Users;

public record UserModel
(
    string UserId,
    UserRole UserRole,
    string Name,
    string Address,
    string PhoneNumber,
    string Email,
    DateTime CreationDate
);
