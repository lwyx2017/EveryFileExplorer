using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQSustainRateEvent : SSEQEvent
    {
    	public byte SustainRate { get; private set; }

    	public SSEQSustainRateEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		SustainRate = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    	}
    }
}