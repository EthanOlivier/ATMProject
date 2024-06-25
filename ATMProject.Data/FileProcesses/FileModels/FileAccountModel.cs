using ATMProject.Banking;

namespace ATMProject.Data.FileProcesses.FileModels;

public record FileAccountModel
(
    string AccountId,
    string UserId,
    AccountType Type,
    double Balance,
    DateTime CreationDate,
    List<string> Transactions
);
