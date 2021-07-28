using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Synthie
{
    public class Synthesizer
    {
        private int channels;
        private int sampleRate;
        private double samplePeriod;

        private double time;

        //Section: the <score> tag
        private double bpm;                   // Beats per minute
        private int beatspermeasure;  // Beats per measure
        private double secperbeat;        // Seconds per beat

        //Section: Keeping track of Notes. 
        private List<Note> notes = new List<Note>();

        //Section: Time, Measures, Beats and Score position.
        private int currentNote;          // The current note we are playing
        private int measure;              // The current measure
        private double beat;              // The current beat within the measure

        private int measureNumber; //Added in

        /// <summary>
        ///  Get the time since we started generating audio
        /// </summary>
        public double Time { get => time; }

        public int Channels { get => channels; set => channels = value; }
        public int SampleRate { get => sampleRate; set => sampleRate = value; }
        public double SamplePeriod { get => samplePeriod; set => samplePeriod = value; }

        //Section:  Adding new instrument to Synthesizer. 
        private List<Instrument> instruments = new List<Instrument>();



        public Synthesizer()
        {
            channels = 2;
            sampleRate = 44100;
            samplePeriod = 1.0 / sampleRate;

            time = 0;

            bpm = 120;
            secperbeat = 0.5;
            beatspermeasure = 4;
        }


        /// <summary>
        /// Ready the generation
        /// </summary>
        public void Start()
        {
            instruments.Clear();
            currentNote = 0;
            measure = 0;
            beat = 0;
            time = 0;
        }

        /// <summary>
        /// Makes a single sound sample. The frame length should be the same as the number of channels.
        /// </summary>
        /// <param name="frame">The sample</param>
        /// <returns>true if there is any more samples to generate.</returns>
        public bool Generate(double[] frame)
        {
            //
            // Phase 1: Determine if any notes need to be played.
            //

            while (currentNote < notes.Count)
            {
                // Get the current note
                Note note = notes[currentNote];

                // If the measure is in the future we can't play
                // this note just yet.
                if (note.Measure > measure)
                    break;

                // If this is the current measure, but the
                // beat has not been reached, we can't play
                // this note.
                if (note.Measure == measure && note.Beat > beat)
                    break;

                //
                // Play the note!
                //

                // Create the instrument object
                Instrument instrument = null;

                if (note.Instrument == "ToneInstrument")
                {
                    instrument = new ToneInstrument();
                }
                //Added in
                if(note.Instrument == "Piano")
                {
                    instrument = new Piano();
                }

                // Configure the instrument object
                if (instrument != null)
                {
                    instrument.SampleRate = SampleRate;
                    instrument.SetNote(note); // Setting pitch and duration.
                    //instrument.Bpm = bpm;
      
                    instrument.Bpm = bpm; //Added this in as part of task 1
                    instrument.Start();

                    instruments.Add(instrument);
                }

                currentNote++;
            }

            //
            // Phase 2: Clear all channels to silence 
            //

            for (int c = 0; c < Channels; c++)
            {
                frame[c] = 0;
            }

            //
            // Phase 3: Play an active instruments
            //

            //
            // We have a list of active (playing) instruments.  We iterate over 
            // that list.  For each instrument we call generate, then add the
            // output to our output frame.  If an instrument is done (Generate()
            // returns false), we remove it from the list.
            //

            List<Instrument> toRemove = new List<Instrument>();
            foreach (Instrument instrument in instruments)
            {

                // Call the generate function
                if (instrument.Generate())
                {
                    // If we returned true, we have a valid sample.  Add it 
                    // to the frame.
                    for (int c = 0; c < Channels; c++)
                    {
                        
                        //wrap the frame in effects here.
                        frame[c] += instrument.Frame(c);

                        //boundary checking frame  <5
                        //after 5 start caching.
                        //manipulate framc with cahced(reverb)
                        //stop  caching at end.
                    }
                }
                else
                {
                    // If we returned false, the instrument is done.  mark it for removal
                    toRemove.Add(instrument);
                }
            }

            foreach (Instrument i in toRemove)
                instruments.Remove(i);

            //
            // Phase 4: Advance the time and beats
            //

            // Time advances by the sample period
            time += SamplePeriod;

            // Beat advances by the sample period divided by the 
            // number of seconds per beat.  The inverse of seconds
            // per beat is beats per second.
            beat += SamplePeriod / secperbeat;

            // When the measure is complete, we move to
            // a new measure.  We might be a fraction into
            // the new measure, so we subtract out rather 
            // than just setting to zero.
            if (beat > beatspermeasure)
            {
                beat -= beatspermeasure;
                measure++;
            }

            //
            // Phase 5: Determine when we are done
            //

            // We are done when there is nothing to play. 
            return instruments.Count > 0 || currentNote < notes.Count;

        }



        public void Clear()
        {
            instruments.Clear();
            notes.Clear();
        }


        public void OpenScore(string filename)
        {
            Clear();

            //
            // Open an XML document
            //
            XmlDocument reader = new XmlDocument();
            reader.Load(filename);

            //
            // Traverse the XML document in memory!!!!
            // Top level tag is 
            //
            //Display the contents of the child nodes.
            foreach (XmlNode node in reader.ChildNodes)
            {
                if (node.Name == "score")
                {
                    XmlLoadScore(node);
                }
            }

            notes.Sort();
        }


        private void XmlLoadScore(XmlNode xml)
        {

            // Get a list of all attribute nodes and the
            // length of that list
            foreach (XmlAttribute attr in xml.Attributes)
            {
                if (attr.Name == "bpm")
                {
                    bpm = Convert.ToDouble(attr.Value);
                    secperbeat = 1 / (bpm / 60);
                    
                }
                else if (attr.Name == "beatspermeasure")
                {
                    beatspermeasure = Convert.ToInt32(attr.Value);
                }
            }

            foreach (XmlNode node in xml.ChildNodes)
            {
                if (node.Name == "instrument")
                {
                    XmlLoadInstrument(node);
                }
            }
        }

        private void XmlLoadInstrument(XmlNode xml)
        {
            string instrument = "";


            // Get a list of all attribute nodes and the
            // length of that list
            foreach (XmlAttribute attr in xml.Attributes)
            {
                if (attr.Name == "instrument")
                {
                    instrument = attr.Value;
                }
            }

            foreach (XmlNode node in xml.ChildNodes)
            {
                if (node.Name == "note")
                {
                    XmlLoadNote(node, instrument);
                }
                //Added in
                if(node.Name == "measure")
                {
                    foreach (XmlAttribute attr in node.Attributes)
                    {
                        if (attr.Name == "number")
                        {
                            measureNumber = Convert.ToInt32(attr.Value) - 1;
                        }
                    }
                }
            }

        }

        private void XmlLoadNote(XmlNode xml, string instrument)
        {
            Note newNote = new Note();
            notes.Add(newNote);

            newNote.setMeasureNumber(measureNumber);//Added in

            newNote.XmlLoad(xml, instrument);
        }


    }
}
