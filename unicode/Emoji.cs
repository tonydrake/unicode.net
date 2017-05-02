﻿using System;
using System.Collections.Generic;
using System.Text;

namespace NeoSmart.Unicode
{
    //We hereby declare emoji to be plurale tantum (in short, "emoji" is both the singular and the plural form)
    //this class refers to emoji in the plural
    public static class Emoji
    {
        /// <summary>
        /// ZWJ is used to combine multiple emoji codepoints into a single emoji symbol.
        /// </summary>
        public static readonly Codepoint ZeroWidthJoiner = Codepoints.ZWJ;

        public static readonly Codepoint ObjectReplacementCharacter = Codepoints.ORC;

        /// <summary>
        /// The Emoji VS indicates that the preceding (non-emoji) unicode codepoint should be represented as an emoji.
        /// </summary>
        public static readonly Codepoint VariationSelector = Codepoints.VariationSelectors.EmojiSymbol;

        public static class SkinTones
        {
            public static readonly Codepoint Light = new Codepoint("U+1F3FB");
            public static readonly Codepoint Fitzpatrick12 = MediumLight;
            public static readonly Codepoint MediumLight = new Codepoint("U+1F3FC");
            public static readonly Codepoint Fitzpatrick3 = MediumLight;
            public static readonly Codepoint Medium = new Codepoint("U+1F3FD");
            public static readonly Codepoint Fitzpatrick4 = Medium;
            public static readonly Codepoint MediumDark = new Codepoint("U+1F3FE");
            public static readonly Codepoint Fitzpatrick5 = MediumDark;
            public static readonly Codepoint Dark = new Codepoint("U+1F3FF");
            public static readonly Codepoint Fitzpatrick6 = Dark;

            /// <summary>
            /// Helper object, most useful for checking if a codepoint is a skin tone quickly.
            /// </summary>
            public static readonly SortedSet<Codepoint> All = new SortedSet<Codepoint>() { Light, MediumLight, Medium, MediumDark, Dark };
        }

        /// <summary>
        /// Determines whether a string is comprised solely of emoji, optionally with a maximum number of drawn symbols.
        /// Can be used to determine whether a message consists of ≦ x emoji for purposes such as displaying at a larger size. Since one emoji symbol can be formed
        /// from many separate emoji "characters" combined with zero-width joiners or even non-emoji characters followed by a "use emoji representation" marker, this
        /// cannot be determined solely from the codepoints.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="maxSymbolCount"></param>
        /// <returns></returns>
        public static bool IsEmoji(string message, int maxSymbolCount = int.MaxValue)
        {
            var codepoints = message.Codepoints();

            bool nextMustBeVS = false;
            string zwj = ZeroWidthJoiner.AsString();
            string variationSelector = VariationSelector.AsString();
            bool ignoreNext = false;
            int count = 0;
            foreach (var cp in codepoints)
            {
                //we used to have message = message.trim() previously. This avoids the extra allocation, hepful in case of long messages.
                //this was not premature optimization, it came out of necessity.
                if (cp == "\n" || cp == "\r" || cp == "\t" || cp == " ")
                {
                    continue;
                }

                if (nextMustBeVS)
                {
                    nextMustBeVS = false;
                    if (cp != variationSelector)
                    {
                        //a non-emoji codepoint was found and this (the codepoint after it) is not the variation selector
                        return false;
                    }
                }
                if (cp.In(SkinTones.All))
                {
                    //don't count as part of the length
                    continue;
                }

                if (cp == zwj)
                {
                    ignoreNext = true;
                    continue;
                }

                if (cp == variationSelector)
                {
                    continue;
                }

                if (cp == Codepoints.ObjectReplacementCharacter)
                {
                    //this is explicitly blacklisted for UI purposes
                    return false;
                }

                if (!ignoreNext)
                {
                    ++count;
                    if (count > maxSymbolCount)
                    {
                        return false;
                    }
                    if (Languages.Emoji.Contains(cp))
                    {
                        continue;
                    }
                    else
                    {
                        //we've either encountered a non-emoji character OR a non-emoji codepoint that should be treated as an emoji if followed by the variation selector codepoint
                        nextMustBeVS = true;
                        continue;
                    }
                }
                ignoreNext = false;
            }

            if (nextMustBeVS)
            {
                return false;
            }

            return count <= maxSymbolCount;
        }
    }
}
