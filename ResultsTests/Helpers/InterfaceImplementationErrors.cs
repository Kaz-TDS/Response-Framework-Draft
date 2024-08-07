using System;
using System.Collections.Generic;
using Tripledot.Results;

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
        [ErrorResult(errorCode:1, errorMessage:"Bla2")]
        public Result<IEnumerable<bool>> InterfaceMethod(List<string> stuff)
        {
            throw new NotImplementedException();
        }
    }
}
