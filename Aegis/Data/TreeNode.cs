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
        public TreeNode Parent { get; private set; }
        public List<TreeNode> Childs { get; private set; } = new List<TreeNode>();
        public string Name { get; private set; }
        public string Value { get; private set; }
        public string Path { get; private set; }

        public string this[string path]
        {
            get { return GetValue(path); }
            set { SetValue(path, value); }
        }

        public delegate string InvalidPathDelegator(TreeNode sender, string path);
        public InvalidPathDelegator InvalidPathHandler;





        public TreeNode(TreeNode parent, string name, string value)
        {
            Parent = parent;
            Name = name;
            Value = value;

            if (parent == null)
                Path = Name;
            else
                Path = parent.Name + "\\" + Name;

            if (parent != null)
                parent.Childs.Add(this);
        }


        public TreeNode AddNode(string path, string value)
        {
            string[] names = path.Split(new char[] { '\\', '/' });
            TreeNode node = this;

            foreach (string name in names)
            {
                var childNode = node.Childs.Find(v => v.Name == name);
                if (childNode == null)
                {
                    if (names.Last() == name)
                        childNode = new TreeNode(node, name, value);
                    else
                        childNode = new TreeNode(node, name, null);
                }

                node = childNode;
            }

            return GetNode(path);
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


        public TreeNode TryGetNode(string path)
        {
            string[] names = path.Split(new char[] { '\\', '/' });
            TreeNode node = this;


            foreach (string name in names)
            {
                node = node.Childs.Find(v => v.Name == name);
                if (node == null)
                    return null;
            }

            return node;
        }


        /// <summary>
        /// 지정된 Path에서 값을 가져옵니다.
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
                {
                    if (InvalidPathHandler != null)
                        return InvalidPathHandler(this, path);
                    else
                        throw new AegisException(AegisResult.InvalidArgument, "Invalid node name({0}).", name);
                }
            }

            return node.Value;
        }


        /// <summary>
        /// 지정된 Path에서 값을 가져옵니다.
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
                    return InvalidPathHandler?.Invoke(this, path) ?? defaultValue;
            }

            return node.Value;
        }


        public void SetValue(string path, string value)
        {
            TreeNode node = TryGetNode(path);
            if (node == null)
                node = AddNode(path, value);
            else
                node.Value = value;
        }
    }
}
