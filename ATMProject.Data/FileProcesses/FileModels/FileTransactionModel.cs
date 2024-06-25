using ATMProject.Banking;

namespace ATMProject.Data.FileProcesses.FileModels;
public record FileTransactionModel
(
    string TranasctionId,
    string AccountId,
    TransactionType Type,
    double Amount,
    double PreviousBalance,
    double NewBalance,
    DateTime DateTime
);