﻿using ATMProject.Application.Users;
using ATMProject.Banking;
using ATMProject.Data.MockDatabase.MockDatabase;
using ATMProject.Data.ModifyData;
using System.Reflection;

namespace ATMProject.Data.MockDatabase;
public class MockDatabaseFileRead : IReadFile
{
    public static HashSet<MockDatabaseUserModel> Users = new HashSet<MockDatabaseUserModel>();
    public static HashSet<MockDatabaseAccountModel> Accounts = new HashSet<MockDatabaseAccountModel>();
    public static HashSet<MockDatabaseTransactionModel> Transactions = new HashSet<MockDatabaseTransactionModel>();
    public static HashSet<MockDatabaseAuditModel> Audits = new HashSet<MockDatabaseAuditModel>();

    public void ReadAllFilesContents()
    {
        string FOLDER_DIRECTORY = "C:\\Users\\Ethan\\source\\repos\\ATMProject\\ATMProject\\Resources\\";

        // ReadAllLines is fine now, things like StreamReader could be better for larger thngs
        string[] usersFileContents = File.ReadAllLines(Path.Combine(FOLDER_DIRECTORY, "Users.txt"));

        foreach (string user in usersFileContents)
        {
            string[] userProperties = user.Split('|');
            if (userProperties.Length == 10)
            {
                if (int.TryParse(userProperties[0], out _))
                {
                    UserRole userRole = (UserRole)Enum.Parse(typeof(UserRole), userProperties[3]);
                    DateTime creationDate = DateTime.Parse(userProperties[8]);
                    List<string> accountIds = new List<string>(userProperties[9].Split(';'));

                    Users.Add(new MockDatabaseUserModel(userProperties[0], userProperties[1], userProperties[2], userRole, userProperties[4], userProperties[5], userProperties[6], userProperties[7], creationDate, accountIds));
                }
            }
        }



        string[] accountsFileContents = File.ReadAllLines(Path.Combine(FOLDER_DIRECTORY, "Accounts.txt"));

        foreach (string account in accountsFileContents)
        {
            string[] accountProperties = account.Split('|');
            if (accountProperties.Length == 6)
            {
                if (int.TryParse(accountProperties[0], out _))
                {
                    AccountType type = (AccountType)Enum.Parse(typeof(AccountType), accountProperties[2]);
                    double balance = Convert.ToDouble(accountProperties[3]);
                    DateTime creationDate = DateTime.Parse(accountProperties[4]);
                    List<string> transactionsAndAuditsIds = new List<string>(accountProperties[5].Split(';'));

                    Accounts.Add(new MockDatabaseAccountModel(accountProperties[0], accountProperties[1], type, balance, creationDate, transactionsAndAuditsIds));
                }
            }
        }



        string[] transactionsFileContents = File.ReadAllLines(Path.Combine(FOLDER_DIRECTORY, "Transactions.txt"));

        foreach (string transaction in transactionsFileContents)
        {
            string[] transactionProperties = transaction.Split('|');
            if (transactionProperties.Length == 7 && int.TryParse(transactionProperties[0], out _))
            {
                TransactionType type = (TransactionType)Enum.Parse(typeof(TransactionType), transactionProperties[2]);
                double amount = Convert.ToDouble(transactionProperties[3]);
                double previousBalance = Convert.ToDouble(transactionProperties[4]);
                double newBalance = Convert.ToDouble(transactionProperties[5]);
                DateTime transactionDate = DateTime.Parse(transactionProperties[6]);

                Transactions.Add(new MockDatabaseTransactionModel(transactionProperties[0], transactionProperties[1], type, amount, previousBalance, newBalance, transactionDate));
            }
        }



        string[] AuditsFileContents = File.ReadAllLines(Path.Combine(FOLDER_DIRECTORY, "Audits.txt"));

        foreach (string audits in AuditsFileContents)
        {
            string[] auditsProperties = audits.Split('|');
            if (auditsProperties.Length == 5 && int.TryParse(auditsProperties[0], out _))
            {
                AdminInteraction interaction = (AdminInteraction)Enum.Parse(typeof(AdminInteraction), auditsProperties[3]);
                DateTime auditDate = DateTime.Parse(auditsProperties[4]);

                Audits.Add(new MockDatabaseAuditModel(auditsProperties[0], auditsProperties[1], auditsProperties[2], interaction, auditDate));
            }
        }
    }
}
