using System;

namespace eric.coreminimal.data
{
    public class TimeWithData
    {
        public DateTime t { get; set; }
        public double v { get; set;  }

        public override string ToString()
        {
            return t.ToString() + "::" + v.ToString(); 
        }

        public override bool Equals(object obj)
        {
            TimeWithData other = obj as TimeWithData;
            if (other != null)
                if (other.t.Equals(t))
                    if (other.v.Equals(v))
                        return true;

            return false; 
        }
    }
}
