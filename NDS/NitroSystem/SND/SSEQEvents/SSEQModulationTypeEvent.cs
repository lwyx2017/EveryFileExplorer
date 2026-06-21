using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQModulationTypeEvent : SSEQEvent
    {
    	public byte Type { get; private set; }

    	public SSEQModulationTypeEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Type = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    	}
    }
}