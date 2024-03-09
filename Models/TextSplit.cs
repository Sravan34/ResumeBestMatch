using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ResumeTextExtract.Models
{
    public static class TextSplit
    {
        public static IEnumerable<string> SplitText(this String value, int desiredLength)
        {
            var characters = StringInfo.GetTextElementEnumerator(value);
            while (characters.MoveNext())
                yield return String.Concat(Take(characters, desiredLength));
        }

        private static IEnumerable<string> Take(TextElementEnumerator enumerator, int count)
        {
            for (int i = 0; i < count; ++i)
            {
                yield return (string)enumerator.Current;

                if (!enumerator.MoveNext())
                    yield break;
            }
        }
    }
}
