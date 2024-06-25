﻿using ATMProject.Application.Operations;
using ATMProject.Application.Users;
using ATMProject.Data.FileProcesses;
using ATMProject.Data.FileProcesses.FileModels;
using ATMProject.Data.MockDatabase;
using ATMProject.Data.MockDatabase.MockDatabase;
using ATMProject.System;

namespace ATMProject.Data.ModifyData;
public class AdminOperationsRepository : IAdminOperationsRepository
{
    private readonly IWriteFile _writeToFile;
    public AdminOperationsRepository(IWriteFile writeToFile)
    {
        _writeToFile = writeToFile;
    }
    public IResult Execute(IChangeBasicUserPassword.Request request)
    {
        FileUserModel oldUser = FileRead.Users.Where(user => user.UserId == request.UserId).FirstOrDefault()!;

        if (oldUser is null)
        {
            return Result.Failed($"Unable to find user with Id: {request.UserId}");
        }


        string newSalt = Guid.NewGuid().ToString();
        string newHash = FileUserRepository.CreateHash(newSalt, request.Password);

        FileUserModel newUser = new FileUserModel(oldUser.UserId, newHash, newSalt, oldUser.UserRole, oldUser.Name, oldUser.Address, oldUser.PhoneNumber, oldUser.Email, oldUser.CreationDate, oldUser.AccountIds);

        _writeToFile.UpdateUsersFile(request.UserId, newUser);

        AddAudit(request.AdminId, AdminInteraction.ResetUserPassword, request.UserId, DateTime.Now);

        return Result.Succeeded();
    }
    public IResult Execute(IAddUser.Request request)
    {
        FileUserModel newUser = new FileUserModel(request.UserId, request.Hash, request.Salt, UserRole.Basic, request.Name, request.Address, request.PhoneNumber, request.Email, DateTime.Now, new List<string> { });

        _writeToFile.UpdateUsersFile(null, newUser);

        AddAudit(request.AdminId, AdminInteraction.AddUser, request.UserId, DateTime.Now);

        return Result.Succeeded();
    }
    public IResult Execute(IAddAccount.Request request)
    {
        string accountId = "";
        do
        {
            Random random = new Random();
            accountId = random.Next(10000, 100000).ToString();
        } while (FileRead.Accounts.Where(acct => acct.AccountId == accountId).FirstOrDefault() != null);

        FileAccountModel newAccount = new FileAccountModel(accountId, request.UserId, request.AccountType, request.Balance, DateTime.Now, null);

        _writeToFile.UpdateAccountsFile(null, newAccount);

        FileUserModel user = FileRead.Users.Where(user => user.UserId == request.UserId).FirstOrDefault()!;

        user.AccountIds.Add(accountId);

        _writeToFile.UpdateUsersFile(request.UserId, user);

        AddAudit(request.AdminId, AdminInteraction.AddAccount, request.UserId, DateTime.Now);

        return Result.Succeeded();
    }
    public IResult Execute(IDeleteUser.Request request)
    {
        FileUserModel user = FileRead.Users.Where(user => user.UserId == request.UserId).FirstOrDefault()!;

        if (user is null)
        {
            return Result.Failed($"Unable to find user with Id: {request.UserId}");
        }

        _writeToFile.UpdateUsersFile(request.UserId, null);
        _writeToFile.UpdateAccountsFile(user.AccountIds.ToArray(), null);
        _writeToFile.UpdateTransactionsFile(null, user.AccountIds.ToArray());

        AddAudit(request.AdminId, AdminInteraction.DeleteUser, request.UserId, DateTime.Now);

        return Result.Succeeded();
    }
    public IResult Execute(IDeleteAccount.Request request)
    {
        FileUserModel user = FileRead.Users.Where(user => user.AccountIds.Contains(request.AccountId)).FirstOrDefault()!;

        user.AccountIds.Remove(request.AccountId);

        _writeToFile.UpdateUsersFile(user.UserId, user);
        _writeToFile.UpdateAccountsFile(new[] { request.AccountId }, null);
        _writeToFile.UpdateTransactionsFile(null, new[] { request.AccountId });

        AddAudit(request.AdminId, AdminInteraction.DeleteAccount, request.AccountId, DateTime.Now);

        return Result.Succeeded();
    }
    public List<string> GetAudits(string userId)
    {
        List<string> audits = new List<string>();
        IEnumerable<FileAuditModel> dbAudits = FileRead.Audits.Where(audit => audit.AdminId == userId);
        foreach (var audit in dbAudits)
        {
            switch (audit.InteractionType)
            {
                case AdminInteraction.LookupUserId:
                    audits.Add($"Looked for User with field {audit.BasicId} on {audit.DateTime}");
                    break;
                case AdminInteraction.ResetUserPassword:
                    audits.Add($"Reset Password of User with Id {audit.BasicId} on {audit.DateTime}");
                    break;
                case AdminInteraction.AddUser:
                    audits.Add($"Added User with Id {audit.BasicId} on {audit.DateTime}");
                    break;
                case AdminInteraction.DeleteUser:
                    audits.Add($"Removed User with former Id {audit.BasicId} on {audit.DateTime}");
                    break;
                case AdminInteraction.AddAccount:
                    audits.Add($"Added an Account to User with Id {audit.BasicId} on {audit.DateTime}");
                    break;
                case AdminInteraction.DeleteAccount:
                    audits.Add($"Removed Account with former Id {audit.BasicId} on {audit.DateTime}");
                    break;
                default:
                    throw new Exception($"Admin Interaction {audit.InteractionType} not valid");
            }
        }
        return audits;
    }
    public string[] LookupUserInfo(IdentityFields field, string input, string userId)
    {
        var allUsers = FileRead.Users;
        IEnumerable<FileUserModel> foundUsers;
        List<string> users = new List<string>();

        switch (field)
        {
            case IdentityFields.Name:
                foundUsers = allUsers.Where(user => user.Name == input);
                break;
            case IdentityFields.Address:
                foundUsers = allUsers.Where(user => user.Address == input);
                break;
            case IdentityFields.PhoneNumber:
                foundUsers = allUsers.Where(user => user.PhoneNumber == input);
                break;
            case IdentityFields.Email:
                foundUsers = allUsers.Where(user => user.Email == input);
                break;
            default:
                throw new Exception("Incorrect Identity Field entered");
        }

        foreach (FileUserModel user in foundUsers)
        {
            users.Add(user.UserId);
        }

        if (users.Count() != 0)
        {
            AddAudit(userId, AdminInteraction.LookupUserId, input, DateTime.Now);
        }

        return users.ToArray();
    }
    private string CreateAuditId()
    {
        string auditId;
        do
        {
            Random random = new Random();
            auditId = random.Next(10000, 100000).ToString();
        } while (FileRead.Audits.Where(audit => audit.AuditId == auditId).FirstOrDefault() != null);
        return auditId;
    }
    public bool DoesUserExist(string userId)
    {
        FileUserModel user = FileRead.Users.Where(user => user.UserId == userId).FirstOrDefault()!;

        if (user is null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public (string, string, string, string, string, string) GetUserIdentifyInfo(string userId)
    {
        FileUserModel user = FileRead.Users.Where(user => user.UserId == userId).FirstOrDefault()!;

        if (user is null)
        {
            throw new Exception($"User with with UserId {userId} could not be found");
        }

        return (user.Name, user.Address, user.PhoneNumber, user.Email, user.Salt, user.Hash);
    }
    public int GetTotalUsers()
    {
        return FileRead.Users.Count();
    }
    public int GetTotalAccounts()
    {
        return FileRead.Accounts.Count();
    }
    public double GetTotalBalance()
    {
        return FileRead.Accounts.Select(acct => acct.Balance).Sum();
    }
    public string CreateUserId()
    {
        return (FileRead.Users.Count() + 1).ToString();
    }

    private void AddAudit(string adminId, AdminInteraction interaction, string userId, DateTime dateTime)
    {
        FileAuditModel newAudit = new FileAuditModel(CreateAuditId(), adminId, userId, interaction, dateTime);

        FileRead.Audits.Add(newAudit);

        _writeToFile.UpdateAuditsFile(newAudit);
    }
}
