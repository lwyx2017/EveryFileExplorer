using LibEveryFileExplorer.IO;
using System.IO;
using System.Windows.Forms;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQReturnEvent : SSEQEvent
    {
    	public SSEQReturnEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.Return = true;
    	}

    	public override string ToString()
    	{
    		return "Return";
    	}

    	public override TreeNode GetTreeNode()
    	{
    		return new TreeNode(ToString(), 7, 7);
    	}
    }
}
