using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQTransposeEvent : SSEQEvent
    {
    	public byte Transpose { get; private set; }

    	public SSEQTransposeEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Transpose = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(101, 0, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    		midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(100, 2, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    		midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(6, Transpose + 64, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}