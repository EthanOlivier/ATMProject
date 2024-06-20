using ATMProject.Banking;

namespace ATMProject.Application.Users;

public record AccountModel
(
    string AccountId,
    string UserId,
    AccountType Type,
    double Balance,
    DateTime CreationDate,
    List<AccountModel.TransactionModel> TransactionIds
)
{
    public record TransactionModel
    (
        string TransactionId,
        string AccountId,
        TransactionType Type,
        double Amount,
        double PreviousBalance,
        double NewBalance,
        DateTime DateTime
    );
};
