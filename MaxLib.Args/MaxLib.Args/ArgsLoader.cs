using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MaxLib.Args
{
    /// <summary>
    /// The parser that can convert the args to <see cref="Arguments"/>.
    /// </summary>
    public class ArgsLoader
    {
        /// <summary>
        /// If set all option names are converted to lower case
        /// </summary>
        public bool IgnoreOptionsCase { get; set; } = true;

        /// <summary>
        /// If set it searches for unix style options that start with a single <c>-</c> (dash).
        /// </summary>
        public bool UseDashOptions { get; set; } = true;

        /// <summary>
        /// If set it searches for windows style options that start with a single <c>/</c> (slash).
        /// </summary>
        public bool UseSlashOptions { get; set; } 
            = RuntimeInformation.IsOSPlatform(OSPlatform.Windows); //on unix arguments starting with / can be a path name

        /// <summary>
        /// If set it handles the option names with a single dash as flags and split 
        /// their name into their chars (one letter names). Any option with double
        /// dashes are handles as it is. This requires the <see cref="UseDashOptions"/>
        /// to work.
        /// </summary>
        /// <remarks>
        /// The command <c>-abc</c> is handled like <c>--a --b --c</c>. The command
        /// <c>--abc</c> is handled unchanged.
        /// </remarks>
        public bool UseSingleDashOptionFlags { get; set; } = false;

        /// <summary>
        /// If set it handles <c>-!abc</c> like <c>-!a -!b -!c</c>. If not
        /// <c>-!abc</c> is handled like <c>-! -a -b -c</c>. This requires
        /// the <see cref="UseSingleDashOptionFlags"/> to work.
        /// </summary>
        public bool NegateFlags { get; set; } = false;

        /// <summary>
        /// If set any option indicator like <c>-</c>, <c>--</c> or <c>/</c> are trimmed
        /// from the option name. The negate indicator remains unchanged.
        /// </summary>
        public bool TrimOptionIndicator { get; set; } = true;

        /// <summary>
        /// If set all content that apears after an option with an empty name (e.g. <c>--</c>)
        /// would be passed through and the parsing stops.
        /// </summary>
        public bool EnablePaseThrough { get; set; } = false;

        /// <summary>
        /// Parses the <paramref name="args"/> with the current options and return an
        /// <see cref="Arguments"/> object with the resulting information.
        /// </summary>
        /// <param name="args">the arguments to parse</param>
        /// <returns>the object with the resulting information</returns>
        public Arguments Parse(string[] args)
        {
            var result = new Arguments();
            for (int i = 0; i<args.Length; ++i)
            {
                var marker = GetOptionMarker(args[i]);
                if (marker == OptionMarker.None)
                {
                    result.PositionalCommands.Add((i, args[i]));
                    continue;
                }
                var value = GetOptionValue(args, ref i, out string name);
                if (TrimOptionIndicator)
                    name = name.Substring(marker == OptionMarker.DoubleDash ? 2 : 1);
                if (IgnoreOptionsCase)
                    name = name.ToLowerInvariant();
                if (EnablePaseThrough && name == "")
                {
                    result.PassedThrough.AddRange(args.Skip(i + 1));
                    return result;
                }
                if (UseSingleDashOptionFlags && marker == OptionMarker.Dash)
                {
                    if (!TrimOptionIndicator)
                        name = name.Substring(1);
                    var negate = NegateFlags && name.StartsWith("!");
                    if (negate)
                        name = name.Substring(1);
                    var names = name
                        .Select(x => negate ? $"!{x}" : $"{x}")
                        .Select(x => TrimOptionIndicator ? x : $"-{x}");
                    foreach (var n in names)
                        AddOption(result, n, value);
                }
                else
                {
                    AddOption(result, name, value);
                }
            }
            return result;
        } 

        enum OptionMarker
        {
            None,
            Slash,
            Dash,
            DoubleDash
        }

        private OptionMarker GetOptionMarker(string name)
        {
            if (UseSlashOptions && name.StartsWith("/"))
                return OptionMarker.Slash;
            if (UseDashOptions && name.StartsWith("--"))
                return OptionMarker.DoubleDash;
            if (UseDashOptions && name.StartsWith("-"))
                return OptionMarker.Dash;
            return OptionMarker.None;
        }

        private string GetOptionValue(string[] args, ref int index, out string name)
        {
            var ind = args[index].IndexOf('=');
            if (ind >= 0)
            {
                name = args[index].Substring(0, ind);
                return args[index].Substring(ind + 1);
            }
            name = args[index];
            if (args.Length > index + 1 && GetOptionMarker(args[index + 1]) == OptionMarker.None)
                return args[++index];
            else return null;
        }

        private void AddOption(Arguments args, string name, string value)
        {
            if (!args.Options.TryGetValue(name, out List<string> list))
                args.Options.Add(name, list = new List<string>());
            list.Add(value);
        }
    }
}
