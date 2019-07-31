using LanguageExt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesUpgrade.Model
{
    public class Node
    {
        public Either<FileInfo, DirectoryInfo> Info { get; }

        public Seq<Node> Children { get; } = Seq<Node>.Empty;

        public Node(FileInfo info)
        {
            this.Info = info;
        }

        public Node(DirectoryInfo info)
        {
            this.Info = info;
        }

        public Node(FileInfo info, IEnumerable<Node> children)
        {
            this.Info = info;
            this.Children = children.ToSeq();
        }

        public Node(DirectoryInfo info, IEnumerable<Node> children)
        {
            this.Info = info;
            this.Children = children.ToSeq();
        }
    }
}
