using ATMProject.Application.Users;

namespace ATMProject.Data.MockDatabase.MockDatabase;

public record MockDatabaseUserModel
(
    string UserId,
    string Hash,
    string Salt,
    UserRole UserRole,
    string Name,
    string Address,
    string PhoneNumber,
    string Email,
    DateTime CreationDate,
    List<string> AccountIds
);
