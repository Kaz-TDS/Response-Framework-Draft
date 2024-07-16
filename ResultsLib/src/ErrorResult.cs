using System;

namespace Tripledot.Results
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ErrorResultAttribute : Attribute
    {
        public ErrorResultAttribute(int errorCode, string errorMessage)
        {
        }
    }
}