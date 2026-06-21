using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQPitchRangeEvent : SSEQEvent
    {
    	public byte PitchRange { get; private set; }

    	public SSEQPitchRangeEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		PitchRange = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(101, 0, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    		midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(100, 0, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    		midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(6, PitchRange, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}