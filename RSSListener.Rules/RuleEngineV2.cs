using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RSSListener.Data.Model;
using System.Xml;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;

namespace RSSListener.Rules
{
    public class RuleEngineV2
    {
        public static string RunRule(RSSListener.Data.Model.Rule2 rule, string input)
        {
            if (rule == null)
            {
                return input;
            }
            string code = getmethod(rule.ClassName, rule.MethodName, rule.ReturnType, rule.MethodSignature, rule.Code, rule.Includes);
            // Compiler and CompilerParameters
            CSharpCodeProvider codeProvider = new CSharpCodeProvider();
            CompilerParameters compParameters = new CompilerParameters();

            // Compile the code
            CompilerResults res = codeProvider.CompileAssemblyFromSource(compParameters, code);
            if (res.Errors.HasErrors)
            {
                throw new Exception("Error in compilation of\r\n" + code + "\r\n\r\nRule ID: " + rule.id.ToString());
            }
            // Create a new instance of the class 'MyClass'
            object myClass = res.CompiledAssembly.CreateInstance(rule.ClassName);

            // Call the method 'PrintConsole' with the parameter 'Hello World'
            // "Hello World" will be written in console
            object o = (String)myClass.GetType().GetMethod(rule.MethodName).Invoke(myClass, new object[] { input });
            return o.ToString();
        }
        private static string getmethod(string classname, string methodname, string returntype, string signature, string meat, string usingstatement)
        {
            return usingstatement +
                    "public class " + classname + "{ " +
                    "   public static " + returntype + " " + methodname + "(" + signature + "){ " +
                    "       " + meat +
                    "   } " +
                    "} ";
        }
        public static string RunRule(RSSListener.Data.Model.Rule2 rule, XMLDefinition definition, WrappedXMLDef definitionelement, XmlNode node)
        {
            string input = "";
            switch (definitionelement)
            {
                case WrappedXMLDef.DescriptionTag:
                    input = node.SelectSingleNode(definition.DescriptionTag).InnerText;
                    break;
                case WrappedXMLDef.EnclosureTag:
                    input = node.SelectSingleNode(definition.EnclosureTag).OuterXml;
                    break;
                case WrappedXMLDef.PublishedTag:
                    input = node.SelectSingleNode(definition.PublishedTag).InnerText;
                    break;
                case WrappedXMLDef.TitleTag:
                    input = node.SelectSingleNode(definition.TitleTag).InnerText;
                    break;
                case WrappedXMLDef.URLTag:
                    input = node.SelectSingleNode(definition.URLTag).InnerText;
                    break;
                default:
                    throw new Exception("The tag does not exist");
            }
            return RunRule(rule, input);
        }
        public enum WrappedXMLDef
        {
            DescriptionTag,
            EnclosureTag,
            TitleTag,
            PublishedTag,
            URLTag
        }
    }
}
