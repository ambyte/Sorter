using System;
using System.Text.RegularExpressions;

namespace Sorter
{
    class Record : IComparable<Record>
    {
        public int Number { get; }
        public string Text { get; }

        public Record(string text)
        {
            Regex regex = new Regex(@"(\d*). (.*)");
            var match = regex.Match(text);
            if (match.Success && match.Groups.Count == 3)
            {
                Number = Convert.ToInt32(match.Groups[1].Value);
                Text = match.Groups[2].Value;
            }
        }        

        public int CompareTo(Record other)
        {
            var compareResult = string.Compare(Text, other.Text, StringComparison.Ordinal);
            if (compareResult == 0)
            {
                return Number.CompareTo(other.Number);
            }
            return compareResult;
        }

        public override string ToString()
        {
            return $"{Number}. {Text}";
        }
    }
}
