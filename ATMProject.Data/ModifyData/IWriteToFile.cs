using ATMProject.Data.MockDatabase;
using ATMProject.Data.MockDatabase.MockDatabase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ATMProject.Data.ModifyData;
public interface IWriteToFile
{
    void UpdateUsersFile(string givenUserId, MockDatabaseUserModel newUser);
    void UpdateAccountsFile(string givenAccountId, MockDatabaseAccountModel newAccount);
    void UpdateTransactionsAndAuditsFile(MockDatabaseTransactionModel newTransaction, MockDatabaseAuditModel newAudit, string[] accountIds);
}