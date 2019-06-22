using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            XmlDocument docVar = new XmlDocument();
            docVar.Load("AppVariables.wxi");
            XmlNode includeNode = docVar.SelectSingleNode("Include");
            foreach (XmlNode item in includeNode.ChildNodes)
            {
                if (item.Name.Equals("define") && item.Value.StartsWith("SourceFileDir"))
                {
                    item.Value = @"SourceFileDir = ""C:\Program Files (x86)\DIAView\""";
                    break;
                }
            }
            docVar.Save("AppVariables.wxi");


            


            XmlDocument doc = new XmlDocument();
            doc.Load("Bundle.wxs");


            XmlNodeList msiPackageList = doc.GetElementsByTagName("MsiPackage");


            foreach (XmlNode xmlNode in msiPackageList)
            {
                string attrId = xmlNode.Attributes["Id"].Value;

                if ("RuntimeSetup.msi".Equals(attrId,StringComparison.OrdinalIgnoreCase))
                {
                    xmlNode.Attributes["SourceFile"].Value = "x";
                }
                else if ("UserProjectSetup.msi".Equals(attrId, StringComparison.OrdinalIgnoreCase))
                {
                    xmlNode.Attributes["SourceFile"].Value = "y";
                }
            }

            doc.Save("Bundle.wxs");
        }
    }
}
