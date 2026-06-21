using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQPortamentoOnOffEvent : SSEQEvent
    {
    	public byte Mode { get; private set; }

    	public SSEQPortamentoOnOffEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Mode = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(65, Mode, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}