using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthie
{
    public class ToneInstrument : Instrument
    {
        //Section:  ToneInstrument
        private SineWave sinewave = new SineWave();
        private double duration;
        private double time;

        private double noteDuration;

        private AR ar;

        public double Frequency { get => sinewave.Frequency; set => sinewave.Frequency = value; }
        //public int Bpm { get => bpm; set => bpm = value; }

        public ToneInstrument()
        {
            duration = 0.1;
            ar = new AR();
        }

        public override void Start()
        {
            sinewave.SampleRate = SampleRate;
            sinewave.Start();
            time = 0;

            noteDuration = duration * (1 * 60 / bpm);
            // Tell the AR object it gets its samples from 
            // the sine wave object.
            ar.Source = sinewave;
            ar.SampleRate = SampleRate;
            ar.NoteDuration = noteDuration;
            ar.Start();
        }


        public override bool Generate()
        {
            // Tell the component to generate an audio sample
            //sinewave.Generate();
            ar.Generate(); 

            // Read the component's sample and make it our resulting frame.
            frame[0] = ar.Frame(0);
            frame[1] = ar.Frame(1);

            // Update time
            time += samplePeriod;

           // noteDuration = duration * (1 * 60 / bpm );
            // We return true until the time reaches the duration.
            return time < noteDuration;
        }

        //Let the instrument set the note. 
        public override void SetNote(Note note)
        {
            //duration = note.Count;
            duration = note.Count;
            Frequency = Notes.NoteToFrequency(note.Pitch);
        }

    }
}
