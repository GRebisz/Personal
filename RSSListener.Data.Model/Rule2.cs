using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RSSListener.Data.Model
{
    public class Rule2
    {
        public Guid id = Guid.Empty;
        public string Includes = "";
        public string ClassName = "";
        public string MethodName = "";
        public string MethodSignature = "";
        public string Code = "";
        public string ReturnType = "";
    }
}
