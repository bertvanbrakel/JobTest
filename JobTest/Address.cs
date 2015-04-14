using System;

namespace JobTest
{
    public class Address : IComparable<Address> {
        public String Full { get; private set;}
        public String StreetName { get; private set;}
        public int StreetNumber { get; private set;}

        public Address(String address){
            this.Full = address;
            this.StreetName = extractStreetName(address);
            this.StreetNumber = extractStreetNumber(address);

        }

        private static String extractStreetName(String address){
            for (int i = 0; i < address.Length; i++) {
                if (Char.IsLetter (address [i])) {
                    //found start of name
                    return address.Substring (i);
                }
            }
            return address;
        }

        private static int extractStreetNumber(String address){

            for (int i = 0; i < address.Length; i++) {
                //not very robust. Will fail on 100 Street, 52nd Avene etc
                if (!Char.IsNumber (address [i])) {

                    //found end of number part. E.g 100 Round Street
                    if (i > 1) {
                        String numberPart = address.Substring (0, i);
                        int num;
                        if(int.TryParse(numberPart,out num)){
                            return num;
                        }

                    }

                }
            }
            return 0;
        }

        public int CompareTo(Address other) {
            int compare = this.StreetName.CompareTo(other.StreetName);
            if (compare == 0) {
                compare = this.StreetNumber.CompareTo (other.StreetNumber);
            }
            if (compare == 0) {
                compare = this.Full.CompareTo (other.Full);
            }
            return compare;
        } 
    }

}

