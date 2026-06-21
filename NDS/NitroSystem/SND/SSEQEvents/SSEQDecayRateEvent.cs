using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQDecayRateEvent : SSEQEvent
    {
    	public byte DecayRate { get; private set; }

    	public SSEQDecayRateEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		DecayRate = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(75, 64 + DecayRate / 2, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}