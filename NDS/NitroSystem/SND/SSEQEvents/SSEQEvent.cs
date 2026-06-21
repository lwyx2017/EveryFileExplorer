using System.Windows.Forms;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQEvent
    {
    	public byte EventID { get; protected set; }

    	public virtual void AddMidiEvents(ref SSEQMidiResult Result)
    	{
    	}

    	public override string ToString()
    	{
    		return GetType().Name;
    	}

    	public virtual TreeNode GetTreeNode()
    	{
    		return new TreeNode(ToString(), 9, 9);
    	}
    }
}