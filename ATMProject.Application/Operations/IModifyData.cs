﻿using ATMProject.Application.Operations.Authorization;
using ATMProject.Application.Users;
using ATMProject.Banking;
using ATMProject.System;

namespace ATMProject.Application.Operations;
public interface IBasicOperationRepository : IDepositToAccountOperation, IWithdrawFromAccountOperation, ITransferBetweenAccountsOperation, IChangeUserPasswordOperation;
public interface IDepositToAccountOperation : IOperation<IDepositToAccountOperation.Request, IResult>
{
    public record Request(string AccountId, double Amount);
}
public interface IWithdrawFromAccountOperation : IOperation<IWithdrawFromAccountOperation.Request, IResult>
{
    public record Request(string AccountId, double Amount);
}
public interface ITransferBetweenAccountsOperation : IOperation<ITransferBetweenAccountsOperation.Request, IResult>
{
    public record Request(string WithdrawalAccountId, string DepositAccountId, double Amount);
}

public interface IChangeUserPasswordOperation : IOperation<IChangeUserPasswordOperation.Request, IResult>
{
    public record Request(UserContext UserContext, string NewPassword);
}



public interface IAdminOperationsRepository : IFindUser, IGetUserIdentifyInfo, IGetAudits, IGetUsersTotals, ICreateUserId, ILookupUser, IChangeBasicUserPassword, IAddUser, IAddAccount, IDeleteUser, IDeleteAccount;

[RequiresAdmin]
public interface IFindUser
{
    bool DoesUserExist(string userId);
}
[RequiresAdmin]
public interface IGetUserIdentifyInfo
{
    (string, string, string, string, string, string) GetUserIdentifyInfo(string userId);
}
[RequiresAdmin]
public interface IGetAudits
{
    List<string> GetAudits(string userId);
}
[RequiresAdmin]
public interface IGetUsersTotals
{
    int GetTotalUsers();
    int GetTotalAccounts();
    double GetTotalBalance();
}
[RequiresAdmin]
public interface ICreateUserId
{
    string CreateUserId();
}
[RequiresAdmin]
public interface ILookupUser
{
    string[] LookupUserInfo(IdentityFields field, string input, string userId);
}
[RequiresAdmin]
public interface IChangeBasicUserPassword : IOperation<IChangeBasicUserPassword.Request, IResult>
{
    public record Request(string UserId, string Password, string AdminId);
}
[RequiresAdmin]
public interface IAddUser : IOperation<IAddUser.Request, IResult>
{
    public record Request(string Name, string Address, string PhoneNumber, string Email, string Salt, string Hash, string UserId, string AdminId);
}
[RequiresAdmin]
public interface IAddAccount : IOperation<IAddAccount.Request, IResult>
{
    public record Request(string UserId, AccountType AccountType, double Balance, string AdminId);
}
[RequiresAdmin]
public interface IDeleteUser : IOperation<IDeleteUser.Request, IResult>
{
    public record Request(string UserId, string AdminId);
}
[RequiresAdmin]
public interface IDeleteAccount : IOperation<IDeleteAccount.Request, IResult>
{
    public record Request(string AccountId, string AdminId);
}
