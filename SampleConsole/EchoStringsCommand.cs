﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ManyConsole.CommandLineUtils;

namespace SampleConsole
{
    public class EchoStringsCommand : ConsoleCommand
    {
        public string comments;

        public EchoStringsCommand()
        {
            IsCommand("echo");

            HasOption("c|comment=",
                           "enter a string, maybe even delimited by double quotes, i.e. - \"See Matt's poorly written code\"",
                           v => comments = v);

            AllowsAnyAdditionalArguments("<foo1> <foo2> <fooN> where N is a word");
        }

        public override int Run(string[] remainingArguments)
        {
            if ((comments?.Trim().Length ?? 0) == 0)
            {
                Console.WriteLine("You made no comment");
            }
            else
            {
                Console.WriteLine("Your comment is: " + comments);
            }

            Console.WriteLine("The remaining arguments were " + Newtonsoft.Json.JsonConvert.SerializeObject(remainingArguments));

            return 0;
        }
    }
}
