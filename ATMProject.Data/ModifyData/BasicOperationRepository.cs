using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Banking;
using ATMProject.Data.MockDatabase;
using ATMProject.Data.MockDatabase.MockDatabase;
using ATMProject.System;

namespace ATMProject.Data.ModifyData
{
    public class BasicOperationRepository : IBasicOperationRepository
    {
        private readonly IWriteToFile _writeToFile;
        public BasicOperationRepository(IWriteToFile writeToFile)
        {
            _writeToFile = writeToFile;
        }
        private string CreateTransactionId()
        {
            string transactionId;
            do
            {
                Random random = new Random();
                transactionId = random.Next(10000, 100000).ToString();
            } while (MockDatabaseFileRead.Transactions.Where(trnsct => trnsct.TranasctionId == transactionId).FirstOrDefault() != null);
            return transactionId;
        }
        public void TransferBetweenAccounts(string withdrawalAccountId, string depositAccountId, double amount)
        {
            double previousBalance;
            double newBalance;

            HashSet<MockDatabaseAccountModel> accounts = MockDatabaseFileRead.Accounts;

            MockDatabaseAccountModel withdrawalAccount = accounts.Where(acct => acct.AccountId == withdrawalAccountId).FirstOrDefault()!;

            if (withdrawalAccount is null)
            {
                throw new Exception($"Could not find any Account with Id: " + withdrawalAccountId);
            }

            previousBalance = withdrawalAccount.Balance;
            newBalance = previousBalance - amount;

            string withdrawalTransactionId = CreateTransactionId();
            withdrawalAccount.Transactions.Add(withdrawalTransactionId);

            MockDatabaseAccountModel newWithdrawalAccount = new MockDatabaseAccountModel(withdrawalAccount.AccountId, withdrawalAccount.UserId, withdrawalAccount.Type, newBalance, withdrawalAccount.CreationDate, withdrawalAccount.Transactions);
            MockDatabaseTransactionModel newWithdrawalTransaction = new MockDatabaseTransactionModel(withdrawalTransactionId, withdrawalAccountId, TransactionType.Transaction, amount, previousBalance, newBalance, DateTime.Now);

            MockDatabaseFileRead.Accounts.Remove(withdrawalAccount);
            MockDatabaseFileRead.Accounts.Add(newWithdrawalAccount);
            MockDatabaseFileRead.Transactions.Add(newWithdrawalTransaction);

            _writeToFile.UpdateAccountsFile(withdrawalAccountId, newWithdrawalAccount);
            _writeToFile.UpdateTransactionsAndAuditsFile(newWithdrawalTransaction, null, null);



            MockDatabaseAccountModel depositAccount = accounts.Where(acct => acct.AccountId == depositAccountId).FirstOrDefault()!;
            if (depositAccount is null)
            {
                throw new Exception($"Could not find any Account with Id: " + depositAccountId);
            }

            previousBalance = depositAccount.Balance;
            newBalance = previousBalance + amount;

            string depositTransactionId = CreateTransactionId();
            depositAccount.Transactions.Add(depositTransactionId);

            MockDatabaseAccountModel newDepositAccount = new MockDatabaseAccountModel(depositAccount.AccountId, depositAccount.UserId, depositAccount.Type, newBalance, depositAccount.CreationDate, depositAccount.Transactions);
            MockDatabaseTransactionModel newDepsitTransaction = new MockDatabaseTransactionModel(depositTransactionId, depositAccountId, TransactionType.Transaction, amount, previousBalance, newBalance, DateTime.Now);

            MockDatabaseFileRead.Accounts.Remove(depositAccount);
            MockDatabaseFileRead.Accounts.Add(newDepositAccount);
            MockDatabaseFileRead.Transactions.Add(newDepsitTransaction);

            _writeToFile.UpdateAccountsFile(depositAccountId, newDepositAccount);
            _writeToFile.UpdateTransactionsAndAuditsFile(newDepsitTransaction, null, null);
        }
        public void ChangeUserPassword(UserContext userContext, string newPassword)
        {
            string newSalt = Guid.NewGuid().ToString();
            string newHash = MockDatabaseUserRepository.CreateHash(newSalt, newPassword);

            MockDatabaseUserModel oldUser = MockDatabaseFileRead.Users.Where(user => user.UserId == userContext.UserId).FirstOrDefault()!;

            if (oldUser is null)
            {
                throw new Exception($"Could not find any User with Id: " + userContext.UserId);
            }

            MockDatabaseUserModel newUser = new MockDatabaseUserModel(oldUser.UserId, newHash, newSalt, oldUser.UserRole, oldUser.Name, oldUser.Address, oldUser.PhoneNumber, oldUser.Email, oldUser.CreationDate, oldUser.AccountIds);

            MockDatabaseFileRead.Users.Remove(oldUser);
            MockDatabaseFileRead.Users.Add(newUser);

            _writeToFile.UpdateUsersFile(userContext.UserId, newUser);
        }

