using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQLoopStartMarkerEvent : SSEQEvent
    {
    	public byte NrLoop { get; private set; }

    	public SSEQLoopStartMarkerEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		NrLoop = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.MidiTrack.Add(new TextEvent("loopStart", MetaEventType.Marker, Result.CurrentTime));
    	}

    	public override string ToString()
    	{
    		return "sound.sseq.events.loopstartmarker";
    	}
    }
}