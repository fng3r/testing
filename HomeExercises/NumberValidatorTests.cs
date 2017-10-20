using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    [TestFixture]
    public class NumberValidatorTests
    {
        private static NumberValidator defaultNumberValidator;

        [OneTimeSetUp]
        public void SetUp()
        {
            defaultNumberValidator = new NumberValidator(10, 8);
        }

        [Test]
        public void Constructor_DoesNotThrowException_WithPositivePrecisionAndNonNegativeScale()
        {
            Action action = () => new NumberValidator(1, 0, true);
            action.ShouldNotThrow<ArgumentException>();

        }

        [Test]
        public void Constructor_ThrowsArgumentException_WhenPrecisionIsNotPositive()
        {
            Action action = () => new NumberValidator(0, 2);
            action.ShouldThrow<ArgumentException>("because precision is zero");
        }

        [Test]
        public void Constructor_ThrowsArgumentException_WhenScaleIsNegative()
        {
            Action test = () => new NumberValidator(1, -2);
            test.ShouldThrow<ArgumentException>("because scale is -2");
        }

        [Test]
        public void Constructor_ThrowsArgumentException_WhenPrecisionIsNotGreaterThanScale()
        {
            Action test = () => new NumberValidator(2, 2);
            test.ShouldThrow<ArgumentException>("because precision and scale are equal");
        }

        [Test]
        public void IsValidNumber_False_WithNull()
        {
            defaultNumberValidator.IsValidNumber(null).Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_False_WithEmptyString()
        {
            defaultNumberValidator.IsValidNumber(string.Empty).Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_False_WithNegativeNumberWhenOnlyPositiveExpected()
        {
            new NumberValidator(3, 2, true).IsValidNumber("-0.00").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_False_WithNotANumber()
        {
            defaultNumberValidator.IsValidNumber("a.sd").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_False_WhenNumberContainsTrailingWhitespaces()
        {
            defaultNumberValidator.IsValidNumber(" 1.1").Should().BeFalse();
            defaultNumberValidator.IsValidNumber("1.1 ").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_False_WhenIntPartIsOmitted()
        {
            defaultNumberValidator.IsValidNumber(".3").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_False_WhenFracPartLengthGreaterThanScale()
        {
            new NumberValidator(17, 2).IsValidNumber("0.000").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_False_WhenNumberLengthGreaterThanPrecision()
        {
            new NumberValidator(3, 2).IsValidNumber("-1.23").Should().BeFalse();
        }

        [Test]
        public void IsValidNumber_True_WhenScaleNumberLengthUnderPrecisionAndFracPartUnderScale()
        {
            new NumberValidator(17, 2, true).IsValidNumber("123456789.01").Should().BeTrue();
        }

        [Test]
        public void IsValidNumber_True_WhenNumberIsPrecededBySign()
        {
            defaultNumberValidator.IsValidNumber("+1.25").Should().BeTrue();
            defaultNumberValidator.IsValidNumber("-1.25").Should().BeTrue();
        }
    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("precision must be a non-negative number less or equal than precision");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            if (onlyPositive && match.Groups[1].Value == "-")
                return false;
            return true;
        }
    }
}