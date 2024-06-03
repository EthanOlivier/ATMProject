using ATMProject.Banking;

namespace ATMProject.Application.Users;

public record AccountModel
(
    string AccountId,
    string UserId,
    AccountModel Type, 
    double Balance, 
    DateTime CreationDate,
    List<string> TransactionIds
);
