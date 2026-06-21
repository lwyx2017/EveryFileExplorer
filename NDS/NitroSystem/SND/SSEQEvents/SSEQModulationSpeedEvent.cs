using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQModulationSpeedEvent : SSEQEvent
    {
    	public byte Speed { get; private set; }

    	public SSEQModulationSpeedEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Speed = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(76, 64 + Speed / 2, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}