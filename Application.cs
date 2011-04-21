#region usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#endregion

namespace Scratch
{
    public class Application
    {
        public static void Main()
        {
            var languages = new string[]
                                {
                                    "aa", "ab", "af", "ar", "as", "ay", "az", "ba", "be", "bg", "bh", "bi", "bn", "bo", "br", "ca", "co", "cs"
                                    , "cy", "da", "de", "dz", "el", "en", "eo", "es", "et", "eu", "fa", "fi", "fj", "fo", "fr", "fy", "ga",
                                    "gd", "gl", "gn", "gu", "ha", "he", "hi", "hr", "hu", "hy", "ia", "id", "ie", "ik", "is", "it", "iu", "ja"
                                    , "jw", "ka", "kk", "kl", "km", "kn", "ko", "ks", "ku", "ky", "la", "ln", "lo", "lt", "lv", "mg", "mi",
                                    "mk", "ml", "mn", "mo", "mr", "ms", "mt", "my", "na", "ne", "nl", "no", "oc", "om", "or", "pa", "pl", "ps"
                                    , "pt", "qu", "rm", "rn", "ro", "ru", "rw", "sa", "sd", "se", "sg", "sh", "si", "sk", "sl", "sm", "sn",
                                    "so", "sq", "sr", "ss", "st", "su", "sw", "ta", "te", "tg", "th", "ti", "tk", "tl", "tn", "to", "tr", "ts"
                                    , "tt", "ug", "uk", "ur", "uz", "vi", "vo", "wo", "xh", "yi", "yo", "za", "zh", "zu"
                                };

            var invalid = languages.Select(lang =>
                                                   {
                                                       try
                                                       {
                                                           var cult = CultureInfo.GetCultureInfo(lang);
                                                           return null;
                                                       }
                                                       catch(Exception e)
                                                       {
                                                           return lang;
                                                       }
                                                   }).Where(cult => cult != null);

            foreach(var inv in invalid)
            {
                Console.WriteLine(inv);
            }
        }
    }
}