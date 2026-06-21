using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQSweepPitchEvent : SSEQEvent
    {
    	public short SweepPitch { get; private set; }

    	public SSEQSweepPitchEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		SweepPitch = er.ReadInt16();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(28, 64 + SweepPitch / 2, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}

    	public override string ToString()
    	{
    		return "Sweep Pitch";
    	}
    }
}