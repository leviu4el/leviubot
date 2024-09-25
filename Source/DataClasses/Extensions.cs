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

        public static List<EmbedBuilder> AddStrings(this EmbedBuilder embed, string title, List<string> strings)
        {
            List<EmbedBuilder> embeds = new List<EmbedBuilder>();
            if (strings.Count == 0) return embeds;

            EmbedBuilder embed_copy = embed;
            string invisibleChar = "‎";
            string reply = string.Empty;
            foreach (var item in strings)
            {
                if ((reply + item).Length < 1000)
                {
                    reply += "\n" + item;
                }
                else
                {
                    embed_copy.AddField(new EmbedFieldBuilder
                    {
                        Name = title,
                        Value = reply
                    });
                    if (title != invisibleChar) title = invisibleChar;
                    if (embed_copy.Fields.Count() >= 3)
                    {
                        embeds.Add(embed_copy);
                        embed_copy = embed;
                    }
                    reply = item;
                }
            }
            if (reply.Length > 0)
            {
                embed_copy.AddField(new EmbedFieldBuilder
                {
                    Name = title,
                    Value = reply
                });
                embeds.Add(embed_copy);
            }

            return embeds;
        }

    }
}
