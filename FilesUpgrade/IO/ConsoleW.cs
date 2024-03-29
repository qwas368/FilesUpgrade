﻿using FilesUpgrade.Model;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace FilesUpgrade.IO
{
    public static class ConsoleW
    {
        public static Unit WriteLine(string value) =>
            fun(() => Console.WriteLine(value))();

        public static Unit WriteLine(string value, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(value);
            Console.ResetColor();
            return unit;
        }

        public static Unit WriteLine(string value, ConsoleColor color, ConsoleColor background)
        {
            Console.ForegroundColor = color;
            Console.BackgroundColor = background;
            Console.WriteLine(value);
            Console.ResetColor();
            return unit;
        }

        public static Unit Write(string value, ConsoleColor color = ConsoleColor.Gray)
        {
            Console.ForegroundColor = color;
            Console.Write(value);
            Console.ResetColor();
            return unit;
        }

        private const string _cross = " ├─";
        private const string _corner = @" └─";
        private const string _vertical = " │ ";
        private const string _space = "   ";
        public static Unit PrintNode(Node node, string indent, bool isLast)
        {
            // Print the provided pipes/spaces indent
            Console.Write(indent);

            // Depending if this node is a last child, print the
            // corner or cross, and calculate the indent that will
            // be passed to its children
            if (isLast)
            {
                Console.Write(_corner);
                indent += _space;
            }
            else
            {
                Console.Write(_cross);
                indent += _vertical;
            }

            node.Info.Match(
                right => ConsoleW.WriteLine(right.Name, node.Color),
                left => ConsoleW.WriteLine(left.Name, node.Color));

            // Loop through the children recursively, passing in the
            // indent, and the isLast parameter
            var numberOfChildren = node.Children.Count();
            for (var i = 0; i < numberOfChildren; i++)
            {
                var child = node.Children[i];
                isLast = (i == (numberOfChildren - 1));
                PrintNode(child, indent, isLast);
            }

            return unit;
        }

        public static Unit PrintNode(Node node, string indent, bool isLast, Node selected)
        {
            // Print the provided pipes/spaces indent
            Console.Write(indent);

            // Depending if this node is a last child, print the
            // corner or cross, and calculate the indent that will
            // be passed to its children
            if (isLast)
            {
                Console.Write(_corner);
                indent += _space;
            }
            else
            {
                Console.Write(_cross);
                indent += _vertical;
            }

            // get file name
            var fileName = node.Info.Match(right => right.Name, left => left.Name);
            if (node == selected)
                ConsoleW.WriteLine(fileName, ConsoleColor.Black, ConsoleColor.Gray);
            else
                ConsoleW.WriteLine(fileName, node.Color);

            // Loop through the children recursively, passing in the
            // indent, and the isLast parameter
            var numberOfChildren = node.Children.Count();
            for (var i = 0; i < numberOfChildren; i++)
            {
                var child = node.Children[i];
                isLast = (i == (numberOfChildren - 1));
                PrintNode(child, indent, isLast, selected);
            }

            return unit;
        }
    }
}
