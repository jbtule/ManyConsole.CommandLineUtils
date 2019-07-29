using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
               
namespace ManyConsole.CommandLineUtils
{

    

    public class ConsoleCommandDispatcher
    {
        public static int DispatchCommand(ConsoleCommand command, string[] arguments, TextWriter consoleOut)
        {
            return DispatchCommand(new[] { command }, arguments, consoleOut);
        }

        public static IEnumerable<ConsoleCommand> FindCommandsInSameAssemblyAs(Type type){
#if NET20
                return FindCommandsInAssembly(type.Assembly);
#else
                return FindCommandsInAssembly(type.GetTypeInfo().Assembly);
#endif
        }

         public static IEnumerable<ConsoleCommand> FindCommandsInAssembly(Assembly assembly)
        {
#if NET20
            foreach(var aType in assembly.GetTypes()){
                if(aType.IsSubclassOf(typeof(ConsoleCommand))&& !aType.IsAbstract ){
                    yield return (ConsoleCommand) Activator.CreateInstance(aType);
                }
            }
#else
            foreach(var typeInfo in assembly.DefinedTypes){
                if(typeInfo.IsSubclassOf(typeof(ConsoleCommand)) && !typeInfo.IsAbstract){
                    yield return (ConsoleCommand) Activator.CreateInstance(typeInfo.AsType());
                }
            }
#endif
        }

        public static int DispatchCommand(IEnumerable<ConsoleCommand> commands, string[] arguments, TextWriter consoleOut)
        {
            var app = new CommandLineApplication();
           
            app.Out = consoleOut;

            var assemblyName =Assembly.GetEntryAssembly()?.GetName();
            app.FullName = assemblyName?.Name;

            app.VersionOption("-v|--version", assemblyName?.Version.ToString(), assemblyName?.Version.ToString());

            app.HelpOption("-?|-h|--help");

            app.OnExecute(() =>
            {
                app.ShowHelp();
                return 2;
            });

            foreach(var command in commands){
                command.Register(app);
            }
            try{
                return app.Execute(arguments);
            }catch(CommandParsingException ex){
                app.Out.WriteLine(ex.Message);
                return 2;
            }
        }
    }
}
