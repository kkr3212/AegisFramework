using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;



namespace Aegis.Data
{
    public partial class TreeNode<T>
    {
        public static TreeNode<string> LoadFromXml(string xml, string nodeName)
        {
            TreeNode<string> root = new TreeNode<string>(null, nodeName, "");
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xml);


            XmlNode node = xmlDoc.SelectSingleNode(nodeName);
            Xml_GetChilds(node, root);

            return root;
        }


        private static void Xml_GetAttributes(XmlNode node, TreeNode<string> target)
        {
            foreach (XmlAttribute attr in node.Attributes)
            {
                new TreeNode<string>(target, attr.Name, attr.Value);
            }
        }


        private static void Xml_GetChilds(XmlNode node, TreeNode<string> target)
        {
            Xml_GetAttributes(node, target);
            foreach (XmlNode child in node.ChildNodes)
            {
                TreeNode<string> newNode = new TreeNode<string>(target, child.Name, "");
                Xml_GetChilds(child, newNode);
            }
        }
    }
}
