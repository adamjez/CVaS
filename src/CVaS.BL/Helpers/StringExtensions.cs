namespace CVaS.BL.Helpers
{
    public static class StringExtensions
    {
        public static string ToTitleCase(this string str)
        {
            var array = str.ToCharArray();

            var isNextWord = true;
            for (var i = 0; i < array.Length; i++)
            {
                if (isNextWord)
                {
                    array[i] = char.ToUpperInvariant(array[i]);
                }

                isNextWord = char.IsWhiteSpace(array[i]);
            }

            return new string(array);
        }
    }
}