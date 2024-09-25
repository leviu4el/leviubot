using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using static Source.DataClasses.EmbedException;

namespace Source.DataClasses
{
    public static class Extensions
    {
        // i shouldn't use this
        public static bool Require(params object[] objects)
        {
            foreach (object obj in objects)
            {
                if (obj == null) throw new ArgumentNullException(nameof(obj));
            }
            return true;
        }
    }
}
