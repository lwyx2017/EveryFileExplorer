using LibEveryFileExplorer.IO;
using System.Windows.Forms;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQCallEvent : SSEQEvent
    {
    	public uint Offset { get; private set; }

    	public SSEQCallEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Offset = (uint)IOUtil.ReadU24LE(er.ReadBytes(3), 0);
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.Goto = true;
    		Result.GotoOffset = Offset;
    	}

    	public override string ToString()
    	{
    		return "Call";
    	}

    	public override TreeNode GetTreeNode()
    	{
    		return new TreeNode(ToString(), 6, 6);
    	}
    }
}
