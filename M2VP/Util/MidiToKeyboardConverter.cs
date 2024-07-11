using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using M2VP;
using NAudio.Midi;

namespace M2VP
{
    public class MidiToKeyboardConverter
    {
        private string outputPath;
        private int noteLookupOctaveTransposition = 3;
        public int tolerance = 1;
        private List<char> qwertyLookup = new List<char>()
        {
            '1','!','2','@','3','4','$','5','%','6','^','7',
            '8','*','9','(','0','q','Q','w','W','e','E','r',
            't','T','y','Y','u','i','I','o','O','p','P','a',
            's','S','d','D','f','g','G','h','H','j','J','k',
            'l','L','z','Z','x','c','C','v','V','b','B','n',
            'm'
        };

        public MidiToKeyboardConverter(Dictionary<int, Note> noteLookup, string outputPath)
        {
            this.outputPath = outputPath;
        }

        public void ConvertMidiToKeyboardInput(string midiFilePath, string name, string outputFilePath)
        {
            var midiFile = new MidiFile(midiFilePath, false);
            var outputLines = new List<string>();

            Dictionary<long, List<int>> simultaneousNotes = new Dictionary<long, List<int>>();

            bool hasNoteOnEvents = false;

            foreach (var trackChunk in midiFile.Events)
            {
                foreach (var midiEvent in trackChunk)
                {
                    if (midiEvent.CommandCode == MidiCommandCode.NoteOn)
                    {
                        var noteEvent = (NoteOnEvent)midiEvent;
                        if (noteEvent.Velocity > 0)
                        {
                            MainWindow.noteLookup.TryGetValue(noteEvent.NoteNumber, out Note note);
                            if (note != null)
                            {
                                long absoluteTime = noteEvent.AbsoluteTime;

                                if (!simultaneousNotes.ContainsKey(absoluteTime))
                                {
                                    simultaneousNotes[absoluteTime] = new List<int>();
                                }
                                simultaneousNotes[absoluteTime].Add(noteEvent.NoteNumber);

                                hasNoteOnEvents = true; // Mark that there are active NoteOn events
                            }
                        }
                        else
                        {
                            long absoluteTime = noteEvent.AbsoluteTime;
                            if (!simultaneousNotes.ContainsKey(absoluteTime))
                            {
                                simultaneousNotes[absoluteTime] = new List<int>();
                            }
                            simultaneousNotes[absoluteTime].Add(500); // Using 500 as a special marker for silence
                        }
                    }
                    else if (midiEvent.CommandCode == MidiCommandCode.NoteOff)
                    {
                        var noteEvent = (NoteEvent)midiEvent;
                        long absoluteTime = noteEvent.AbsoluteTime;
                        if (simultaneousNotes.ContainsKey(absoluteTime))
                        {
                            simultaneousNotes[absoluteTime].Remove(noteEvent.NoteNumber);
                            if (simultaneousNotes[absoluteTime].Count == 0)
                            {
                                simultaneousNotes.Remove(absoluteTime);
                            }
                        }
                    }
                }

                if (trackChunk == midiFile.Events.Last())
                {
                    string nt = "";

                    foreach (var time in simultaneousNotes.Keys.OrderBy(t => t))
                    {
                        var chordNotes = simultaneousNotes[time];
                        string chordString = GetChordRepresentation(chordNotes);
                        nt += chordString;
                    }

                    outputLines.Add(nt);
                    simultaneousNotes.Clear(); // Clear simultaneousNotes after processing

                    hasNoteOnEvents = false;
                }
            }
            File.WriteAllLines(outputFilePath, outputLines);
        }

        private string GetChordRepresentation(List<int> chordNotes)
        {
            if (chordNotes.Count == 0)
                return " "; // Return space if no notes are active

            List<string> noteStrings = new List<string>();
            List<int> sortedNotes = chordNotes.OrderBy(n => n).ToList(); // Sort notes ascending

            foreach (var noteNumber in sortedNotes)
            {
                if (MainWindow.noteLookup.TryGetValue(noteNumber, out Note note))
                {
                    string noteString = GetNoteString(note);
                    noteStrings.Add(noteString);
                }
                else
                {
                }
            }

            if (noteStrings.Count > 0)
            {
                string result = "";
                if (noteStrings.Count > 1)
                    result = $"[{string.Join("", noteStrings)}]";
                else
                    result = noteStrings[0];

                return result;
            }
            else
            {
                return " "; // Return space if no valid notes were found
            }
        }

        private string GetNoteString(Note note)
        {
            if (note == null)
                return "";

            if (note.IsShiftedKey)
            {
                switch (note.NoteCharacter)
                {
                    case '1':
                        return "!";
                    case '2':
                        return "@";
                    case '3':
                        return "#";
                    case '4':
                        return "$";
                    case '5':
                        return "%";
                    case '6':
                        return "^";
                    case '7':
                        return "&";
                    case '8':
                        return "*";
                    case '9':
                        return "(";
                    case '0':
                        return ")";
                    default:
                        return note.NoteCharacter.ToString().ToUpper(); // Fallback to original character
                }
            }
            else
            {
                return note.NoteCharacter.ToString().ToLower(); // Convert to lowercase by default
            }
        }

        public static string ProcessSpace(string outputLines, int tolerance)
        {
            StringBuilder processedString = new StringBuilder();

            for (int i = 0; i < outputLines.Length; i++)
            {
                if (outputLines[i] == ' ' && (i == 0 || outputLines[i - 1] == ' '))
                {
                    tolerance--;
                    if (tolerance < 0)
                        continue; // Skip this space
                }
                else if (outputLines[i] == ' ')
                {
                    continue;
                }

                processedString.Append(outputLines[i]);

                if (outputLines[i] == ' ')
                {
                    tolerance++;
                }
            }

            return processedString.ToString();
        }
    }
}


