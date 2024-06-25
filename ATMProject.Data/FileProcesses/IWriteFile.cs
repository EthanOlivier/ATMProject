using ATMProject.Data.FileProcesses.FileModels;

namespace ATMProject.Data.FileProcesses;
public interface IWriteFile
{
    void UpdateUsersFile(string givenUserId, FileUserModel newUser);
    void UpdateAccountsFile(string[] givenAccountIds, FileAccountModel newAccount);
    void UpdateTransactionsFile(FileTransactionModel newTransaction, string[] accountIds);
    void UpdateAuditsFile(FileAuditModel newAudit);
}