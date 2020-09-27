using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MaxLib.Args
{
    /// <summary>
    /// This class contains the parsed information from the argument array
    /// </summary>
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class Arguments : IEquatable<Arguments>
    {
        /// <summary>
        /// The Options that are defined in the args
        /// </summary>
        public Dictionary<string, List<string>> Options { get; }
            = new Dictionary<string, List<string>>();

        /// <summary>
        /// The list of the positional commands that are no options. They preserve their
        /// original index and keep their order of apperiance.
        /// </summary>
        public List<(int position, string command)> PositionalCommands { get; }
            = new List<(int position, string command)>();

        /// <summary>
        /// A list of arguments that are not passed and should be passed through
        /// </summary>
        public List<string> PassedThrough { get; } = new List<string>();


        /// <summary>
        /// checks if the commands string matches matches <paramref name="command"/>
        /// </summary>
        /// <param name="command">the command string to match</param>
        /// <returns>true if match is successfull</returns>
        public bool MatchesCommand(params string[] command)
            => MatchesCommand(StringComparison.CurrentCulture, command);

        /// <summary>
        /// checks if the commands string matches matches <paramref name="command"/>
        /// </summary>
        /// <param name="command">the command string to match</param>
        /// <param name="comparison">The comparison type of the match</param>
        /// <returns>true if match is successfull</returns>
        public bool MatchesCommand(StringComparison comparison, params string[] command)
        {
            _ = command ?? throw new ArgumentNullException(nameof(command));
            if (command.Length != PositionalCommands.Count)
                return false;
            for (int i = 0; i < command.Length; ++i)
                if (!string.Equals(command[i], PositionalCommands[i].command, comparison))
                    return false;
            return true;
        }

        /// <summary>
        /// checks if the start of the commands string matches matches <paramref name="command"/>
        /// </summary>
        /// <param name="command">the command string to match</param>
        /// <returns>true if match is successfull</returns>
        public bool MatchesCommandAtStart(params string[] command)
            => MatchesCommandAtStart(StringComparison.CurrentCulture, command);

        /// <summary>
        /// checks if the start of the commands string matches matches <paramref name="command"/>
        /// </summary>
        /// <param name="command">the command string to match</param>
        /// <param name="comparison">The comparison type of the match</param>
        /// <returns>true if match is successfull</returns>
        public bool MatchesCommandAtStart(StringComparison comparison, params string[] command)
        {
            _ = command ?? throw new ArgumentNullException(nameof(command));
            if (command.Length > PositionalCommands.Count)
                return false;
            for (int i = 0; i < command.Length; ++i)
                if (!string.Equals(command[i], PositionalCommands[i].command, comparison))
                    return false;
            return true;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var pair in Options)
                foreach (var value in pair.Value)
                {
                    if (first)
                        first = false;
                    else sb.Append(" ");
                    if ((value ?? "") != "")
                        sb.Append($"--{pair.Key} {value}");
                    else sb.Append($"--{pair.Key}");
                }
            foreach (var (_, command) in PositionalCommands)
            {
                if (first)
                    first = false;
                else sb.Append(" ");
                sb.Append(command);
            }
            return sb.ToString();
        }

        public static bool operator ==(Arguments left, Arguments right)
        {
            return EqualityComparer<Arguments>.Default.Equals(left, right);
        }

        public static bool operator !=(Arguments left, Arguments right)
        {
            return !(left == right);
        }

        private string GetDebuggerDisplay()
        {
            return ToString();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Arguments);
        }

        public bool Equals(Arguments other)
        {
            return other != null &&
                Enumerable.SequenceEqual(Options, other.Options, new OptionsComparer()) &&
                Enumerable.SequenceEqual(PositionalCommands, other.PositionalCommands) &&
                Enumerable.SequenceEqual(PassedThrough, other.PassedThrough);
        }

        class OptionsComparer : IEqualityComparer<KeyValuePair<string, List<string>>>
        {
            public bool Equals(KeyValuePair<string, List<string>> x, KeyValuePair<string, List<string>> y)
            {
                return x.Key == y.Key &&
                    Enumerable.SequenceEqual(x.Value, y.Value);
            }

            public int GetHashCode(KeyValuePair<string, List<string>> obj)
            {
                return obj.Key.GetHashCode() ^ HashCode(obj.Value);
            }
        }

        static int HashCode<T>(IEnumerable<T> items)
        {
            if (items is null)
                return 0;
            int res = 0x202816FE;
            foreach (var item in items)
                res = res * 31 + (item?.GetHashCode() ?? 0);
            return res;
        }

        public override int GetHashCode()
        {
            int hashCode = -1977035083;
            hashCode = hashCode * -1521134295 + HashCode(Options.Select(x => new OptionsComparer().GetHashCode(x)));
            hashCode = hashCode * -1521134295 + HashCode(PositionalCommands);
            hashCode = hashCode * -1521134295 + HashCode(PassedThrough);
            return hashCode;
        }
    }
}
