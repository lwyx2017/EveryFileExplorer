using System.Collections.Generic;
using NAudio.Midi;

namespace NDS.NitroSystem.SND
{
    public class SSEQMidiResult
    {
    	public bool NoteWait = true;

    	public bool Tie = false;

    	public IList<MidiEvent> MidiTrack = new List<MidiEvent>();

    	public int CurrentTime = 0;

    	public bool Goto = false;

    	public bool Return = false;

    	public uint GotoOffset = 0u;

    	public int ReturnOffset = -1;

    	public short[] LocalVariables = new short[16];

    	public short[] GlobalVariables = new short[16];

    	public bool ComparisonResult = true;

    	public bool If = false;

    	public int TrackID { get; set; }

    	public bool GotDrums { get; private set; }

    	public SSEQMidiResult(int TrackID, short[] GlobalVariables)
    	{
    		this.GlobalVariables = GlobalVariables;
    		this.TrackID = TrackID;
    	}
    }
}