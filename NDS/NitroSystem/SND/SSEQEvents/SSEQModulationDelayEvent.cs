using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQModulationDelayEvent : SSEQEvent
    {
    	public short ModulationDelay { get; private set; }

    	public SSEQModulationDelayEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		ModulationDelay = er.ReadInt16();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(78, 64 + ModulationDelay / 2, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}