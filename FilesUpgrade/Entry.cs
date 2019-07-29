﻿#nullable enable
using FilesUpgrade.Controller;
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

        public Entry(MainController mainController)
        {
            this.mainController = mainController;
        }

        public Out<string> Execute(string[] args)
        {
            var expr = from command in FetchCommand(args)
                       from _       in Router(command, args.Tail().ToSeq())
                       select command;
            var result = expr();

            if (result.IsFailed)
            {
                result.Error.Match(
                    err => Console.WriteLine(err.Message),
                    () => Console.WriteLine("unknown error"));
            }
            return result;
        }

        public Subsystem<string> FetchCommand(string[] args)
        {
            if (args.Count() == 0)
            {
                string message = "Usage: FilesUpgrad.exe <command>\n";
                message += "\n";
                message += "where <command> is one of:";
                message += "upgrade";
                return Subsystem.Fail<string>(message);
            }
            else
            {
                return Subsystem.Return<string>(args[0]);
            }
        }

        public Subsystem<Unit> Router(string command, Seq<string> args) =>
            command switch
            {
                "upgrade" => mainController.Upgrade(args),
                _         => Subsystem.Fail<Unit>($@"unknown command {command}")
            };
    }
}
