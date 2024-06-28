using ATMProject.Application.Operations;
using ATMProject.Application.Users;
using ATMProject.Data.FileProcesses;
using ATMProject.Data.FileProcesses.FileModels;
using ATMProject.Data.MockDatabase.MockDatabase;
using ATMProject.System;

namespace ATMProject.Data.ModifyData;
public class AdminOperationsRepository : IAdminOperationsRepository
{
    private readonly IWriteFile _writeToFile;
    private readonly HashSet<FileUserModel> _users;
    private readonly HashSet<FileAccountModel> _accounts;
    private readonly HashSet<FileTransactionModel> _transactions;
    private readonly HashSet<FileAuditModel> _audits;

    public AdminOperationsRepository
    (
        IWriteFile writeToFile, 
        IDataStoreService<FileUserModel> users, 
        IDataStoreService<FileAccountModel> accounts, 
        IDataStoreService<FileTransactionModel> transactions, 
        IDataStoreService<FileAuditModel> audits
    )
    {
        _writeToFile = writeToFile;
        _users = users.GetModels();
        _accounts = accounts.GetModels();
        _transactions = transactions.GetModels();
        _audits = audits.GetModels();
    }

    public IResult Execute(IChangeBasicUserPassword.Request request)
    {
        FileUserModel oldUser = _users.Where(user => user.UserId == request.UserId).FirstOrDefault()!;

        if (oldUser is null)
        {
            return Result.Failed($"Unable to find user with Id: {request.UserId}");
        }


        string newSalt = Guid.NewGuid().ToString();
        string newHash = FileUserRepository.CreateHash(newSalt, request.Password);

        FileUserModel newUser = new FileUserModel(
            oldUser.UserId, 
            newHash, 
            newSalt, 
            oldUser.UserRole, 
            oldUser.Name, 
            oldUser.Address, 
            oldUser.PhoneNumber, 
            oldUser.Email, 
            oldUser.CreationDate
        );

        _writeToFile.UpdateUsersFile(request.UserId, newUser);

        AddAudit(
            request.AdminId, 
            AdminInteraction.ResetUserPassword, 
            request.UserId, 
            DateTime.Now
        );

        return Result.Succeeded();
    }
    public IResult Execute(IAddUser.Request request)
    {
        FileUserModel newUser = new FileUserModel(
            request.UserId, 
            request.Hash, 
            request.Salt, 
            UserRole.Basic, 
            request.Name, 
            request.Address, 
            request.PhoneNumber, 
            request.Email, 
            DateTime.Now
        );

        _writeToFile.UpdateUsersFile(null, newUser);

        AddAudit(
            request.AdminId, 
            AdminInteraction.AddUser, 
            request.UserId, 
            DateTime.Now
        );

        return Result.Succeeded();
    }
    public IResult Execute(IAddAccount.Request request)
    {
        string accountId = "";
        do
        {
            Random random = new Random();
            accountId = random.Next(10000, 100000).ToString();
        } while (_accounts.Where(acct => acct.AccountId == accountId).FirstOrDefault() != null);

        FileAccountModel newAccount = new FileAccountModel(
            accountId, 
            request.UserId, 
            request.AccountType, 
            request.Balance, 
            DateTime.Now
        );

        FileUserModel user = _users.Where(user => user.UserId == request.UserId).FirstOrDefault()!;

        _writeToFile.UpdateAccountsFile(null, newAccount);

        AddAudit(request.AdminId, AdminInteraction.AddAccount, request.UserId, DateTime.Now);

        return Result.Succeeded();
    }
    public IResult Execute(IDeleteUser.Request request)
    {
        FileUserModel user = _users.Where(user => user.UserId == request.UserId).FirstOrDefault()!;

        if (user is null)
        {
            return Result.Failed($"Unable to find user with Id: {request.UserId}");
        }

        _writeToFile.UpdateUsersFile(request.UserId, null);


        string[] accounts = _accounts.Where(acct => acct.UserId == user.UserId)
            .Select(acct => acct.AccountId)
            .ToArray();

        if (accounts.Any())
        {
            _writeToFile.UpdateAccountsFile(accounts, null);

            string[] transactions = _transactions.Where(tran => accounts.Contains(tran.AccountId))
                .Select(tran => tran.TranasctionId)
                .ToArray();

            if (transactions.Any())
            {
                _writeToFile.UpdateTransactionsFile(transactions, null);
            }
        }
        

        AddAudit(request.AdminId, AdminInteraction.DeleteUser, request.UserId, DateTime.Now);

        return Result.Succeeded();
    }
    public IResult Execute(IDeleteAccount.Request request)
    {
        _writeToFile.UpdateAccountsFile(new[] { request.AccountId }, null);

        string[] transactions = _transactions.Where(tran => tran.AccountId == request.AccountId)
            .Select(tran => tran.TranasctionId)
            .ToArray();

        if (transactions.Any())
        {
            _writeToFile.UpdateTransactionsFile(transactions, null);
        }

        AddAudit(request.AdminId, AdminInteraction.DeleteAccount, request.AccountId, DateTime.Now);

        return Result.Succeeded();
    }
    public List<string> GetAudits(string userId)
    {
        List<string> audits = new List<string>();
        IEnumerable<FileAuditModel> fileAudits = _audits.Where(audit => audit.AdminId == userId);
        foreach (var audit in fileAudits)
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
        IEnumerable<FileUserModel> foundUsers;
        List<string> users = new List<string>();

        switch (field)
        {
            case IdentityFields.Name:
                foundUsers = _users.Where(user => user.Name == input);
                break;
            case IdentityFields.Address:
                foundUsers = _users.Where(user => user.Address == input);
                break;
            case IdentityFields.PhoneNumber:
                foundUsers = _users.Where(user => user.PhoneNumber == input);
                break;
            case IdentityFields.Email:
                foundUsers = _users.Where(user => user.Email == input);
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
        } while (_audits.Where(audit => audit.AuditId == auditId).FirstOrDefault() != null);
        return auditId;
    }
    public bool DoesUserExist(string userId)
    {
        FileUserModel user = _users.Where(user => user.UserId == userId).FirstOrDefault()!;

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
        FileUserModel user = _users.Where(user => user.UserId == userId).FirstOrDefault()!;

        if (user is null)
        {
            throw new Exception($"User with with UserId {userId} could not be found");
        }

        return (user.Name, user.Address, user.PhoneNumber, user.Email, user.Salt, user.Hash);
    }
    public int GetTotalUsers()
    {
        return _users.Count();
    }
    public int GetTotalAccounts()
    {
        return _accounts.Count();
    }
    public double GetTotalBalance()
    {
        return _accounts.Select(acct => acct.Balance).Sum();
    }
    public string CreateUserId()
    {
        return (_users.Count() + 1).ToString();
    }

    private void AddAudit
    (
        string adminId, 
        AdminInteraction interaction, 
        string userId, 
        DateTime dateTime
    )
    {
        FileAuditModel newAudit = new FileAuditModel(
            CreateAuditId(), 
            adminId, 
            userId, 
            interaction, 
            dateTime
        );

        _audits.Add(newAudit);

        _writeToFile.UpdateAuditsFile(newAudit);
    }
}
