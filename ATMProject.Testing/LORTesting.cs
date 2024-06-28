﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ATMProject.Application.Operations;
using ATMProject.Application.Users;
using ATMProject.Data.ModifyData;
using ATMProject.System;
using ATMProject.Data.MockDatabase;
using ATMProject.Application;
using ATMProject.Data.FileProcesses;
using ATMProject.Data.FileProcesses.FileModels;
using static Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext;
using System.Transactions;

namespace ATMProject.Testing;
[TestClass]
public class LORTesting
{
    private IDataStoreService<FileUserModel> _users;
    private IDataStoreService<FileAccountModel> _accounts;
    private IDataStoreService<FileTransactionModel> _transactions;
    private IDataStoreService<FileAuditModel> _audits;
    private IOperation<IDepositToAccountOperation.Request, IResult> _innerOperation;
    private IUserContextService _userContextService;
    private ILogger _logger;
    private IDepositToAccountOperation _depositToAccountOp;
    private IReadFile _readFile;

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

        _innerOperation = _depositToAccountOp;
        _userContextService = new UserContextService();

        _innerOperation = new LoggingOperationDecorator<IDepositToAccountOperation.Request, IResult>(_innerOperation, _userContextService, _logger);
        _userContextService.SetUserContext(new UserContext("1", UserRole.Basic));
    }
    [TestMethod]
    public void LORTest()
    {
        IResult expectedResult = Result.Succeeded();
        var result = _innerOperation.Execute(new IDepositToAccountOperation.Request("11111", 123));
        Assert.AreEqual(expectedResult.Success, result.Success);
    }
}