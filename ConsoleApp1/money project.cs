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
        private char _sign;
        private int _integerPart;
        private int _fractionalPart;
        private Currency _currency;

        private const int FractionMax = 100;

        public char Sign
        {
            get { return _sign; }
            set { _sign = value; }
        }

        public int IntegerPart
        {
            get { return _integerPart; }
            set { _integerPart = value; }
        }

        public int FractionalPart
        {
            get { return _fractionalPart; }
            set
            {
                if (value < 0 || value >= FractionMax)
                    throw new ArgumentOutOfRangeException(nameof(value), "Fractional part must be between 0 and 99.");
                _fractionalPart = value;
            }
        }

        public Currency CurrencyType
        {
            get { return _currency; }
            set { _currency = value; }
        }

        // Default constructor
        public Money()
        {
            Random rnd = new Random();
            _sign = rnd.Next(2) == 0 ? '+' : '-';
            _integerPart = rnd.Next(0, 1000);
            _fractionalPart = rnd.Next(0, 100);
            _currency = (Currency)rnd.Next(Enum.GetValues(typeof(Currency)).Length);
        }

        // Constructor with parameters
        public Money(Currency currency, char sign, int integer, int fractional)
        {
            _currency = currency;
            _sign = sign;
            _integerPart = integer;
            FractionalPart = fractional;
        }

        // Copy constructor
        public Money(Money other)
        {
            _sign = other._sign;
            _integerPart = other._integerPart;
            _fractionalPart = other._fractionalPart;
            _currency = other._currency;
        }

        // String-based constructor
        public Money(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Input cannot be empty or null", nameof(value));

            _sign = value[0];
            var parts = value.Substring(1).Split(new[] { '.', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            _integerPart = int.Parse(parts[0]);
            _fractionalPart = int.Parse(parts[1]);
            _currency = (Currency)Enum.Parse(typeof(Currency), parts[2]);
        }

        // Display method
        public string Display()
        {
            return $"{_sign}{_integerPart}.{_fractionalPart:D2} {_currency}";
        }

        // Addition of integer and fractional values
        public void Add(char sign, int integer, int fractional)
        {
            if (sign == '-')
            {
                _integerPart -= integer;
                _fractionalPart -= fractional;
            }
            else
            {
                _integerPart += integer;
                _fractionalPart += fractional;
            }
            AdjustForFractionOverflow();
            UpdateSign();
        }

        // Add another Money object
        public void Add(Money other)
        {
            Add(other._sign, other._integerPart, other._fractionalPart);
        }

        // Subtract integer and fractional values
        public void Subtract(char sign, int integer, int fractional)
        {
            if (sign == '+')
            {
                _integerPart -= integer;
                _fractionalPart -= fractional;
            }
            else
            {
                _integerPart += integer;
                _fractionalPart += fractional;
            }
            AdjustForFractionOverflow();
            UpdateSign();
        }

        // Subtract another Money object
        public void Subtract(Money other)
        {
            Subtract(other._sign == '+' ? '-' : '+', other._integerPart, other._fractionalPart);
        }

        // Equality check from stack overflow
        public bool Equals(Money other)
        {
            if (other == null) return false;
            return _sign == other._sign && _integerPart == other._integerPart && _fractionalPart == other._fractionalPart && _currency == other._currency;
        }

        //using Comparison method from stack overflow
        public int CompareTo(Money other)
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
            result.Add(other);
            return result;
        }

        // Difference between two Money objects
        public Money Difference(Money other)
        {
            Money result = new Money(this);
            result.Subtract(other);
            return result;
        }

        // to coonvert to another currency
        public Money ConvertToCurrency(Currency targetCurrency, decimal exchangeRate)
        {
            if (exchangeRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(exchangeRate), "Exchange rate must be positive.");

            decimal amount = (_sign == '-' ? -1 : 1) * (_integerPart + _fractionalPart / (decimal)FractionMax);
            decimal convertedAmount = amount * exchangeRate;

            char newSign = convertedAmount < 0 ? '-' : '+';
            convertedAmount = Math.Abs(convertedAmount);

            int newInteger = (int)convertedAmount;
            int newFractional = (int)((convertedAmount - newInteger) * FractionMax);

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
            else if (_fractionalPart < 0)
            {
                _integerPart--;
                _fractionalPart += FractionMax;
            }
        }

        //we need to Update sign based on the current state
        private void UpdateSign()
        {
            if (_integerPart < 0)
            {
                _sign = '-';
                _integerPart = Math.Abs(_integerPart);
            }
            else if (_integerPart == 0 && _fractionalPart == 0)
            {
                _sign = '+';
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
            int integerPart = int.Parse(Console.ReadLine());

            Console.WriteLine("Enter fractional part (0 to 99):");
            int fractionalPart = int.Parse(Console.ReadLine());

            Money m1 = new Money(currency, sign, integerPart, fractionalPart);
            Console.WriteLine("Created money object: " + m1.Display());

            // Perform some operations for testing
            m1.Add('+', 20, 75);
            Console.WriteLine("After addition: " + m1.Display());

            m1.Subtract('-', 10, 15);
            Console.WriteLine("After subtraction: " + m1.Display());

            Money m2 = m1.ConvertToCurrency(Currency.EUR, 0.85M);
            Console.WriteLine("Converted to EUR: " + m2.Display());

            Money m3 = new Money(m1);
            Console.WriteLine("Copy of m1: " + m3.Display());

            Money m4 = m1.Sum(m2);
            Console.WriteLine("Sum of m1 and m2: " + m4.Display());

            Money m5 = m1.Difference(m2);
            Console.WriteLine("Difference between m1 and m2: " + m5.Display());
        }
    }
}
