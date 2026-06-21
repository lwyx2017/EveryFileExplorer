using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQPanEvent : SSEQEvent
    {
    	public byte Pan { get; private set; }

    	public SSEQPanEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Pan = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.MidiTrack.Add(new ControlChangeEvent(Result.CurrentTime, Result.TrackID + 1, MidiController.Pan, Pan));
    	}
    }
}