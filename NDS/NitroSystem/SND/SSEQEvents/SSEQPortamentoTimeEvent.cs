using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQPortamentoTimeEvent : SSEQEvent
    {
    	public byte Time { get; private set; }

    	public SSEQPortamentoTimeEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Time = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(5, Time, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}