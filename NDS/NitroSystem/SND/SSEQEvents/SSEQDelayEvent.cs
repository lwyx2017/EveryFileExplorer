using LibEveryFileExplorer.IO;
using System.Windows.Forms;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQDelayEvent : SSEQEvent
    {
    	public int Delay { get; private set; }

    	public SSEQDelayEvent(byte EventID, EndianBinaryReader er)
    	{
    		base.EventID = EventID;
    		Delay = er.ReadVariableLength();
    	}

    	public override void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    		Result.CurrentTime += Delay;
    	}

    	public override string ToString()
    	{
    		return ("Delay") + " (" + Delay + ")";
    	}

    	public override TreeNode GetTreeNode()
    	{
    		return new TreeNode(ToString(), 3, 3);
    	}
    }
}
