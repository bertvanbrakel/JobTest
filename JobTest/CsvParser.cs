using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.IO;
using System.Deployment.Internal;

namespace JobTest
{


    /// <summary>
    /// Csv parser for small files. For large data sets will need to use some streaming reader rather than load all records in
    /// memory
    /// </summary>
    public class CsvParser {

        private readonly Regex LineSplitter = new Regex("\r\n|\n",RegexOptions.Compiled | RegexOptions.CultureInvariant);
        private readonly Regex FieldSplitter = new Regex(",",RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public String CommentChar { get; set;}
        public bool TrimFields { get; set;}

        public CsvParser(){
            CommentChar = "#";
            TrimFields = false;
        }

        public IList<CsvRecord> ParseFile(string filePath){
            return ParseFile(filePath,0);
        }

        public IList<CsvRecord> ParseFile(string filePath,int skipNumRecords){
            return ParseString(File.ReadAllText(filePath), skipNumRecords);
        }

        public IList<CsvRecord> ParseString(string s){
            return ParseString(s,0);
        }

        public IList<CsvRecord> ParseString(string s, int skipNumRecords){
            var lines = LineSplitter.Split(s);

            var records = new List<CsvRecord> (lines.Length);
            int numRecordsRead = 0;
            foreach(String line in lines){
                if(!String.IsNullOrWhiteSpace(line) && !line.StartsWith(CommentChar)){//ignore empty lines
                    if(numRecordsRead >= skipNumRecords){//skip headers etc
                        String[] fields = FieldSplitter.Split(line);
                        if (TrimFields) {
                            TrimAll(fields);
                        }
                        records.Add (new CsvRecord(fields));
                    }
                    numRecordsRead++;
                }
            }

            return records;
        }  

        private static void TrimAll(String[] fields){
            for (int i = 0; i < fields.Length; i++) {
                fields [i] = fields [i].Trim();
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

            public String GetOrNull(int position){
                if(position >= fields.Length){
                    return null;
                }
                return fields[position];
            }

            public override String ToString(){
                return "CsvRecord@" + GetHashCode() + "<fields=[" + String.Join (",", fields) + "]>";
            }
        }
    }

}

