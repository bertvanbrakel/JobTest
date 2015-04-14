using NUnit.Framework;
using System;
using TestFirst.Net;
using TestFirst.Net.Matcher;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Linq.Expressions;
using TestFirst.Net.Extensions.NUnit;
using System.IO;
using JobTest;

namespace JobTest
{
    [TestFixture ()]
    public class ProgramTest : AbstractNUnitScenarioTest
    {

        [Test ()]
        public void GetSortedNameCounts()
        {
            const string TEXT = @"
FirstName,LastName,Address,PhoneNumber
Jimmy,Smith,somewhere,5555
Smith,James,somewhere,5555
James,Brown,somewhere,5555
James,Owen,somewhere,5555
";

            var sorted = new Program ().GetSortedNameCounts (new CsvParser ().ParseString(TEXT,1));
            foreach (var sort in sorted) {
                Console.WriteLine (sort.Key + "," + sort.Value);
            }
            Expect
                .That(sorted)
                .Is(AList.InOrder()
                    .WithOnly(AKeyValuePair.EqualTo("James",3))
                    .And(AKeyValuePair.EqualTo("Smith",2))
                    .And(AKeyValuePair.EqualTo("Brown",1))
                    .And(AKeyValuePair.EqualTo("Jimmy",1))
                    .And(AKeyValuePair.EqualTo("Owen",1)));

        }

        [Test ()]
        public void GetSortedAddress()
        {
            String input;
            IList<String> sorted;
            //lets use a scenario for a change
            Scenario()
                .Given(input = @"
FirstName,LastName,Address,PhoneNumber
Jimmy,Smith,100 Street A,5555
Smith,James,50 Street B,5555
James,Brown,20 Street A,5555
James,Owen,1 Street C,5555
James,Owen,10 Street C,5555
Nicky,Smith,50 Street B,5555")
                .When(sorted = new Program ().GetSortedAddresses (new CsvParser ().ParseString(input,1)))
                .Then(ExpectThat(sorted), Is(AList.InOrder()
                        .WithOnly(AString.EqualTo("20 Street A"))
                        .And(AString.EqualTo("100 Street A"))
                .And(AString.EqualTo("50 Street B"))
                .And(AString.EqualTo("1 Street C"))
                .And(AString.EqualTo("10 Street C"))));

        }

        [Test ()]
        public void ReadAndWriteFiles()
        {
            String input;
            Program.Options opts;
            //lets use a scenario for a change. Bit crap with the magic string. Maybe should create a AString.EqualToTrimLines(...)
            Scenario ()      
                .Given (input = Path.GetTempFileName())
                .Given(()=>File.WriteAllText(input,@"FirstName,LastName,Address,PhoneNumber
Jimmy,Smith,102 Long Lane,29384857
Clive,Owen,65 Ambling Way,31214788
James,Brown,82 Stewart St,32114566
Graham,Howe,12 Howard St,8766556
John,Howe,78 Short Lane,29384857
Clive,Smith,49 Sutherland St,31214788
James,Owen,8 Crimson Rd,32114566
Graham,Brown,94 Roland St,8766556"))
                .Given(opts = new Program.Options{ 
                    Input=input,
                    OutNameFilePath=Path.GetTempFileName(), 
                    OutAddressFilePath=Path.GetTempFileName()})
                .When(()=>Program.Main(new String[]{opts.Input,opts.OutNameFilePath, opts.OutAddressFilePath}))
                .Then(
                    ExpectThat(File.ReadAllText(opts.OutNameFilePath)), 
                    Is(AString.EqualTo(@"Brown,2
Clive,2
Graham,2
Howe,2
James,2
Owen,2
Smith,2
Jimmy,1
John,1
")))
                .Then(ExpectThat(File.ReadAllText(opts.OutAddressFilePath)), Is(AString.EqualTo(@"65 Ambling Way
8 Crimson Rd
12 Howard St
102 Long Lane
94 Roland St
78 Short Lane
82 Stewart St
49 Sutherland St
")));
        }
    }

    public class ACsvRecord : PropertyMatcher<CsvParser.CsvRecord> {
        public static ACsvRecord With(){
            return new ACsvRecord ();
        }

        public ACsvRecord fields(String[] expect){
            WithMatcher ("fields",(actual)=>actual.fields, AStringArray.EqualTo(expect));
            return this;
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

    public static class AKeyValuePair {

        public static IMatcher<KeyValuePair<String,int>> EqualTo(String key, int value){
            return Matchers.Function ((KeyValuePair<String,int> actual)=>{
                if(!key.Equals(actual.Key) || !value.Equals(actual.Value)){
                    return false;
                }     
                return true;
            },()=>{return "KeyValuePair, key=" + key + ",value=" + value;} );
        }
    }


}

