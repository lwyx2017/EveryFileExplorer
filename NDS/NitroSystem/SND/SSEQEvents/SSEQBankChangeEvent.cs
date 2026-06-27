using LibEveryFileExplorer.IO;
using NAudio.Midi;
using System;
using System.Windows.Forms;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQBankChangeEvent : SSEQEvent
    {
        public int ActualProgramNumber { get; private set; }

        public SSEQBankChangeEvent(byte EventID, EndianBinaryReader endianReader)
        {
            base.EventID = EventID;
            ActualProgramNumber = endianReader.ReadVariableLength();
        }

        public override void AddMidiEvents(ref SSEQMidiResult midiConvertResult)
        {
            int patchIndex = ActualProgramNumber % 128;
            int bankSubNumber = (ActualProgramNumber / 128) & 0xF;
            int bankMainNumber = (ActualProgramNumber / 128 / 128) & 0xF;
            int midiChannel = midiConvertResult.TrackID + 1;
            if (midiChannel < 1) midiChannel = 1;
            if (midiChannel > 16) midiChannel = 16;
            MidiEvent bankMainCtrlEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(0, bankMainNumber, midiChannel).RawData);
            bankMainCtrlEvent.AbsoluteTime = midiConvertResult.CurrentTime;
            midiConvertResult.MidiTrack.Add(bankMainCtrlEvent);
            MidiEvent bankSubCtrlEvent = MidiEvent.FromRawMessage(MidiMessage.ChangeControl(32, bankSubNumber, midiChannel).RawData);
            bankSubCtrlEvent.AbsoluteTime = midiConvertResult.CurrentTime;
            midiConvertResult.MidiTrack.Add(bankSubCtrlEvent);
            midiConvertResult.MidiTrack.Add(new PatchChangeEvent(midiConvertResult.CurrentTime, midiChannel, patchIndex));

            if (patchIndex != 127)
            {
                return;
            }
            midiConvertResult.TrackID = 9;
            foreach (MidiEvent trackEventItem in midiConvertResult.MidiTrack)
            {
                trackEventItem.Channel = 10;
            }
        }

        public override TreeNode GetTreeNode()
        {
            return new TreeNode(ToString(), 5, 5);
        }
    }
}