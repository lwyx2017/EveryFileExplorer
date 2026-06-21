using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQTrackPriorityEvent : SSEQEvent
    {
    	public byte Priority { get; private set; }

    	public SSEQTrackPriorityEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Priority = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(14, Priority, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}