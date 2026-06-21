using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQAttackRateEvent : SSEQEvent
    {
    	public byte AttackRate { get; private set; }

    	public SSEQAttackRateEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		AttackRate = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(73, 64 + AttackRate / 2, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}