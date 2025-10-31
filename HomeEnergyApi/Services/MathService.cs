using System.Text;

namespace HomeEnergyApi.Services
{
    public class MathService
    {
        public string ConvertToRomanNumeral(int? number)
        {
            if (number <= 0 || number > 3999)
                throw new ArgumentOutOfRangeException(nameof(number), "Input must be in the range 1-3999");

            var values = new[] { 1000, 900, 500, 400, 100, 90, 50, 40, 10, 9, 5, 4, 1 };
            var numerals = new[] { "M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };

            var result = new StringBuilder();

            for (int i = 0; i < values.Length; i++)
            {
                while (number >= values[i])
                {
                    result.Append(numerals[i]);
                    number -= values[i];
                }
            }

            return result.ToString();
        }
    }
}