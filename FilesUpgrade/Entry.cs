#nullable enable
using FilesUpgrade.Controller;
using FilesUpgrade.IO;
using FilesUpgrade.Model;
using FilesUpgrade.Monad;
using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilesUpgrade
{
    public class Entry
    {
        private readonly MainController mainController;

        private readonly FileSystem fs;

        public Entry(
            MainController mainController,
            FileSystem fileSystem
            )
        {
            this.mainController = mainController;
            this.fs = fileSystem;
        }

        public Out<string> Execute(string[] args)
        {
            fs.CleanTmpFolder();

            var expr = from command in FetchCommand(args)
                       from _       in Router(command, args.Tail().ToSeq())
                       select command;
            var result = expr();

            if (result.IsFailed)
            {
                var errMessage = result.Error.Match(
                    err => err.Exception.Match(x => x.ToString(), () => err.Message),
                    () => "unknown error");

                Console.WriteLine(errMessage);
            }
            return result;
        }

        public Subsystem<string> FetchCommand(string[] args)
        {
            if (args.Count() == 0)
            {
                string message = "Usage: FilesUpgrad.exe <command>\n";
                message += "\n";
                message += "where <command> is :";
                message += "upgrade, copysetting";
                return Subsystem.Fail<string>(message);
            }
            else
            {
                return Subsystem.Return<string>(args[0]);
            }
        }

        public Subsystem<Unit> Router(string command, Seq<string> args) =>
            command.ToLower() switch
            {
                "upgrade"     => mainController.Upgrade(args),
                "copysetting" => mainController.CopySetting(args),
                _             => Subsystem.Fail<Unit>($@"unknown command {command}")
            };
    }
}
