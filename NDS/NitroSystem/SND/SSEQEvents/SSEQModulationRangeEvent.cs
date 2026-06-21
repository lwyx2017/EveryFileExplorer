using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQModulationRangeEvent : SSEQEvent
    {
    	public byte Range { get; private set; }

    	public SSEQModulationRangeEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Range = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(77, 64 + Range / 2, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}