using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQPrintVariableEvent : SSEQEvent
    {
    	public byte PrintVariable { get; private set; }

    	public SSEQPrintVariableEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		PrintVariable = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    	}
    }
}