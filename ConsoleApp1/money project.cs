using System;

namespace Victor_Project_1
{
    public enum Currency
    {
        USD,
        RUB,
        EUR
    }

    public class Money : IEquatable<Money>, IComparable<Money>
    {
        private char _sign; // '+' or '-'
        private uint _integerPart; // non-negative whole number
        private uint _fractionalPart; // non-negative between 0-99
        private Currency _currency; // USD, RUB, EUR

        private const int FractionMax = 100;

        public char Sign
        {
            get { return _sign; }
            set
            {
                // for us to validate only for + and -
                if (value != '+' && value != '-')
                    throw new ArgumentException("the sign must be '+' or '-'.", nameof(value));
                _sign = value;
            }
        }

        public uint IntegerPart
        {
            get { return _integerPart; }
            set { _integerPart = value; } // no need to validate since uint can't be negative
        }

        public uint FractionalPart
        {
            get { return _fractionalPart; }
            set
            {
                if (value >= FractionMax) // to check if fractional part exceeds the allowed range
                    throw new ArgumentOutOfRangeException(nameof(value), "The fractional part must be between 0-99.");
                _fractionalPart = value;
            }
        }

        public Currency CurrencyType
        {
            get { return _currency; }
            set { _currency = value; }
        }

        // Default constructor to ensure that the sign is validated against invalid character
        public Money()
        {
            Random rnd = new Random();
            _sign = rnd.Next(2) == 0 ? '+' : '-';
            _integerPart = (uint)rnd.Next(0, 1000);
            _fractionalPart = (uint)rnd.Next(0, 100);
            _currency = (Currency)rnd.Next(Enum.GetValues(typeof(Currency)).Length);
        }

        // Constructor with parameters
        public Money(Currency currency, char sign, uint integer, uint fractional)
        {
            CurrencyType = currency;
            Sign = sign; // Validate sign
            IntegerPart = integer; // No validation needed for integer since it's uint
            FractionalPart = fractional; // Validate fractional part
        }

        // Copy constructor from stack overflow
        public Money(Money other)
        {
            if (other == null)
                throw new ArgumentNullException(nameof(other));

            _sign = other._sign;
            _integerPart = other._integerPart;
            _fractionalPart = other._fractionalPart;
            _currency = other._currency;
        }

        // String-based constructor from stack overflow
        // added checks to ensure the input format is correct and that it handles invalid inputs
        public Money(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Your input cannot be null", nameof(value));

            // Validate and assign the sign
            _sign = value[0];
            if (_sign != '+' && _sign != '-')
                throw new ArgumentException("The first character must be '+' or '-'.", nameof(value));

            var parts = value.Substring(1).Split(new[] { '.', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 3) // Check for correct format
                throw new ArgumentException("Input must be in the format '+100.25 USD'", nameof(value));

            // Parse integer and fractional parts as unsigned integers
            _integerPart = uint.Parse(parts[0]);
            _fractionalPart = uint.Parse(parts[1]);
            if (_fractionalPart >= FractionMax)
                throw new ArgumentOutOfRangeException(nameof(value), "Fractional part must be between 0-99.");

            _currency = Enum.Parse<Currency>(parts[2], true);
        }

        // Display method
        public string Display()
        {
            return $"{_sign}{_integerPart}.{_fractionalPart:D2} {_currency}";
        }

        // Addition of integer and fractional values
        public void Add(char sign, uint integer, uint fractional)
        {
            if (sign == '-')
            {
                Subtract('+', integer, fractional); // delegate to subtraction logic
            }
            else
            {
                _integerPart += integer;
                _fractionalPart += fractional;
                AdjustForFractionOverflow();
            }
        }

        // Subtract integer and fractional values from stack overflow
        public void Subtract(char sign, uint integer, uint fractional)
        {
            if (sign == '+')
            {
                if (_integerPart < integer || (_integerPart == integer && _fractionalPart < fractional))
                    throw new InvalidOperationException("Cannot subtract more than the current amount.");

                if (_fractionalPart < fractional)
                {
                    _integerPart -= 1; // Borrow from the integer part
                    _fractionalPart += FractionMax;
                }
                _integerPart -= integer;
                _fractionalPart -= fractional;
            }
            else
            {
                Add('+', integer, fractional); // Delegate to the addition logic
            }
        }

        // Equality check from stack overflow
        public bool Equals(Money? other)
        {
            if (other == null) return false;
            return _sign == other._sign
                && _integerPart == other._integerPart
                && _fractionalPart == other._fractionalPart
                && _currency == other._currency;
        }

        // Using Comparison method from stack overflow
        public int CompareTo(Money? other)
        {
            if (other == null) throw new ArgumentNullException(nameof(other));

            if (_sign != other._sign)
                return _sign == '-' ? -1 : 1;

            int result = _integerPart.CompareTo(other._integerPart);
            if (result != 0) return result;

            return _fractionalPart.CompareTo(other._fractionalPart);
        }

        // Summing two Money objects
        public Money Sum(Money other)
        {
            Money result = new Money(this);
            result.Add(other._sign, other._integerPart, other._fractionalPart);
            return result;
        }

        // Difference between two Money objects
        public Money Difference(Money other)
        {
            Money result = new Money(this);
            result.Subtract(other._sign, other._integerPart, other._fractionalPart);
            return result;
        }

        // to convert to another currency
        public Money ConvertToCurrency(Currency targetCurrency, decimal exchangeRate)
        {
            if (exchangeRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be positive.");

            decimal amount = (_sign == '-' ? -1 : 1) * (_integerPart + _fractionalPart / (decimal)FractionMax);
            decimal convertedAmount = amount * exchangeRate;

            char newSign = convertedAmount < 0 ? '-' : '+';
            convertedAmount = Math.Abs(convertedAmount);

            uint newInteger = (uint)convertedAmount;
            uint newFractional = (uint)((convertedAmount - newInteger) * FractionMax);

            return new Money(targetCurrency, newSign, newInteger, newFractional);
        }

        // Adjust overflow in fractional part
        private void AdjustForFractionOverflow()
        {
            if (_fractionalPart >= FractionMax)
            {
                _integerPart += _fractionalPart / FractionMax;
                _fractionalPart %= FractionMax;
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // for us to get the user input for the money creation
            Console.WriteLine("Enter currency (USD, RUB, EUR):");
            Currency currency = (Currency)Enum.Parse(typeof(Currency), Console.ReadLine().ToUpper());

            Console.WriteLine("Enter sign (+ or -):");
            char sign = Console.ReadLine()[0];

            Console.WriteLine("Enter integer part:");
            uint integerPart = uint.Parse(Console.ReadLine());

            Console.WriteLine("Enter fractional part (0 to 99):");
            uint fractionalPart = uint.Parse(Console.ReadLine());

            Money m1 = new Money(currency, sign, integerPart, fractionalPart);
            Console.WriteLine("Created money object: " + m1.Display());

            // And let's perform some operations for testing
            m1.Add('+', 20, 75);
            Console.WriteLine("After addition: " + m1.Display());

            m1.Subtract('+', 10, 15);
            Console.WriteLine("After subtraction: " + m1.Display());
        }
    }
}
