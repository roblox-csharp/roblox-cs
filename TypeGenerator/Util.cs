using System.Text;

namespace TypeGenerator
{
    internal static class Util
    {
        public static string ToCamelCase(string str)
        {
            if (string.IsNullOrEmpty(str))
                return str;

            if (str.Length < 2)
                return str.ToLower();

            string[] words = str.Split(new char[] { '_', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder camelCaseString = new StringBuilder(words[0].ToLower());

            for (int i = 1; i < words.Length; i++)
            {
                string word = words[i];
                if (word.Length > 0)
                {
                    camelCaseString.Append(char.ToUpper(word[0]));
                    camelCaseString.Append(word.Substring(1).ToLower());
                }
            }

            return camelCaseString.ToString();
        }
    }
}
