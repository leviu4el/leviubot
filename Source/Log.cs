using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwitchLib.Api.Core.Enums;

namespace leviubot.Source
{
    public class Log // Uuuuh, dont ask
    {
        public static void Print(string text = null, Logtype type = Logtype.Default, string prefix = null, bool withDate = true)
        {
            if (text == null)
            {
                Console.WriteLine();
                return;
            }
            if (withDate)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write($"{DateTime.Now:HH:mm:ss} ");
            }

            switch (type)
            {
                case Logtype.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case Logtype.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Logtype.Warning:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                case Logtype.Info:
                    Console.ForegroundColor = ConsoleColor.Blue;
                    break;
                case Logtype.Twitch:
                    Console.ForegroundColor = ConsoleColor.DarkMagenta;
                    break;
                default:
                    Console.ResetColor();
                    break;
            }

            if (prefix != null)
            {
                Console.Write($"{prefix}: ");
                Console.ResetColor();
            }
            Console.WriteLine(text);
            Console.ResetColor();
        }
    }
}
public enum Logtype
{
    Default,
    Success,
    Info,
    Warning,
    Error,
    Twitch
}