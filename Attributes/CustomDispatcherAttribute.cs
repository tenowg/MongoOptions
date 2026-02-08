using System;
using System.Collections.Generic;
using System.Text;

namespace MongoOptions.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class CustomDispatcherAttribute : Attribute
    {
        public string MethodName { get; set; } = "Execute";
        public string WhiteList { get; set; } = "";
    }
}
