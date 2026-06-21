using System.Collections.Generic;
using LibEveryFileExplorer.IO;
using NDS.NitroSystem.SND.SSEQEvents;

namespace NDS.NitroSystem.SND
{
    public class SSEQTrack
    {
    	private List<SSEQEvent> Events = new List<SSEQEvent>();

    	private Dictionary<long, int> EventOffsets = new Dictionary<long, int>();

    	public int TrackID { get; private set; }

    	public SSEQTrack(EndianBinaryReader er, int TrackID)
    	{
    		this.TrackID = TrackID;
    		while (true)
    		{
    			//bool flag = true;
    			EventOffsets.Add(er.BaseStream.Position, Events.Count);
    			byte b = er.ReadByte();
    			if (b < 128)
    			{
    				Events.Add(new SSEQNoteEvent(b, er));
    				continue;
    			}
    			switch (b)
    			{
    			case 128:
    				Events.Add(new SSEQDelayEvent(b, er));
    				break;
    			case 129:
    				Events.Add(new SSEQBankChangeEvent(b, er));
    				break;
    			case 148:
    				Events.Add(new SSEQJumpEvent(b, er));
    				break;
    			case 149:
    				Events.Add(new SSEQCallEvent(b, er));
    				break;
    			case 192:
    				Events.Add(new SSEQPanEvent(b, er));
    				break;
    			case 193:
    				Events.Add(new SSEQVolumeEvent(b, er));
    				break;
    			case 194:
    				Events.Add(new SSEQMasterVolumeEvent(b, er));
    				break;
    			case 195:
    				Events.Add(new SSEQTransposeEvent(b, er));
    				break;
    			case 196:
    				Events.Add(new SSEQPitchEvent(b, er));
    				break;
    			case 197:
    				Events.Add(new SSEQPitchRangeEvent(b, er));
    				break;
    			case 198:
    				Events.Add(new SSEQTrackPriorityEvent(b, er));
    				break;
    			case 199:
    				Events.Add(new SSEQNoteWaitModeEvent(b, er));
    				break;
    			case 200:
    				Events.Add(new SSEQTieEvent(b, er));
    				break;
    			case 201:
    				Events.Add(new SSEQPortamentoEvent(b, er));
    				break;
    			case 202:
    				Events.Add(new SSEQModulationDepthEvent(b, er));
    				break;
    			case 203:
    				Events.Add(new SSEQModulationSpeedEvent(b, er));
    				break;
    			case 204:
    				Events.Add(new SSEQModulationTypeEvent(b, er));
    				break;
    			case 205:
    				Events.Add(new SSEQModulationRangeEvent(b, er));
    				break;
    			case 206:
    				Events.Add(new SSEQPortamentoOnOffEvent(b, er));
    				break;
    			case 207:
    				Events.Add(new SSEQPortamentoTimeEvent(b, er));
    				break;
    			case 208:
    				Events.Add(new SSEQAttackRateEvent(b, er));
    				break;
    			case 209:
    				Events.Add(new SSEQDecayRateEvent(b, er));
    				break;
    			case 210:
    				Events.Add(new SSEQSustainRateEvent(b, er));
    				break;
    			case 211:
    				Events.Add(new SSEQReleaseRateEvent(b, er));
    				break;
    			case 212:
    				Events.Add(new SSEQLoopStartMarkerEvent(b, er));
    				break;
    			case 213:
    				Events.Add(new SSEQExpressionEvent(b, er));
    				break;
    			case 214:
    				Events.Add(new SSEQPrintVariableEvent(b, er));
    				break;
    			case 224:
    				Events.Add(new SSEQModulationDelayEvent(b, er));
    				break;
    			case 225:
    				Events.Add(new SSEQTempoEvent(b, er));
    				break;
    			case 252:
    				Events.Add(new SSEQLoopEndMarkerEvent(b, er));
    				break;
    			case 253:
    				Events.Add(new SSEQReturnEvent(b, er));
    				break;
    			case byte.MaxValue:
    				Events.Add(new SSEQEndOfTrackEvent(b, er));
    				return;
    			}
    		}
    	}
    }
}