using static System.Net.Mime.MediaTypeNames;
using System.Text.Json;

namespace Source.DataClasses
{
    public class EmbedException : Exception
    {
        public EmbedException(string title)
            : base(FormatParameters(title, new List<ExceptionParameter>()))
        {
        }

        public EmbedException(string title, ExceptionParameter parameter)
            : base(FormatParameters(title, new List<ExceptionParameter>{ parameter }))
        {
        }

        public EmbedException(string title, List<ExceptionParameter> parameters)
            : base(FormatParameters(title, parameters))
        {
        }

        private static string FormatParameters(string title, List<ExceptionParameter> parameters)
        {
            if (parameters.Count() == 0) return title;
            string reply = $"{title}\n\nParameters:\n";
            string description = string.Join("\n", parameters.Select(p => $"‎‎{p.Name} = {p.Value ?? "null"}"));
            if (description.Length > 1800 - reply.Length)
            {
                description.Substring(0, 1800 - title.Length);
                description += "...";
            }

            return $"{reply}```{description}```";
        }
    }



    public class RequireException : Exception
    {
        public RequireException(string name, object value)
            : base(FormatException(name, value))
        {
        }

        private static string FormatException(string name, object value)
        {
            string reply = $"Required\n\nParameter:\n";
            value =
                value == null
                ? JsonSerializer.Serialize(value, new JsonSerializerOptions { WriteIndented = true })
                : "null";

            string description = $"‎{name} = {value}";
            if (description.Length > 1800 - reply.Length)
            {
                description.Substring(0, 1800 - reply.Length);
                description += "...";
            }

            return $"{reply}```{description}```";
        }
    }

    public class ExceptionParameter
    {
        public string Name;
        public string Value;

        public ExceptionParameter (string name, object value)
        {
            Name = name;
            Value = value.ToString();
        }
    }
}
