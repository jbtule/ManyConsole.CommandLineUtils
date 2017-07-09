using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using System.ComponentModel;

namespace ManyConsole.CommandLineUtils.Internal
{
    internal abstract class Option
    {
        internal abstract CommandOption Register(CommandLineApplication app);

        public abstract void Invoke();
    }

    internal class Option<T>:Option
	{
        public string Template { get;}
        public string Description { get; }
        public Action<T> Action { get; }

        internal CommandOption CmdOption { get; set; }

        public Option(string template, string description, Action<T> action)
		{
            Action = action;
            Description = description;
            Template = template;
        }

        public string MassagedTemplate(){
            var temp = Template;
            if(temp.EndsWith("=") || temp.EndsWith(":")){
                temp = temp.TrimEnd(':','=');
                temp += " <arg>";
            }

            
            var items = temp.Split('|');
            var template = new List<string>();
            foreach(var item in items){
                if(item.StartsWith("-")){
                   template.Add(item);
                }else{
                     if(item.Length > 1){
                        template.Add("--"+item);
                    }else{
                        template.Add("-"+item);
                    }
                }
            }
            temp = String.Join("|", template.ToArray());
            
            return temp;
        }

        internal override CommandOption Register(CommandLineApplication app)
        {
           
            var template = MassagedTemplate();

            CmdOption =app.Option(template,Description,
                                  template.Contains("<")
                                    ? CommandOptionType.MultipleValue 
                                    : CommandOptionType.NoValue);
            return CmdOption;
        }

        protected T Parse (string value)
		{
#if NET20
			var tt = typeof (T);
#else
            var tt = typeof(T).GetTypeInfo();
#endif
			var nullable = tt.IsValueType && tt.IsGenericType && 
				!tt.IsGenericTypeDefinition && 
				tt.GetGenericTypeDefinition () == typeof (Nullable<>);
			var targetType = nullable ? tt.GetGenericArguments () [0] : typeof (T);
			var conv = TypeDescriptor.GetConverter (targetType);
			T t = default (T);
			try {
				if (value != null)
					t = (T) conv.ConvertFromString (value);
			}
			catch (Exception e) {

				throw new Exception (string.Format ("Could not convert string `{0}' to type {1} for option `{2}'.",
							value, targetType.Name, MassagedTemplate()),e);
			}
			return t;
		}
        public override void Invoke()
        {
            foreach (var val in CmdOption.Values)
            {
                var convert = Parse(val);
                
                Action(convert);
            }
        }
    }
}
