using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQNoteWaitModeEvent : SSEQEvent
    {
    	public byte Mode { get; private set; }

    	public SSEQNoteWaitModeEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Mode = er.ReadByte();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.NoteWait = Mode == 1;
    	}
    }
}