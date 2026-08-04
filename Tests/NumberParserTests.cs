using Microsoft.VisualStudio.TestTools.UnitTesting;
using OnlyV.VerseExtraction.Parser;

namespace Tests
{
    [TestClass]
    public class NumberParserTests
    {
        [TestMethod]
        public void TryParseNumber_ValidDigits_ReturnsTrueAndValue()
        {
            var success = NumberParser.TryParseNumber("42", out var value);

            Assert.IsTrue(success);
            Assert.AreEqual(42, value);
        }

        [TestMethod]
        public void TryParseNumber_LeadingZeros_ReturnsTrueAndValue()
        {
            var success = NumberParser.TryParseNumber("007", out var value);

            Assert.IsTrue(success);
            Assert.AreEqual(7, value);
        }

        [TestMethod]
        public void TryParseNumber_NonDigitCharacter_ReturnsFalse()
        {
            var success = NumberParser.TryParseNumber("12a", out var value);

            Assert.IsFalse(success);
            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void TryParseNumber_EmptyString_ReturnsFalse()
        {
            var success = NumberParser.TryParseNumber(string.Empty, out var value);

            Assert.IsFalse(success);
            Assert.AreEqual(0, value);
        }

        [TestMethod]
        public void TryParseNumber_AllZeros_ReturnsFalse()
        {
            // Documents existing behavior: the parser can't distinguish "no digits parsed"
            // from "parsed to zero", so an all-zero string is reported as unparsed.
            var success = NumberParser.TryParseNumber("0", out var value);

            Assert.IsFalse(success);
        }
    }
}
