using ATMProject.Banking;

namespace ATMProject.Data.MockDatabase
{
    public record MockDatabaseTransactionModel
    (
        string TranasctionId,
        string AccountId,
        TransactionType Type,
        double Amount,
        double PreviousBalance,
        double NewBalance,
        DateTime DateTime
    );
}
