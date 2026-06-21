using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using NDS.NitroSystem.SND.SSEQEvents;
using NAudio.Midi;
using LibEveryFileExplorer.IO;

namespace NDS.NitroSystem.SND
{
    public class SSEQDecoder
    {
        private List<SSEQEvent> ParsedEventList;
        private Dictionary<long, int> StreamPositionToEventIndexMap;
        private int DataReadStartByteOffset;

        public int LoopStartTickOffset = -1;
        public int LoopEndTickOffset = -1;

        public SSEQDecoder(byte[] rawSseqData, int DataReadStartByteOffset = 0)
        {
            this.DataReadStartByteOffset = DataReadStartByteOffset;
            EndianBinaryReader littleEndianReader = new EndianBinaryReader(new MemoryStream(rawSseqData), Endianness.LittleEndian);
            ParsedEventList = new List<SSEQEvent>();
            StreamPositionToEventIndexMap = new Dictionary<long, int>();

            while (littleEndianReader.BaseStream.Position != littleEndianReader.BaseStream.Length)
            {
                StreamPositionToEventIndexMap.Add(littleEndianReader.BaseStream.Position, ParsedEventList.Count);
                byte eventOpcode = littleEndianReader.ReadByte();

                if (eventOpcode < 128)
                {
                    ParsedEventList.Add(new SSEQNoteEvent(eventOpcode, littleEndianReader));
                    continue;
                }

                switch (eventOpcode)
                {
                    case 128:
                        ParsedEventList.Add(new SSEQDelayEvent(eventOpcode, littleEndianReader));
                        break;
                    case 129:
                        ParsedEventList.Add(new SSEQBankChangeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 147:
                        ParsedEventList.Add(new SSEQTrackEvent(eventOpcode, littleEndianReader));
                        break;
                    case 148:
                        ParsedEventList.Add(new SSEQJumpEvent(eventOpcode, littleEndianReader));
                        break;
                    case 149:
                        ParsedEventList.Add(new SSEQCallEvent(eventOpcode, littleEndianReader));
                        break;
                    case 160:
                        ParsedEventList.Add(new SSEQRandomEvent(eventOpcode, littleEndianReader));
                        break;
                    case 161:
                        ParsedEventList.Add(new SSEQVarEvent(eventOpcode, littleEndianReader));
                        break;
                    case 162:
                        ParsedEventList.Add(new SSEQIfEvent(eventOpcode, littleEndianReader));
                        break;
                    case 176:
                        ParsedEventList.Add(new SSEQSetVarEvent(eventOpcode, littleEndianReader));
                        break;
                    case 177:
                        ParsedEventList.Add(new SSEQAddVarEvent(eventOpcode, littleEndianReader));
                        break;
                    case 178:
                        ParsedEventList.Add(new SSEQSubVarEvent(eventOpcode, littleEndianReader));
                        break;
                    case 179:
                        ParsedEventList.Add(new SSEQMulVarEvent(eventOpcode, littleEndianReader));
                        break;
                    case 180:
                        ParsedEventList.Add(new SSEQDivVarEvent(eventOpcode, littleEndianReader));
                        break;
                    case 181:
                        ParsedEventList.Add(new SSEQShiftVarEvent(eventOpcode, littleEndianReader));
                        break;
                    case 182:
                        ParsedEventList.Add(new SSEQRandVarEvent(eventOpcode, littleEndianReader));
                        break;
                    case 183:
                        ParsedEventList.Add(new SSEQCmpEqEvent(eventOpcode, littleEndianReader));
                        break;
                    case 184:
                        ParsedEventList.Add(new SSEQCmpGeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 185:
                        ParsedEventList.Add(new SSEQCmpGtEvent(eventOpcode, littleEndianReader));
                        break;
                    case 186:
                        ParsedEventList.Add(new SSEQCmpLeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 187:
                        ParsedEventList.Add(new SSEQCmpLtEvent(eventOpcode, littleEndianReader));
                        break;
                    case 188:
                        ParsedEventList.Add(new SSEQCmpNeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 189:
                        ParsedEventList.Add(new SSEQRandVarEvent(eventOpcode, littleEndianReader));
                        break;
                    case 192:
                        ParsedEventList.Add(new SSEQPanEvent(eventOpcode, littleEndianReader));
                        break;
                    case 193:
                        ParsedEventList.Add(new SSEQVolumeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 194:
                        ParsedEventList.Add(new SSEQMasterVolumeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 195:
                        ParsedEventList.Add(new SSEQTransposeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 196:
                        ParsedEventList.Add(new SSEQPitchEvent(eventOpcode, littleEndianReader));
                        break;
                    case 197:
                        ParsedEventList.Add(new SSEQPitchRangeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 198:
                        ParsedEventList.Add(new SSEQTrackPriorityEvent(eventOpcode, littleEndianReader));
                        break;
                    case 199:
                        ParsedEventList.Add(new SSEQNoteWaitModeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 200:
                        ParsedEventList.Add(new SSEQTieEvent(eventOpcode, littleEndianReader));
                        break;
                    case 201:
                        ParsedEventList.Add(new SSEQPortamentoEvent(eventOpcode, littleEndianReader));
                        break;
                    case 202:
                        ParsedEventList.Add(new SSEQModulationDepthEvent(eventOpcode, littleEndianReader));
                        break;
                    case 203:
                        ParsedEventList.Add(new SSEQModulationSpeedEvent(eventOpcode, littleEndianReader));
                        break;
                    case 204:
                        ParsedEventList.Add(new SSEQModulationTypeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 205:
                        ParsedEventList.Add(new SSEQModulationRangeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 206:
                        ParsedEventList.Add(new SSEQPortamentoOnOffEvent(eventOpcode, littleEndianReader));
                        break;
                    case 207:
                        ParsedEventList.Add(new SSEQPortamentoTimeEvent(eventOpcode, littleEndianReader));
                        break;
                    case 208:
                        ParsedEventList.Add(new SSEQAttackRateEvent(eventOpcode, littleEndianReader));
                        break;
                    case 209:
                        ParsedEventList.Add(new SSEQDecayRateEvent(eventOpcode, littleEndianReader));
                        break;
                    case 210:
                        ParsedEventList.Add(new SSEQSustainRateEvent(eventOpcode, littleEndianReader));
                        break;
                    case 211:
                        ParsedEventList.Add(new SSEQReleaseRateEvent(eventOpcode, littleEndianReader));
                        break;
                    case 212:
                        ParsedEventList.Add(new SSEQLoopStartMarkerEvent(eventOpcode, littleEndianReader));
                        break;
                    case 213:
                        ParsedEventList.Add(new SSEQExpressionEvent(eventOpcode, littleEndianReader));
                        break;
                    case 214:
                        ParsedEventList.Add(new SSEQPrintVariableEvent(eventOpcode, littleEndianReader));
                        break;
                    case 224:
                        ParsedEventList.Add(new SSEQModulationDelayEvent(eventOpcode, littleEndianReader));
                        break;
                    case 225:
                        ParsedEventList.Add(new SSEQTempoEvent(eventOpcode, littleEndianReader));
                        break;
                    case 227:
                        ParsedEventList.Add(new SSEQSweepPitchEvent(eventOpcode, littleEndianReader));
                        break;
                    case 252:
                        ParsedEventList.Add(new SSEQLoopEndMarkerEvent(eventOpcode, littleEndianReader));
                        break;
                    case 253:
                        ParsedEventList.Add(new SSEQReturnEvent(eventOpcode, littleEndianReader));
                        break;
                    case 254:
                        ParsedEventList.Add(new SSEQAllocTrackEvent(eventOpcode, littleEndianReader));
                        break;
                    case byte.MaxValue:
                        ParsedEventList.Add(new SSEQEndOfTrackEvent(eventOpcode, littleEndianReader));
                        break;
                }
            }
            littleEndianReader.Close();
        }

        public IList<MidiEvent>[] GetTracks()
        {
            Dictionary<int, int> EventIndexToTimeCache = new Dictionary<int, int>();
            SSEQMidiResult[] midiTrackContainerArray = new SSEQMidiResult[16];
            short[] sharedGlobalVariableTable = new short[16];
            midiTrackContainerArray[0] = new SSEQMidiResult(0, sharedGlobalVariableTable);

            int activeTrackIndex = 0;
            int nextMidiChannelId = 0;
            Stack<int> trackCallPreviousTrackStack = new Stack<int>();
            Stack<int> trackCallReturnEventIndexStack = new Stack<int>();
            trackCallPreviousTrackStack.Push(0);
            trackCallReturnEventIndexStack.Push(ParsedEventList.Count);
            bool isJumpRedirectActive = false;
            int lastJumpTargetOffset = -1;

            for (int currentEventIndex = StreamPositionToEventIndexMap[DataReadStartByteOffset]; currentEventIndex < ParsedEventList.Count; currentEventIndex++)
            {
                if (!EventIndexToTimeCache.ContainsKey(currentEventIndex) && !isJumpRedirectActive)
                {
                    EventIndexToTimeCache.Add(currentEventIndex, midiTrackContainerArray[activeTrackIndex].CurrentTime);
                }

                if ((midiTrackContainerArray[activeTrackIndex].If && midiTrackContainerArray[activeTrackIndex].ComparisonResult) || !midiTrackContainerArray[activeTrackIndex].If)
                {
                    midiTrackContainerArray[activeTrackIndex].If = false;

                    if (ParsedEventList[currentEventIndex] is SSEQTrackEvent)
                    {
                        trackCallReturnEventIndexStack.Push(currentEventIndex);
                        trackCallPreviousTrackStack.Push(activeTrackIndex);
                        activeTrackIndex = ((SSEQTrackEvent)ParsedEventList[currentEventIndex]).TrackNr;
                        nextMidiChannelId++;

                        if (nextMidiChannelId == 9)
                        {
                            nextMidiChannelId++;
                        }

                        currentEventIndex = StreamPositionToEventIndexMap[((SSEQTrackEvent)ParsedEventList[currentEventIndex]).Offset] - 1;
                        midiTrackContainerArray[activeTrackIndex] = new SSEQMidiResult(nextMidiChannelId, sharedGlobalVariableTable);
                        midiTrackContainerArray[activeTrackIndex].CurrentTime = midiTrackContainerArray[trackCallPreviousTrackStack.Peek()].CurrentTime;
                        continue;
                    }

                    ParsedEventList[currentEventIndex].AddMidiEvents(ref midiTrackContainerArray[activeTrackIndex]);

                    if (midiTrackContainerArray[activeTrackIndex].Goto)
                    {
                        midiTrackContainerArray[activeTrackIndex].ReturnOffset = currentEventIndex;
                        currentEventIndex = StreamPositionToEventIndexMap[midiTrackContainerArray[activeTrackIndex].GotoOffset] - 1;
                        midiTrackContainerArray[activeTrackIndex].Goto = false;
                        midiTrackContainerArray[activeTrackIndex].GotoOffset = 0u;
                        isJumpRedirectActive = true;
                        continue;
                    }

                    if (midiTrackContainerArray[activeTrackIndex].Return)
                    {
                        currentEventIndex = midiTrackContainerArray[activeTrackIndex].ReturnOffset;
                        midiTrackContainerArray[activeTrackIndex].Return = false;
                        midiTrackContainerArray[activeTrackIndex].ReturnOffset = -1;
                        isJumpRedirectActive = false;
                    }

                    if (ParsedEventList[currentEventIndex] is SSEQJumpEvent && lastJumpTargetOffset != ((SSEQJumpEvent)ParsedEventList[currentEventIndex]).Offset)
                    {
                        lastJumpTargetOffset = (int)((SSEQJumpEvent)ParsedEventList[currentEventIndex]).Offset;
                        currentEventIndex = StreamPositionToEventIndexMap[((SSEQJumpEvent)ParsedEventList[currentEventIndex]).Offset] - 1;
                        isJumpRedirectActive = true;
                        continue;
                    }

                    if (ParsedEventList[currentEventIndex] is SSEQEndOfTrackEvent || (ParsedEventList[currentEventIndex] is SSEQJumpEvent && lastJumpTargetOffset == ((SSEQJumpEvent)ParsedEventList[currentEventIndex]).Offset))
                    {
                        sharedGlobalVariableTable = midiTrackContainerArray[activeTrackIndex].GlobalVariables;
                        midiTrackContainerArray[trackCallPreviousTrackStack.Peek()].GlobalVariables = sharedGlobalVariableTable;

                        if (midiTrackContainerArray[activeTrackIndex].TrackID == 9 && nextMidiChannelId != 10)
                        {
                            nextMidiChannelId--;
                        }

                        currentEventIndex = trackCallReturnEventIndexStack.Pop();
                        activeTrackIndex = trackCallPreviousTrackStack.Pop();
                    }

                    if (currentEventIndex == ParsedEventList.Count)
                    {
                        break;
                    }
                }
                else if (midiTrackContainerArray[activeTrackIndex].If && !midiTrackContainerArray[activeTrackIndex].ComparisonResult)
                {
                    midiTrackContainerArray[activeTrackIndex].If = false;
                }
            }

            List<IList<MidiEvent>> outputMidiTrackList = new List<IList<MidiEvent>>();
            SSEQMidiResult[] allMidiResultContainers = midiTrackContainerArray;

            foreach (SSEQMidiResult singleMidiResult in allMidiResultContainers)
            {
                if (singleMidiResult != null)
                {
                    outputMidiTrackList.Add(singleMidiResult.MidiTrack);
                }
            }

            return outputMidiTrackList.ToArray();
        }

        public void GetTreeNodes(TreeNodeCollection uiNodeCollection)
        {
            TreeNode rootMainTrackNode = new TreeNode("Track 0", 4, 4);
            uiNodeCollection.Add(rootMainTrackNode);

            Stack<int> trackTreePreviousTrackStack = new Stack<int>();
            Stack<int> trackTreeReturnEventIndexStack = new Stack<int>();
            trackTreePreviousTrackStack.Push(0);
            trackTreeReturnEventIndexStack.Push(ParsedEventList.Count);

            int currentTreeTrackId = 0;
            bool treeJumpRedirectFlag = false;
            int lastCallJumpEventIndex = -1;
            int lastDirectJumpOffset = -1;

            for (int currentEventIndex = StreamPositionToEventIndexMap[DataReadStartByteOffset]; currentEventIndex < ParsedEventList.Count; currentEventIndex++)
            {
                if (ParsedEventList[currentEventIndex] is SSEQTrackEvent)
                {
                    trackTreeReturnEventIndexStack.Push(currentEventIndex);
                    trackTreePreviousTrackStack.Push(currentTreeTrackId);

                    TreeNode childTrackUiNode = new TreeNode("Track " + ((SSEQTrackEvent)ParsedEventList[currentEventIndex]).TrackNr, 4, 4);
                    if (treeJumpRedirectFlag)
                    {
                        childTrackUiNode.Tag = -1;
                    }
                    else
                    {
                        childTrackUiNode.Tag = currentEventIndex;
                    }

                    rootMainTrackNode.Nodes.Add(childTrackUiNode);
                    rootMainTrackNode = childTrackUiNode;
                    currentTreeTrackId = ((SSEQTrackEvent)ParsedEventList[currentEventIndex]).TrackNr;
                    currentEventIndex = StreamPositionToEventIndexMap[((SSEQTrackEvent)ParsedEventList[currentEventIndex]).Offset] - 1;
                    continue;
                }

                rootMainTrackNode.Nodes.Add(ParsedEventList[currentEventIndex].GetTreeNode());

                if (ParsedEventList[currentEventIndex] is SSEQCallEvent)
                {
                    lastCallJumpEventIndex = currentEventIndex;
                    currentEventIndex = StreamPositionToEventIndexMap[((SSEQCallEvent)ParsedEventList[currentEventIndex]).Offset] - 1;
                    rootMainTrackNode = rootMainTrackNode.Nodes[rootMainTrackNode.Nodes.Count - 1];
                    continue;
                }

                if (ParsedEventList[currentEventIndex] is SSEQReturnEvent)
                {
                    currentEventIndex = lastCallJumpEventIndex;
                    lastCallJumpEventIndex = -1;
                    rootMainTrackNode = rootMainTrackNode.Parent;
                    continue;
                }

                if (ParsedEventList[currentEventIndex] is SSEQJumpEvent && lastDirectJumpOffset != ((SSEQJumpEvent)ParsedEventList[currentEventIndex]).Offset)
                {
                    lastDirectJumpOffset = (int)((SSEQJumpEvent)ParsedEventList[currentEventIndex]).Offset;
                    currentEventIndex = StreamPositionToEventIndexMap[((SSEQJumpEvent)ParsedEventList[currentEventIndex]).Offset] - 1;
                    rootMainTrackNode = rootMainTrackNode.Nodes[rootMainTrackNode.Nodes.Count - 1];
                    treeJumpRedirectFlag = true;
                    continue;
                }

                if (ParsedEventList[currentEventIndex] is SSEQEndOfTrackEvent || (ParsedEventList[currentEventIndex] is SSEQJumpEvent && lastDirectJumpOffset == ((SSEQJumpEvent)ParsedEventList[currentEventIndex]).Offset))
                {
                    if ((ParsedEventList[currentEventIndex] is SSEQJumpEvent && lastDirectJumpOffset == ((SSEQJumpEvent)ParsedEventList[currentEventIndex]).Offset) || treeJumpRedirectFlag)
                    {
                        rootMainTrackNode = rootMainTrackNode.Parent;
                    }

                    rootMainTrackNode = rootMainTrackNode.Parent;
                    lastDirectJumpOffset = -1;
                    treeJumpRedirectFlag = false;
                    currentEventIndex = trackTreeReturnEventIndexStack.Pop();
                    currentTreeTrackId = trackTreePreviousTrackStack.Pop();
                }

                if (currentEventIndex == ParsedEventList.Count)
                {
                    break;
                }
            }
        }
    }
}