﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using ATMProject.Application.Operations;
using ATMProject.Data.ModifyData;
using ATMProject.System;
using ATMProject.Data.MockDatabase;
using ATMProject.Data.FileProcesses;

namespace ATMProject.Testing;
[TestClass]
public class BORTesting
{
    private IOperation<IDepositToAccountOperation.Request, IResult> _depositToAccountOp;
    private IReadFile _readFile;
    [TestInitialize]
    public void Setup()
    {
        _depositToAccountOp = new BasicOperationRepository(new FileWrite());
        _readFile = new FileRead();
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
