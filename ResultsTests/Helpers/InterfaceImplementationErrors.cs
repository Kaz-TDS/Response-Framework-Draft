using System;
using System.Collections.Generic;
using TDS.Results;

namespace ResultsTests.Helpers
{
    public interface Interface
    {
        public Result<IEnumerable<bool>> InterfaceMethod(List<string> stuff);
    }

    public class InterfaceImplementationCaseOne : Interface
    {
        [ErrorResult(errorCode:1, errorMessage:"BLA")]
        public Result<IEnumerable<bool>> InterfaceMethod(List<string> stuff)
        {
            return ResultsFactory.InterfaceImplementationCaseOne.InterfaceMethod.Bla();
        }
    }

    public class InterfaceImplementationCaseTwo : Interface
    {
        public Result<IEnumerable<bool>> InterfaceMethod(List<string> stuff)
        {
            throw new NotImplementedException();
        }
    }
}
