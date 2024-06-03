using ATMProject.Banking;

namespace ATMProject.Data.MockDatabase;

public record MockDatabaseAccountModel
(
    string AccountId,
    string UserId,
    AccountType Type,
    double Balance,
    DateTime CreationDate,
    List<string> Transactions
);
