using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQModulationDepthEvent : SSEQEvent
    {
    	public byte Depth { get; private set; }

    	public SSEQModulationDepthEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Depth = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.MidiTrack.Add(new ControlChangeEvent(Result.CurrentTime, Result.TrackID + 1, MidiController.Modulation, Depth));
    	}
    }
}