using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQEndOfTrackEvent : SSEQEvent
    {
    	public SSEQEndOfTrackEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.MidiTrack.Add(new MetaEvent(MetaEventType.EndTrack, 0, Result.CurrentTime));
    	}

    	public override string ToString()
    	{
    		return "End of Track";
    	}
    }
}
