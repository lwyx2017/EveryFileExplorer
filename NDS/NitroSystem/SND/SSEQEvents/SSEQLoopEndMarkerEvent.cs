using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQLoopEndMarkerEvent : SSEQEvent
    {
    	public SSEQLoopEndMarkerEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.MidiTrack.Add(new TextEvent("loopEnd", MetaEventType.Marker, Result.CurrentTime));
    	}

    	public override string ToString()
    	{
    		return "LoopEnd";
    	}
    }
}