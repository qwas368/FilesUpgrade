using LanguageExt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace FilesUpgrade.Model
{
    public class Node
    {
        public Either<FileInfo, DirectoryInfo> Info { get; }

        public ConsoleColor Color { get; set; } = ConsoleColor.White;

        public Seq<Node> Children { get; } = Seq<Node>();

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

    public static class NodeExt
    {
        public static IEnumerable<Node> Enumerate(this Node node)
        {
            if (node.Children.Count() > 0)
            {
                return Seq1(node) + node.Children.SelectMany(x => x.Enumerate()).ToSeq();
            }
            else
            {
                return Seq1(node);
            }
        }
    }
}
