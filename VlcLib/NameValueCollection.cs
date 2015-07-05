using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VlcLib
{
    public class NameValueCollection
    {

        public NameValueCollection()
        {
            Values = new List<KeyValuePair>();
        }

        private List<KeyValuePair> values;

        public int Count
        {
            get
            {
                return Values.Count;
            }
        }

        internal List<KeyValuePair> Values
        {
            get
            {
                return values;
            }

            private set
            {
                values = value;
            }
        }

        public bool IsKeyExists(string key)
        {
            return (from s in Values
                    where s.Key == key
                    select s).Count() > 0;
        }

        public string GetKey(int i)
        {
            if ((i < 0) || (i > Values.Count - 1))
            {
                throw new IndexOutOfRangeException();
            }
            return Values[i].Key;
        }

        public void AddItem(string key, string value)
        {
            if (IsKeyExists(key))
            {
                throw new ArgumentException("The key already exists");
            }
            this.Values.Add(new KeyValuePair() { Key = key, Value = value });
        }

        public string this[int i]
        {
            get
            {
                if ((i < 0) || (i > Values.Count - 1))
                {
                    throw new IndexOutOfRangeException();
                }
                return this.Values[i].Value;
            }
        }
    }

    internal class KeyValuePair
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
