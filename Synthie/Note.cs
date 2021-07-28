using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Synthie
{
    public class Note : IComparable<Note>
    {

        private string instrument;
        private int measure;
        private double beat;
        private double count;
        private string pitch;
        private string pedal;
        private string dynamic;
        private double amp;


        public double Beat { get => beat; }
        public double Count { get => count; }
        public string Instrument { get => instrument; }
        public int Measure { get => measure; }
        public string Pitch { get => pitch; }

        public double Amp { get => amp; }


        public Note()
        {
            amp = 1.0;
        }

        public void XmlLoad(XmlNode xml, string instrument)
        {
            this.instrument = instrument;


            // Get a list of all attribute nodes and the
            // length of that list
            foreach (XmlAttribute attr in xml.Attributes)
            {
                if (attr.Name == "measure")
                {
                    measure = Convert.ToInt32(attr.Value) - 1;
                }

                if (attr.Name == "beat")
                {
                    beat = Convert.ToDouble(attr.Value) - 1;
                }

                if (attr.Name == "count")
                {
                    count = Convert.ToDouble(attr.Value);
                }

                if (attr.Name == "note")
                {
                    pitch = attr.Value;
                }

                
                if (attr.Name == "pedal")
                {
                    pedal = attr.Value;
                }

                //Added in
                if (attr.Name == "dynamic")
                {
                    dynamic = attr.Value;
                    resolveDynamic();
                }
            }
            
            //Added in
            if(pedal == "down")
            {
                count = 6.5;
            }
            

        }

        //Added in
        public void resolveDynamic()
        {
            if(dynamic == "pp")
            {
                amp = 0.2;
            }
            else if(dynamic == "p")
            {
                amp = 0.4;
            }
            else if(dynamic == "mp")
            {
                amp = 0.5;
            }
            else if(dynamic == "mf")
            {
                amp = 0.7;
            }
            else if(dynamic == "f")
            {
                amp = 0.9;
            }
            else
            {
                amp = 1.0;
            }
           
        }

        //Added in
        public void setMeasureNumber(int measureNumber)
        {
            measure = measureNumber;
        }

        public int CompareTo(Note b)
        {
            if (measure < b.Measure)
                return -1;
            if (measure > b.Measure)
                return 1;
            if (beat < b.Beat)
                return -1;

            return 1;
        }
    }
}
