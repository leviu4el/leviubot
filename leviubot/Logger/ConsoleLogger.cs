namespace leviubot.Logger;

public class ConsoleLogger
{
    private static Dictionary<Logtype, ConsoleColor> _colors = new()
    {
        {Logtype.Success, ConsoleColor.Green},
        {Logtype.Warning, ConsoleColor.Yellow},
        {Logtype.Error, ConsoleColor.Red},
        {Logtype.Info, ConsoleColor.Blue},
        
        {Logtype.Default, ConsoleColor.Black}
    };


    public async Task PrintAsync(string? text = null, Logtype type = Logtype.Default, string? prefix = null, bool withDate = true)
    {
        if (text == null)
        {
            await Console.Out.WriteLineAsync();
            return;
        }
        if (withDate)
        {
            await ConsoleColor.DarkGray.WriteAsync($"{DateTime.Now:HH:mm:ss} ");
        }
        if (prefix != null)
        {
            await _colors[type].WriteAsync($"{prefix}: ");
        }
        
        await Console.Out.WriteLineAsync(text);
    }
}
public enum Logtype
{
    Default,
    Success,
    Info,
    Warning,
    Error,
}

public static class ConsoleExtensions
{
    private static readonly Lock _consoleLock = new();

    public static async Task WriteAsync(this ConsoleColor color, string text)
    {
        await Task.Yield();
        lock (_consoleLock)
        {
            //var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            //Console.ForegroundColor = originalColor;
            Console.ResetColor();
        }
    }
}