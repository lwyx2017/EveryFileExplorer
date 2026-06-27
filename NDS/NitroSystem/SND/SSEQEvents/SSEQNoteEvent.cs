using System.Windows.Forms;
using LibEveryFileExplorer.IO;
using NAudio.Midi;

namespace NDS.NitroSystem.SND.SSEQEvents
{
    public class SSEQNoteEvent : SSEQEvent
    {
        private string[] NoteNameTable = new string[12]
        {
            "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A",
            "A#", "B"
        };

        public byte NoteVelocity { get; private set; }

        public int NoteDurationTicks { get; private set; }

        public SSEQNoteEvent(byte EventID, EndianBinaryReader endianReader)
        {
            base.EventID = EventID;
            NoteVelocity = endianReader.ReadByte();
            NoteDurationTicks = endianReader.ReadVariableLength();
        }

        public SSEQNoteEvent(NoteOnEvent midiNoteOnEvent)
        {
            base.EventID = (byte)midiNoteOnEvent.NoteNumber;
            NoteDurationTicks = midiNoteOnEvent.NoteLength;
            NoteVelocity = (byte)midiNoteOnEvent.Velocity;
        }

        public override void AddMidiEvents(ref SSEQMidiResult midiConvertResult)
        {
            midiConvertResult.MidiTrack.Add(new NoteEvent(midiConvertResult.CurrentTime, midiConvertResult.TrackID + 1, MidiCommandCode.NoteOn, base.EventID, Clamp(NoteVelocity, 0, 127)));
            if (NoteDurationTicks != 0)
            {
                midiConvertResult.MidiTrack.Add(new NoteEvent(midiConvertResult.CurrentTime + NoteDurationTicks, midiConvertResult.TrackID + 1, MidiCommandCode.NoteOff, base.EventID, 64));
            }
            else
            {
                midiConvertResult.MidiTrack.Add(new NoteEvent(midiConvertResult.CurrentTime + 5000, midiConvertResult.TrackID + 1, MidiCommandCode.NoteOff, base.EventID, 64));
            }
            if (midiConvertResult.NoteWait)
            {
                midiConvertResult.CurrentTime += NoteDurationTicks;
            }
        }

        private int Clamp(int inputValue, int minLimit, int maxLimit)
        {
            return inputValue < minLimit ? minLimit : inputValue > maxLimit ? maxLimit : inputValue;
        }

        public override string ToString()
        {
            int noteOctave = base.EventID / 12 - 1;
            int noteIndexInOctave = base.EventID % 12;
            string noteName = NoteNameTable[noteIndexInOctave];
            return "Note" + " (" + noteName + noteOctave + ")";
        }

        public override TreeNode GetTreeNode()
        {
            return new TreeNode(ToString(), 1, 1);
        }
    }
}