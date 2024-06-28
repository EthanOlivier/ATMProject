using Microsoft.VisualStudio.TestTools.UnitTesting;
using ATMProject.Application.Operations;
using ATMProject.Data.ModifyData;
using ATMProject.System;
using ATMProject.Data.MockDatabase;
using ATMProject.Data.FileProcesses;
using ATMProject.Data.FileProcesses.FileModels;
using ATMProject.Application;

namespace ATMProject.Testing;
[TestClass]
public class BORTesting
{
    private IDataStoreService<FileUserModel> _users;
    private IDataStoreService<FileAccountModel> _accounts;
    private IDataStoreService<FileTransactionModel> _transactions;
    private IDataStoreService<FileAuditModel> _audits;
    private ILogger _logger;
    private IOperation<IDepositToAccountOperation.Request, IResult> _depositToAccountOp;
    private IReadFile _readFile;
    private IWriteFile _writeFile;

    [TestInitialize]
    public void Setup()
    {
        _users = new DataStoreService<FileUserModel>(new HashSet<FileUserModel>());
        _accounts = new DataStoreService<FileAccountModel>(new HashSet<FileAccountModel>());
        _transactions = new DataStoreService<FileTransactionModel>(new HashSet<FileTransactionModel>());
        _audits = new DataStoreService<FileAuditModel>(new HashSet<FileAuditModel>());

        _logger = new ConsoleLogger();

        _depositToAccountOp = new BasicOperationRepository(new FileWrite(_users, _accounts, _transactions, _audits), _users, _accounts, _transactions);
        _readFile = new FileRead(_users, _accounts, _transactions, _audits, _logger);
        _readFile.ReadAllFilesContents();
    }
    [TestMethod]
    public void BORTest()
    {
        IResult expectedResult = Result.Succeeded();
        var result = _depositToAccountOp.Execute(new IDepositToAccountOperation.Request("11111", 123));
        Assert.AreEqual(expectedResult.Success, result.Success);
    }
}
