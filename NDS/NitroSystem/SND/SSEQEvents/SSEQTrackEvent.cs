using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQTrackEvent : SSEQEvent
    {
    	public uint Offset { get; private set; }

    	public byte TrackNr { get; private set; }

    	public SSEQTrackEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		TrackNr = er.ReadByte();
    		Offset = (uint)IOUtil.ReadU24LE(er.ReadBytes(3), 0);
    	}

    	public SSEQTrackEvent(int TrackNr)
    	{
    		base.EventID = 147;
    		this.TrackNr = (byte)TrackNr;
    	}

    	public void SetOffset(uint Offset)
    	{
    		this.Offset = Offset;
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    	}
    }
}