using ATMProject.Banking;
namespace ATMProject.Application.Users
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
}

