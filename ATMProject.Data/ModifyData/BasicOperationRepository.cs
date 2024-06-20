using ATMProject.Application;
using ATMProject.Application.Operations;
using ATMProject.Banking;
using ATMProject.Data.MockDatabase;
using ATMProject.Data.MockDatabase.MockDatabase;
using ATMProject.System;

namespace ATMProject.Data.ModifyData;
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
    public IResult Execute(IDepositToAccountOperation.Request request)
    {
        double previousBalance;
        double newBalance;

        MockDatabaseAccountModel account = MockDatabaseFileRead.Accounts.Where(acct => acct.AccountId == request.AccountId).FirstOrDefault()!;

        if (account is null)
        {
            return Result.Failed($"Could not find any Account with Id: " + request.AccountId);
        }

        previousBalance = account.Balance;
        newBalance = previousBalance + (double)request.Amount;

        string transacitonId = CreateTransactionId();
        account.Transactions.Add(transacitonId);

        MockDatabaseAccountModel newAccount = new MockDatabaseAccountModel(account.AccountId, account.UserId, account.Type, newBalance, account.CreationDate, account.Transactions);
        MockDatabaseTransactionModel newTransaction = new MockDatabaseTransactionModel(transacitonId, request.AccountId, TransactionType.Deposit, (double)request.Amount, previousBalance, newBalance, DateTime.Now);

        _writeToFile.UpdateAccountsFile(new[] { request.AccountId }, newAccount);
        _writeToFile.UpdateTransactionsFile(newTransaction, null);

        return Result.Succeeded();
    }

    public IResult Execute(IWithdrawFromAccountOperation.Request request)
    {
        double previousBalance;
        double newBalance;

        MockDatabaseAccountModel account = MockDatabaseFileRead.Accounts.Where(acct => acct.AccountId == request.AccountId).FirstOrDefault()!;

        if (account is null)
        {
            return Result.Failed($"Could not find any Account with Id: " + request.AccountId);
        }

        previousBalance = account.Balance;
        newBalance = previousBalance - (double)request.Amount;

        string transacitonId = CreateTransactionId();
        account.Transactions.Add(transacitonId);

        MockDatabaseAccountModel newAccount = new MockDatabaseAccountModel(account.AccountId, account.UserId, account.Type, newBalance, account.CreationDate, account.Transactions);
        MockDatabaseTransactionModel newTransaction = new MockDatabaseTransactionModel(transacitonId, request.AccountId, TransactionType.Withdrawal, (double)request.Amount, previousBalance, newBalance, DateTime.Now);

        _writeToFile.UpdateAccountsFile(new[] { request.AccountId }, newAccount);
        _writeToFile.UpdateTransactionsFile(newTransaction, null);

        return Result.Succeeded();
    }
    public IResult Execute(ITransferBetweenAccountsOperation.Request request)
    {
        double previousBalance;
        double newBalance;

        HashSet<MockDatabaseAccountModel> accounts = MockDatabaseFileRead.Accounts;

        MockDatabaseAccountModel withdrawalAccount = accounts.Where(acct => acct.AccountId == request.WithdrawalAccountId).FirstOrDefault()!;

        if (withdrawalAccount is null)
        {
            return Result.Failed($"Could not find any Account with Id: " + request.WithdrawalAccountId);
        }

        previousBalance = withdrawalAccount.Balance;
        newBalance = previousBalance - request.Amount;

        string withdrawalTransactionId = CreateTransactionId();
        withdrawalAccount.Transactions.Add(withdrawalTransactionId);

        MockDatabaseAccountModel newWithdrawalAccount = new MockDatabaseAccountModel(withdrawalAccount.AccountId, withdrawalAccount.UserId, withdrawalAccount.Type, newBalance, withdrawalAccount.CreationDate, withdrawalAccount.Transactions);
        MockDatabaseTransactionModel newWithdrawalTransaction = new MockDatabaseTransactionModel(withdrawalTransactionId, request.WithdrawalAccountId, TransactionType.Transaction, request.Amount, previousBalance, newBalance, DateTime.Now);

        _writeToFile.UpdateAccountsFile(new[] { request.WithdrawalAccountId }, newWithdrawalAccount);
        _writeToFile.UpdateTransactionsFile(newWithdrawalTransaction, null);



        MockDatabaseAccountModel depositAccount = accounts.Where(acct => acct.AccountId == request.DepositAccountId).FirstOrDefault()!;
        if (depositAccount is null)
        {
            throw new Exception($"Could not find any Account with Id: " + request.DepositAccountId);
        }

        previousBalance = depositAccount.Balance;
        newBalance = previousBalance + request.Amount;

        string depositTransactionId = CreateTransactionId();
        depositAccount.Transactions.Add(depositTransactionId);

        MockDatabaseAccountModel newDepositAccount = new MockDatabaseAccountModel(depositAccount.AccountId, depositAccount.UserId, depositAccount.Type, newBalance, depositAccount.CreationDate, depositAccount.Transactions);
        MockDatabaseTransactionModel newDepsitTransaction = new MockDatabaseTransactionModel(depositTransactionId, request.WithdrawalAccountId, TransactionType.Transaction, request.Amount, previousBalance, newBalance, DateTime.Now);

        _writeToFile.UpdateAccountsFile(new[] { request.DepositAccountId }, newDepositAccount);
        _writeToFile.UpdateTransactionsFile(newDepsitTransaction, null);

        return Result.Succeeded();
    }
    public IResult Execute(IChangeUserPasswordOperation.Request request)
    {
        string newSalt = Guid.NewGuid().ToString();
        string newHash = MockDatabaseUserRepository.CreateHash(newSalt, request.NewPassword);

        MockDatabaseUserModel oldUser = MockDatabaseFileRead.Users.Where(user => user.UserId == request.UserContext.UserId).FirstOrDefault()!;

        if (oldUser is null)
        {
            return Result.Failed($"Could not find any User with Id: " + request.UserContext.UserId);
        }

        MockDatabaseUserModel newUser = new MockDatabaseUserModel(oldUser.UserId, newHash, newSalt, oldUser.UserRole, oldUser.Name, oldUser.Address, oldUser.PhoneNumber, oldUser.Email, oldUser.CreationDate, oldUser.AccountIds);

        _writeToFile.UpdateUsersFile(request.UserContext.UserId, newUser);

        return Result.Succeeded();
    }
}
