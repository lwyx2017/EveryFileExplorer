using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQReleaseRateEvent : SSEQEvent
    {
    	public byte ReleaseRate { get; private set; }

    	public SSEQReleaseRateEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		ReleaseRate = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		MidiEvent midiEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(72, 64 + ReleaseRate / 2, Result.TrackID + 1).RawData);
    		midiEvent.AbsoluteTime = Result.CurrentTime;
    		Result.MidiTrack.Add(midiEvent);
    	}
    }
}