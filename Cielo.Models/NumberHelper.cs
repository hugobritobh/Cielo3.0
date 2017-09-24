using System;

namespace Cielo.Helper
{
    public static class NumberHelper
    {
        private const int NUM = 100;

        public static decimal IntegerToDecimal(object value)
        {
            return Convert.ToDecimal(value) / NUM;
        }

        public static object DecimalToInteger(object value)
        {
            return Convert.ToInt32(Convert.ToDecimal(value) * NUM);
        }
    }
}
