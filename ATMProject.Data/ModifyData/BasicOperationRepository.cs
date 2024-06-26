using ATMProject.Application.Operations;
using ATMProject.Banking;
using ATMProject.Data.FileProcesses;
using ATMProject.Data.FileProcesses.FileModels;
using ATMProject.Data.MockDatabase.MockDatabase;
using ATMProject.System;

namespace ATMProject.Data.ModifyData;
public class BasicOperationRepository : IBasicOperationRepository
{
    private readonly IWriteFile _writeToFile;
    private readonly HashSet<FileUserModel> _users;
    private readonly HashSet<FileAccountModel> _accounts;
    private readonly HashSet<FileTransactionModel> _transactions;
    public BasicOperationRepository(IWriteFile writeToFile, IDataStoreService<FileUserModel> users, IDataStoreService<FileAccountModel> accounts, IDataStoreService<FileTransactionModel> transactions)
    {
        _writeToFile = writeToFile;
        _users = users.GetModels();
        _accounts = accounts.GetModels();
        _transactions = transactions.GetModels();
    }
    public IResult Execute(IDepositToAccountOperation.Request request)
    {
        double previousBalance;
        double newBalance;

        FileAccountModel account = _accounts.Where(acct => acct.AccountId == request.AccountId).FirstOrDefault()!;

        if (account is null)
        {
            return Result.Failed($"Could not find any Account with Id: " + request.AccountId);
        }

        previousBalance = account.Balance;
        newBalance = previousBalance + (double)request.Amount;

        string transacitonId = CreateTransactionId();
        account.Transactions.Add(transacitonId);

        FileAccountModel newAccount = new FileAccountModel(account.AccountId, account.UserId, account.Type, newBalance, account.CreationDate, account.Transactions);
        FileTransactionModel newTransaction = new FileTransactionModel(transacitonId, request.AccountId, TransactionType.Deposit, (double)request.Amount, previousBalance, newBalance, DateTime.Now);

        _writeToFile.UpdateAccountsFile(new[] { request.AccountId }, newAccount);
        _writeToFile.UpdateTransactionsFile(newTransaction, null);

        return Result.Succeeded();
    }

    public IResult Execute(IWithdrawFromAccountOperation.Request request)
    {
        double previousBalance;
        double newBalance;

        FileAccountModel account = _accounts.Where(acct => acct.AccountId == request.AccountId).FirstOrDefault()!;

        if (account is null)
        {
            return Result.Failed($"Could not find any Account with Id: " + request.AccountId);
        }

        previousBalance = account.Balance;
        newBalance = previousBalance - (double)request.Amount;

        string transacitonId = CreateTransactionId();
        account.Transactions.Add(transacitonId);

        FileAccountModel newAccount = new FileAccountModel(account.AccountId, account.UserId, account.Type, newBalance, account.CreationDate, account.Transactions);
        FileTransactionModel newTransaction = new FileTransactionModel(transacitonId, request.AccountId, TransactionType.Withdrawal, (double)request.Amount, previousBalance, newBalance, DateTime.Now);

        _writeToFile.UpdateAccountsFile(new[] { request.AccountId }, newAccount);
        _writeToFile.UpdateTransactionsFile(newTransaction, null);

        return Result.Succeeded();
    }
    public IResult Execute(ITransferBetweenAccountsOperation.Request request)
    {
        double previousBalance;
        double newBalance;

        FileAccountModel withdrawalAccount = _accounts.Where(acct => acct.AccountId == request.WithdrawalAccountId).FirstOrDefault()!;

        if (withdrawalAccount is null)
        {
            return Result.Failed($"Could not find any Account with Id: " + request.WithdrawalAccountId);
        }

        previousBalance = withdrawalAccount.Balance;
        newBalance = previousBalance - request.Amount;

        string withdrawalTransactionId = CreateTransactionId();
        withdrawalAccount.Transactions.Add(withdrawalTransactionId);

        FileAccountModel newWithdrawalAccount = new FileAccountModel(withdrawalAccount.AccountId, withdrawalAccount.UserId, withdrawalAccount.Type, newBalance, withdrawalAccount.CreationDate, withdrawalAccount.Transactions);
        FileTransactionModel newWithdrawalTransaction = new FileTransactionModel(withdrawalTransactionId, request.WithdrawalAccountId, TransactionType.Transaction, request.Amount, previousBalance, newBalance, DateTime.Now);

        _writeToFile.UpdateAccountsFile(new[] { request.WithdrawalAccountId }, newWithdrawalAccount);
        _writeToFile.UpdateTransactionsFile(newWithdrawalTransaction, null);



        FileAccountModel depositAccount = _accounts.Where(acct => acct.AccountId == request.DepositAccountId).FirstOrDefault()!;

        if (depositAccount is null)
        {
            throw new Exception($"Could not find any Account with Id: " + request.DepositAccountId);
        }

        previousBalance = depositAccount.Balance;
        newBalance = previousBalance + request.Amount;

        string depositTransactionId = CreateTransactionId();
        depositAccount.Transactions.Add(depositTransactionId);

        FileAccountModel newDepositAccount = new FileAccountModel(depositAccount.AccountId, depositAccount.UserId, depositAccount.Type, newBalance, depositAccount.CreationDate, depositAccount.Transactions);
        FileTransactionModel newDepsitTransaction = new FileTransactionModel(depositTransactionId, request.WithdrawalAccountId, TransactionType.Transaction, request.Amount, previousBalance, newBalance, DateTime.Now);

        _writeToFile.UpdateAccountsFile(new[] { request.DepositAccountId }, newDepositAccount);
        _writeToFile.UpdateTransactionsFile(newDepsitTransaction, null);

        return Result.Succeeded();
    }
    public IResult Execute(IChangeUserPasswordOperation.Request request)
    {
        string newSalt = Guid.NewGuid().ToString();
        string newHash = FileUserRepository.CreateHash(newSalt, request.NewPassword);

        FileUserModel oldUser = _users.Where(user => user.UserId == request.UserContext.UserId).FirstOrDefault()!;

        if (oldUser is null)
        {
            return Result.Failed($"Could not find any User with Id: " + request.UserContext.UserId);
        }

        FileUserModel newUser = new FileUserModel(oldUser.UserId, newHash, newSalt, oldUser.UserRole, oldUser.Name, oldUser.Address, oldUser.PhoneNumber, oldUser.Email, oldUser.CreationDate, oldUser.AccountIds);

        _writeToFile.UpdateUsersFile(request.UserContext.UserId, newUser);

        return Result.Succeeded();
    }
    private string CreateTransactionId()
    {
        string transactionId;
        do
        {
            Random random = new Random();
            transactionId = random.Next(10000, 100000).ToString();
        } while (_transactions.Where(trnsct => trnsct.TranasctionId == transactionId).FirstOrDefault() != null);
        return transactionId;
    }
}
