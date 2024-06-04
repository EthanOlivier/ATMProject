using ATMProject.Application.Operations.Authorization;
using ATMProject.Application.Users;
using ATMProject.Banking;
using ATMProject.System;

namespace ATMProject.Application.Operations;
public interface IBasicOperationRepository : IDepositToAccountOperation, IWithdrawFromAccountOperation, ITransferBetweenAccountsOperation, IChangeUserPasswordOperation;
// This one was changed
public interface IDepositToAccountOperation : IOperation<IDepositToAccountOperation.Request, IResult>
{
    public record Request(string AccountId, double Amount);
}
// and this one was also changed
[RequiresAdmin]
public interface IWithdrawFromAccountOperation : IOperation<IWithdrawFromAccountOperation.Request, IResult>
{
    public record Request(string AccountId, double Amount);
}
// all below are not changed
public interface ITransferBetweenAccountsOperation
{
    void TransferBetweenAccounts(string withdrawalAccountId, string depositAccountId, double amount);
}

public interface IChangeUserPasswordOperation
{
    void ChangeUserPassword(UserContext userContext, string newPassword);
}



public interface IAdminOperationsRepository : IFindUser, IGetUserIdentifyInfo, IGetAudits, IGetUsersTotals, ICreateUserId, ILookupUserOperations, IChangeBasicUserPassword, IAddUser, IAddAccount, IDeleteUser, IDeleteAccount;
public interface IFindUser
{
    bool DoesUserExist(string userId);
}
public interface IGetUserIdentifyInfo
{
    (string, string, string, string, string, string) GetUserIdentifyInfo(string userId);
}
public interface IGetAudits
{
    List<string> GetAudits(string userId);
}
public interface IGetUsersTotals
{
    int GetTotalUsers();
    int GetTotalAccounts();
    double GetTotalBalance();
}
public interface ICreateUserId
{
    string CreateUserId();
}
public interface ILookupUserOperations
{
    string[] LookupUserInfo(IdentityFields field, string input, string userId);
}
public interface IChangeBasicUserPassword
{
    void ChangeBasicUserPassword(string userId, string password, string adminId);
}
public interface IAddUser
{
    void AddUser(string name, string address, string phoneNumber, string email, string salt, string hash, string userId, string adminId);
}
public interface IAddAccount
{
    void AddAccount(string userId, AccountType accountType, double balance, string adminId);
}
public interface IDeleteUser
{
    void DeleteUser(string userId, string adminId);
}
public interface IDeleteAccount
{
    void DeleteAccount(string accountId, string adminId);
}
