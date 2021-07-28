using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synthie
{
    public abstract class Instrument : AudioNode
    {
        //Section: Instrument
        public abstract void SetNote(Note note);
    }
}
