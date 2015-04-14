using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
using System.Linq;
using System.Linq.Expressions;
using System.IO;
using System.ComponentModel.Design;

namespace JobTest
{
    public class Program {
        private const int IDX_FNAME = 0;
        private const int IDX_LNAME = 1;
        private const int IDX_ADDRESS = 2;

        public struct Options {
            public String Input;
            public String OutNameFilePath;
            public String OutAddressFilePath;
        }

        public static void Main(String[] args){
            if (args == null || args.Length < 3) {
                throw new ArgumentException ("Need to specify args:\n\targs[0]=csv input file\n\targs[1]=sorted name output\n\targs[2]=sorted address output");
            }
            new Program ().Main(new Options{ Input=args[0], OutNameFilePath=args[1], OutAddressFilePath=args[2]});
        }

        public void Main(Options opts){
            var records = new CsvParser ().ParseFile (opts.Input, 1);
            var sortedNames = GetSortedNameCounts (records);
            var sortedAddresses = GetSortedAddresses(records);

            WriteToFile (sortedNames, pair => {return pair.Key + "," + pair.Value;}, opts.OutNameFilePath);
            WriteToFile (sortedAddresses, name => {return name;}, opts.OutAddressFilePath);
        }

        internal IList<KeyValuePair<String,int>> GetSortedNameCounts(IList<CsvParser.CsvRecord> records){
            var nameCounts = new Dictionary<String,int> (StringComparer.InvariantCultureIgnoreCase);
            var addName = new Action<String> (name=>{
                if(name == null ){
                    return;
                }
                int count;
                nameCounts.TryGetValue(name, out count);//if not existing, will set to default '0'
                nameCounts[name] = count + 1;
            });
            foreach (var record in records) {
                addName(record.GetOrNull (IDX_FNAME));
                addName(record.GetOrNull (IDX_LNAME));
            };
            return nameCounts.OrderByDescending(x=>x.Value).ThenBy(x=>x.Key).ToList();
        }

        internal IList<String> GetSortedAddresses(IList<CsvParser.CsvRecord> records){
            //TODO:strip house number from sort!
            var addresses = new SortedSet<Address> ();
            foreach (var record in records) {
                var address = record.GetOrNull (IDX_ADDRESS);
                if (address != null) {
                    addresses.Add (new Address (address));
                }
            };
            return addresses.Select(x=>x.Full).ToList();
        }

        private void WriteToFile<T>(IEnumerable<T> items,Func<T,String> toLineConverter,String fileName){

            using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName, true))
            {
                foreach (T item in items) {
                    file.WriteLine(toLineConverter.Invoke(item));
                }
            }
        }

    }
}

