using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQPortamentoEvent : SSEQEvent
    {
    	public byte Portamento { get; private set; }

    	public SSEQPortamentoEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Portamento = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(84, Portamento, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}