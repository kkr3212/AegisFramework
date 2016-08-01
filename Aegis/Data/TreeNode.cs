using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;



namespace Aegis.Data
{
    [DebuggerDisplay("Name={Name} Value={Value}")]
    public partial class TreeNode
    {
        public readonly TreeNode Parent;
        public readonly List<TreeNode> Childs = new List<TreeNode>();
        public readonly string Name;
        public readonly string Value;

        public string this[string path] { get { return GetValue(path); } }





        public TreeNode(TreeNode parent, string name, string value)
        {
            Parent = parent;
            Name = name;
            Value = value;

            if (parent != null)
                parent.Childs.Add(this);
        }


        public TreeNode GetNode(string path)
        {
            string[] names = path.Split(new char[] { '\\', '/' });
            TreeNode node = this;


            foreach (string name in names)
            {
                node = node.Childs.Find(v => v.Name == name);
                if (node == null)
                    throw new AegisException(AegisResult.InvalidArgument, "Invalid node name({0}).", name);
            }

            return node;
        }


        /// <summary>
        /// 지정된 Path에서 값을 가져옵니다.
        /// 지정한 Path가 XmlAttribute가 아닌 경우, null을 반환할 수 있습니다.
        /// </summary>
        /// <param name="path">구분자는 \ 혹은 / 를 사용할 수 있습니다.</param>
        /// <returns>지정된 Path에 정의된 값</returns>
        public string GetValue(string path)
        {
            string[] names = path.Split(new char[] { '\\', '/' });
            TreeNode node = this;


            foreach (string name in names)
            {
                node = node.Childs.Find(v => v.Name == name);
                if (node == null)
                    throw new AegisException(AegisResult.InvalidArgument, "Invalid node name({0}).", name);
            }

            return node.Value;
        }


        /// <summary>
        /// 지정된 Path에서 값을 가져옵니다.
        /// 지정한 Path가 XmlAttribute가 아닌 경우, null을 반환할 수 있습니다.
        /// </summary>
        /// <param name="path">구분자는 \ 혹은 / 를 사용할 수 있습니다.</param>
        /// <param name="defaultValue">path에서 값을 가져올 수 없으면 default값을 반환합니다.</param>
        /// <returns>path에 값이 정의되어있으면 해당 값을 반환하고 path가 잘못되어있으면 defaultValue를 반환합니다.</returns>
        public string GetValue(string path, string defaultValue)
        {
            string[] names = path.Split(new char[] { '\\', '/' });
            TreeNode node = this;


            foreach (string name in names)
            {
                node = node.Childs.Find(v => v.Name == name);
                if (node == null)
                    return defaultValue;
            }

            return node.Value;
        }
    }
}
