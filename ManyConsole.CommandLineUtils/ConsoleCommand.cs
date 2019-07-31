using System;
using System.Collections.Generic;
using System.Linq;
using ManyConsole.CommandLineUtils.Internal;
using McMaster.Extensions.CommandLineUtils;

namespace ManyConsole.CommandLineUtils
{
    public abstract class ConsoleCommand
    {
        public ConsoleCommand()
        {
            OneLineDescription = "";
            TraceCommandAfterParse = true;
            RemainingArgumentsCount = 0;
            RemainingArgumentsHelpText = "";
            OptionsHasd = new List<Option>();
            RequiredOptions = new List<RequiredOptionRecord>();
        }

        public string Command { get; private set; }
        public string OneLineDescription { get; private set; }
        public string LongDescription { get; private set; }

        internal void Register(CommandLineApplication app)
        {
            app.Command(Command,c =>{
                c.FullName = Command;
                c.Description = OneLineDescription;
                c.ExtendedHelpText = LongDescription;
                c.UsePagerForHelpText = app.UsePagerForHelpText;

                foreach (var option in OptionsHasd)
                {
                    var cmdOption = option.Register(c);
                }

                c.HelpOption("-?|-h|--help");

                CommandArgument remainingArgs = null;
                if(RemainingArgumentsHelpText?.Length > 0){
                    remainingArgs =c.Argument("", RemainingArgumentsHelpText, multipleValues: (RemainingArgumentsCount ?? 100) > 1);
                }
                c.OnExecute(() => {
                    
                    try{
                        if(TraceCommandAfterParse){
                            string introLine = String.Format("Executing {0}", Command);

                            if ((OneLineDescription?.Length ?? 0) == 0){
                                introLine += String.Format(" ({0})",OneLineDescription);
                            }

                            c.Out.WriteLine(introLine);

                        }
                        foreach (var option in OptionsHasd)
                        {
                            option.Invoke();
                        }

                        CheckRequiredArguments();
                
                        if(RemainingArgumentsCount != 0){
                            var actCount =(remainingArgs?.Values.Count() ?? 0);
                            if(actCount < RemainingArgumentsCount){
                                c.Out.WriteLine("Invalid number of arguments-- expected {1} more.", RemainingArgumentsCount - actCount);
                                c.ShowHelp();
                                return 2;
                            }
                        }

                        return Run(remainingArgs?.Values.ToArray() ?? new string []{});

                    }catch(ConsoleHelpAsException ex){
                        c.Out.WriteLine(ex.Message);
                        c.ShowHelp();
                        return 2;
                    }

                });
            });
        }

        public bool TraceCommandAfterParse { get; private set; }
        public int? RemainingArgumentsCount { get; private set; }
        public string RemainingArgumentsHelpText { get; private set; }
        private List<Option> OptionsHasd { get; set; }
        private List<RequiredOptionRecord> RequiredOptions { get; set; }

        public ConsoleCommand IsCommand(string command, string oneLineDescription = "")
        {
            Command = command;
            OneLineDescription = oneLineDescription;
            return this;
        }

        public ConsoleCommand HasLongDescription(string longDescription)
        {
            LongDescription = longDescription;
            return this;
        }

        public ConsoleCommand HasAdditionalArguments(int? count = 0, string helpText = "")
        {
            RemainingArgumentsCount = count;
            RemainingArgumentsHelpText = helpText;
            return this;
        }

        public ConsoleCommand AllowsAnyAdditionalArguments(string helpText = "")
        {
            HasAdditionalArguments(null, helpText);
            return this;
        }

        public ConsoleCommand SkipsCommandSummaryBeforeRunning()
        {
            TraceCommandAfterParse = false;
            return this;
        }

        public ConsoleCommand HasOption(string prototype, string description, Action<string> action)
        {
            return HasOption<string>(prototype, description, action);
        }	

        public ConsoleCommand HasRequiredOption(string prototype, string description, Action<string> action)
        {
            HasRequiredOption<string>(prototype, description, action);

            return this;
        }

        public ConsoleCommand HasOption<T>(string prototype, string description, Action<T> action)
        {
         
            OptionsHasd.Add(new Option<T>(prototype, description, action));
            return this;
        }

        public ConsoleCommand HasRequiredOption<T>(string prototype, string description, Action<T> action)
        {
            var requiredRecord = new RequiredOptionRecord();

            var option = new Option<T>(prototype, description, s =>
            {
                requiredRecord.WasIncluded = true;
                action(s);
            });

            OptionsHasd.Add(option);


            requiredRecord.Name = option.Template;

            RequiredOptions.Add(requiredRecord);

            return this;
        }

        public virtual void CheckRequiredArguments()
        {
            var missingOptions = this.RequiredOptions
                .Where(o => !o.WasIncluded).Select(o => o.Name).OrderBy(n => n).ToArray();

            if (missingOptions.Any())
            {
                throw new ConsoleHelpAsException(string.Format("Missing option: {0}", String.Join(", ", missingOptions)));
            }
        }

        public abstract int Run(string[] remainingArguments);

    }
}