        public IResult Execute(IDepositToAccountOperation.Request request)
        {
            double previousBalance;
            double newBalance;

            MockDatabaseAccountModel account = MockDatabaseFileRead.Accounts.Where(acct => acct.AccountId == request.AccountId).FirstOrDefault()!;

            if (account is null)
            {
                throw new Exception($"Could not find any Account with Id: " + request.AccountId);
            }

            previousBalance = account.Balance;
            newBalance = previousBalance + (double)request.Amount;

            string transacitonId = CreateTransactionId();
            account.Transactions.Add(transacitonId);

            MockDatabaseAccountModel newAccount = new MockDatabaseAccountModel(account.AccountId, account.UserId, account.Type, newBalance, account.CreationDate, account.Transactions);
            MockDatabaseTransactionModel newTransaction = new MockDatabaseTransactionModel(transacitonId, request.AccountId, TransactionType.Deposit, (double)request.Amount, previousBalance, newBalance, DateTime.Now);

            MockDatabaseFileRead.Accounts.Remove(account);
            MockDatabaseFileRead.Accounts.Add(newAccount);
            MockDatabaseFileRead.Transactions.Add(newTransaction);

            _writeToFile.UpdateAccountsFile(request.AccountId, newAccount);
            _writeToFile.UpdateTransactionsAndAuditsFile(newTransaction, null, null);

            return Result.Succeeded();
        }

        public IResult Execute(IWithdrawFromAccountOperation.Request request)
        {
            double previousBalance;
            double newBalance;

            MockDatabaseAccountModel account = MockDatabaseFileRead.Accounts.Where(acct => acct.AccountId == request.AccountId).FirstOrDefault()!;

            if (account is null)
            {
                throw new Exception($"Could not find any Account with Id: " + request.AccountId);
            }

            previousBalance = account.Balance;
            newBalance = previousBalance - (double)request.Amount;

            string transacitonId = CreateTransactionId();
            account.Transactions.Add(transacitonId);

            MockDatabaseAccountModel newAccount = new MockDatabaseAccountModel(account.AccountId, account.UserId, account.Type, newBalance, account.CreationDate, account.Transactions);
            MockDatabaseTransactionModel newTransaction = new MockDatabaseTransactionModel(transacitonId, request.AccountId, TransactionType.Withdrawal, (double)request.Amount, previousBalance, newBalance, DateTime.Now);

            MockDatabaseFileRead.Accounts.Remove(account);
            MockDatabaseFileRead.Accounts.Add(newAccount);
            MockDatabaseFileRead.Transactions.Add(newTransaction);

            _writeToFile.UpdateAccountsFile(request.AccountId, newAccount);
            _writeToFile.UpdateTransactionsAndAuditsFile(newTransaction, null, null);

            return Result.Succeeded();
        }
    }
}
