using NUnit.Framework;
using System;
using TestFirst.Net;
using TestFirst.Net.Matcher;
using System.Runtime.Remoting.Messaging;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using NUnit.Framework.Constraints;
using System.Text.RegularExpressions;

namespace JobTest
{
    [TestFixture ()]
    public class Test
    {
        const string TEXT2 = @"
FirstName,LastName,Address,PhoneNumber
Jimmy,Smith,102 Long Lane,29384857
Clive,Owen,65 Ambling Way,31214788
James,Brown,82 Stewart St,32114566
Graham,Howe,12 Howard St,8766556
John,Howe,78 Short Lane,29384857
Clive,Smith,49 Sutherland St,31214788
James,Owen,8 Crimson Rd,32114566
Graham,Brown,94 Roland St,8766556";


        [Test ()]
        public void CanParseAndSkipHeader()
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
    }

    /// <summary>
    /// Csv parser for small files. For large data sets will need to use some streaming reader rather than load all records in
    /// memory
    /// </summary>
    public class CsvParser {

        private readonly String CommentChar = "#";
        private readonly bool TrimFields = false;

        private readonly Regex LineSep = new Regex("\r\n|\n",RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly Regex FieldSepNoTrim = new Regex(",",RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly Regex FieldSepTrim = new Regex("[\f\r\v]*,[\f\r\v]*",RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public IList<CsvRecord> ParseString(string s){
            return ParseString(s,0);
        }

        public IList<CsvRecord> ParseString(string s, int skipNumRecords){
            String[] lines = LineSep.Split(s);
            Regex FieldSep = TrimFields ? FieldSepTrim : FieldSepNoTrim;
            var records = new List<CsvRecord> (lines.Length);
            int numRecordsRead = 0;
            foreach(String line in lines){
                if(!String.IsNullOrWhiteSpace(line) && !line.StartsWith(CommentChar)){//ignore empty lines
                    if(numRecordsRead >= skipNumRecords){//skip headers etc
                        String[] fields = FieldSep.Split(line);
                        records.Add (new CsvRecord (fields));
                    }
                    numRecordsRead++;
                }
            }

            return records;
        }  
    }

    public class CsvRecord {
        public String[] fields {
            get;
            private set;
        }

        public CsvRecord(String[] fields){
            this.fields = fields;
        }

        public override String ToString(){
            return "CsvRecord@" + GetHashCode() + "<" + String.Join (",", fields) + ">";
        }
    }

    public class ACsvRecord : AbstractMatcher<CsvRecord> {
        private IMatcher<String[]> arrayMatcher;

        public static ACsvRecord With(){
            return new ACsvRecord ();
        }

        public ACsvRecord field(int index,String expect){
            //WithMatcher ();
            return this;
        }

        public override bool Matches (CsvRecord actual, IMatchDiagnostics diag)
        {
            if (actual == null) {
                return false;
            }
            if (arrayMatcher != null) {
                return diag.TryMatch (actual.fields, arrayMatcher, this);
            }

            return true;
        }

        public ACsvRecord fields(String[] expect){
            arrayMatcher =  AStringArray.EqualTo (expect);

  /*          IMatcher<Object[]> matcher = AnArray.EqualTo<Object>(expect);
            WithMatcher ("fields",(actual)=>actual.fields, AStringArray);
            */
            return this;
        }

        public override void DescribeTo (IDescription desc)
        {

            if (arrayMatcher != null) {
                desc.Value ("CSVRecord with fields",arrayMatcher);
            } else {
                desc.Text ("Non Null CSVRecord");
            }
        }

    
    }

    /// <summary>
    /// As the TestFirst.Net.Matcher.AnArray seems to be bust, at least in Mono. Got to fix it
    /// </summary>
    public static class AStringArray {

        public static IMatcher<String[]> EqualTo(String[] expect){
            return Matchers.Function ((String[] actual) => {
                if(actual == null){
                    return false;
                }
                if(actual.Length != expect.Length){
                    return false;
                }
                for(int i = 0; i < actual.Length;i++){
                    if(actual[i] != expect[i]){
                        return false;
                    }
                }
                return true;
            }, () => {
                return "'" + String.Join("','",expect) + "'";
            });
        }
    }


}

