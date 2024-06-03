using ATMProject.Application.Operations;
using ATMProject.Application.Users;
using ATMProject.Banking;
using ATMProject.Data.MockDatabase;
using ATMProject.Data.MockDatabase.MockDatabase;

namespace ATMProject.Data.ModifyData
{
    public class AdminOperationsRepository : IAdminOperationsRepository
    {
        private readonly IWriteToFile _writeToFile;
        public AdminOperationsRepository(IWriteToFile writeToFile)
        {
            _writeToFile = writeToFile;
        }
        private string CreateAuditId()
        {
            string auditId;
            do
            {
                Random random = new Random();
                auditId = random.Next(10000, 100000).ToString();
            } while (MockDatabaseFileRead.Audits.Where(audit => audit.AuditId == auditId).FirstOrDefault() != null);
            return auditId;
        }
        public bool DoesUserExist(string userId)
        {
            MockDatabaseUserModel user = MockDatabaseFileRead.Users.Where(user => user.UserId == userId).FirstOrDefault()!;

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
            MockDatabaseUserModel user = MockDatabaseFileRead.Users.Where(user => user.UserId == userId).FirstOrDefault()!;

            if (user is null)
            {
                throw new Exception($"User with with UserId {userId} could not be found");
            }

            return (user.Name, user.Address, user.PhoneNumber, user.Email, user.Salt, user.Hash);
        }

        private void AddAudit(string adminId, AdminInteraction interaction, string userId, DateTime dateTime)
        {
            MockDatabaseAuditModel newAudit = new MockDatabaseAuditModel(CreateAuditId(), adminId, userId, interaction, dateTime);

            MockDatabaseFileRead.Audits.Add(newAudit);

            _writeToFile.UpdateTransactionsAndAuditsFile(null, newAudit, null);
        }

        public List<string> GetAudits(string userId)
        {
            List<string> audits = new List<string>();
            IEnumerable<MockDatabaseAuditModel> dbAudits = MockDatabaseFileRead.Audits.Where(audit => audit.AdminId == userId);
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
        public int GetTotalUsers()
        {
            return MockDatabaseFileRead.Users.Count();
        }
        public int GetTotalAccounts()
        {
            return MockDatabaseFileRead.Accounts.Count();
        }
        public double GetTotalBalance()
        {
            return MockDatabaseFileRead.Accounts.Select(acct => acct.Balance).Sum();
        }
        public string CreateUserId()
        {
            return (MockDatabaseFileRead.Users.Count() + 1).ToString();
        }

        public string[] LookupUserInfo(IdentityFields field, string input, string userId)
        {
            var allUsers = MockDatabaseFileRead.Users;
            IEnumerable<MockDatabaseUserModel> foundUsers;
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

            foreach (MockDatabaseUserModel user in foundUsers)
            {
                users.Add(user.UserId);
            }

            if (users.Count() != 0)
            {
                AddAudit(userId, AdminInteraction.LookupUserId, input, DateTime.Now);
            }

            return users.ToArray();
        }
        public void ChangeBasicUserPassword(string userId, string password, string adminId)
        {
            MockDatabaseUserModel oldUser = MockDatabaseFileRead.Users.Where(user => user.UserId == userId).FirstOrDefault()!;

            if (oldUser is null)
            {
                throw new Exception($"Unable to find user with Id: {userId}");
            }


            string newSalt = Guid.NewGuid().ToString();
            string newHash = MockDatabaseUserRepository.CreateHash(newSalt, password);

            MockDatabaseUserModel newUser = new MockDatabaseUserModel(oldUser.UserId, newHash, newSalt, oldUser.UserRole, oldUser.Name, oldUser.Address, oldUser.PhoneNumber, oldUser.Email, oldUser.CreationDate, oldUser.AccountIds);

            MockDatabaseFileRead.Users.Remove(oldUser);
            MockDatabaseFileRead.Users.Add(newUser);

            _writeToFile.UpdateUsersFile(userId, newUser);

            AddAudit(adminId, AdminInteraction.ResetUserPassword, userId, DateTime.Now);
        }
        public void AddUser(string name, string address, string phoneNumber, string email, string salt, string hash, string userId, string adminId)
        {
            MockDatabaseUserModel newUser = new MockDatabaseUserModel(userId, hash, salt, UserRole.Basic, name, address, phoneNumber, email, DateTime.Now, new List<string> { });

            MockDatabaseFileRead.Users.Add(newUser);

            _writeToFile.UpdateUsersFile(null, newUser);

            AddAudit(adminId, AdminInteraction.AddUser, userId, DateTime.Now);
        }
        public void AddAccount(string userId, AccountType accountType, double balance, string adminId)
        {
            string accountId = "";
            do
            {
                Random random = new Random();
                accountId = random.Next(10000, 100000).ToString();
            } while (MockDatabaseFileRead.Accounts.Where(acct => acct.AccountId == accountId).FirstOrDefault() != null);

            MockDatabaseAccountModel newAccount = new MockDatabaseAccountModel(accountId, userId, accountType, balance, DateTime.Now, null);

            MockDatabaseFileRead.Accounts.Add(newAccount);

            _writeToFile.UpdateAccountsFile(null, newAccount);

            MockDatabaseUserModel user = MockDatabaseFileRead.Users.Where(user => user.UserId == userId).FirstOrDefault()!;

            user.AccountIds.Add(accountId);

            _writeToFile.UpdateUsersFile(userId, user);

            AddAudit(adminId, AdminInteraction.AddAccount, userId, DateTime.Now);
        }
        public void DeleteUser(string userId, string adminId)
        {
            MockDatabaseUserModel user = MockDatabaseFileRead.Users.Where(user => user.UserId == userId).FirstOrDefault()!;

            if (user is null)
            {
                throw new Exception($"Unable to find user with Id: {userId}");
            }

            MockDatabaseFileRead.Users.Remove(user);

            _writeToFile.UpdateUsersFile(userId, null);

            AddAudit(adminId, AdminInteraction.DeleteUser, userId, DateTime.Now);
        }
        public void DeleteAccount(string accountId, string adminId)
        {
            MockDatabaseAccountModel accountToDelete = MockDatabaseFileRead.Accounts.Where(acct => acct.AccountId == accountId).FirstOrDefault()!;

            if (accountToDelete is null)
            {
                throw new Exception($"Unable to find account with Id: {accountToDelete}");
            }

            MockDatabaseFileRead.Accounts.Remove(accountToDelete);

            _writeToFile.UpdateAccountsFile(accountId, null);

            AddAudit(adminId, AdminInteraction.DeleteAccount, accountId, DateTime.Now);
        }
    }
}
