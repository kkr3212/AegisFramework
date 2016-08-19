using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;



namespace Aegis.Data
{
    public partial class TreeNode
    {
        public static TreeNode LoadFromXml(string xml, string nodeName)
        {
            TreeNode root = new TreeNode(null, nodeName, "");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);


            XmlNode node = xmlDoc.SelectSingleNode(nodeName);
            Xml_GetChilds(node, root);

            return root;
        }


        private static void Xml_GetAttributes(XmlNode node, TreeNode target)
        {
            foreach (XmlAttribute attr in node.Attributes)
            {
                new TreeNode(target, attr.Name, attr.Value);
            }
        }


        private static void Xml_GetChilds(XmlNode node, TreeNode target)
        {
            Xml_GetAttributes(node, target);
            foreach (XmlNode child in node.ChildNodes)
            {
                TreeNode newNode = new TreeNode(target, child.Name, "");
                Xml_GetChilds(child, newNode);
            }
        }
    }
}
