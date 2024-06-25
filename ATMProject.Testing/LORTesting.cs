using Microsoft.VisualStudio.TestTools.UnitTesting;
using ATMProject.Application.Operations;
using ATMProject.Application.Users;
using ATMProject.Data.ModifyData;
using ATMProject.System;
using ATMProject.Data.MockDatabase;
using ATMProject.Application;
using ATMProject.Data.FileProcesses;

namespace ATMProject.Testing;
[TestClass]
public class LORTesting
{
    private IOperation<IDepositToAccountOperation.Request, IResult> _innerOperation;
    private IUserContextService _userContextService;
    private ILogger _logger;
    private IDepositToAccountOperation _depositToAccountOp;
    private IReadFile _readFile;
    [TestInitialize]
    public void Setup()
    {
        _depositToAccountOp = new BasicOperationRepository(new FileWrite());
        _readFile = new FileRead();
        _readFile.ReadAllFilesContents();

        _innerOperation = _depositToAccountOp;
        _userContextService = new UserContextService();
        _logger = new ConsoleLogger();

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