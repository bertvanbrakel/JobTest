using System;
using NUnit.Framework;
using TestFirst.Net;
using TestFirst.Net.Matcher;
using JobTest;

namespace JobTest
{
    [TestFixture ()]
    public class CsvParserTest
    {
            [Test()]
            public void ParseAndSkipHeader()
            {
                const string TEXT = @"
FirstName,LastName,Address,PhoneNumber
Jimmy,Smith,102 Long Lane,29384857
Clive,Owen,65 Ambling Way,31214788";

                CsvParser parser = new CsvParser ();
                var records = parser.ParseString(TEXT,1);
                Expect
                    .That(records)
                    .Is(AList.InOrder()
                        .WithOnly(ACsvRecord.With().fields(new String[]{ "Jimmy","Smith","102 Long Lane","29384857"}))
                        .And(ACsvRecord.With().fields(new String[]{ "Clive","Owen","65 Ambling Way","31214788"})));

            }

        [Test()]
        public void ParseAndTrim()
        {
            const string TEXT = @"
FirstName,LastName,Address,PhoneNumber
  Jimmy,  Smith,    102 Long Lane,  29384857  
Clive,Owen,  65 Ambling Way,   31214788";

           CsvParser parser = new CsvParser {TrimFields= true};

            var records = parser.ParseString(TEXT,1);
            Expect
                .That(records)
                .Is(AList.InOrder()
                    .WithOnly(ACsvRecord.With().fields(new String[]{ "Jimmy","Smith","102 Long Lane","29384857"}))
                    .And(ACsvRecord.With().fields(new String[]{ "Clive","Owen","65 Ambling Way","31214788"})));

        }

      }

}

