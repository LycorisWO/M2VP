using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Input;
using M2VP;
using Microsoft.Win32;
using NAudio.Midi;

namespace M2VP
{
    public partial class MainWindow : Window
    {
        public static Dictionary<int, Note> noteLookup;
        private MidiToKeyboardConverter converter;
        private bool isDragging = false;
        private Point lastMousePosition;
        public MainWindow()
        {
            InitializeComponent();
            BuildNoteDictionary();
            converter = new MidiToKeyboardConverter(noteLookup, "output.txt");
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close(); // Close the window
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "MIDI files (*.mid)|*.mid"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                MidiFilePathTextBox.Text = openFileDialog.FileName;
            }
        }

        private void ConvertButton_Click(object sender, RoutedEventArgs e)
        {
            string midiFilePath = MidiFilePathTextBox.Text;
            if (!File.Exists(midiFilePath))
            {
                StatusTextBlock.Text = "MIDI file not found.";
                return;
            }

            string name = Path.GetFileNameWithoutExtension(midiFilePath);

            
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt",
                    FileName = $"{name}_output.txt"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    converter.ConvertMidiToKeyboardInput(midiFilePath, name, saveFileDialog.FileName);
                    StatusTextBlock.Text = "Conversion Successful!";
                }
            
        }

        private void BuildNoteDictionary()
        {
            noteLookup = new Dictionary<int, Note>();
            List<char> qwertyLookup = new List<char>()
            {
                '1','!','2','@','3','4','$','5','%','6','^','7',
                '8','*','9','(','0','q','Q','w','W','e','E','r',
                't','T','y','Y','u','i','I','o','O','p','P','a',
                's','S','d','D','f','g','G','h','H','j','J','k',
                'l','L','z','Z','x','c','C','v','V','b','B','n',
                'm'
            };
            int noteLookupOctaveTransposition = 3;

            for (int row = 0; row < 6; row++)
            {
                for (int column = 0; column < 12; column++)
                {
                    if (row == 5 && column == 1)
                    {
                        break;
                    }

                    if (!char.IsLetterOrDigit(qwertyLookup[row * 12 + column]) || char.IsUpper(qwertyLookup[row * 12 + column]))
                    {
                        noteLookup.Add((row + noteLookupOctaveTransposition) * 12 + column, new Note(qwertyLookup[(row * 12 + column) - 1], true));
                    }
                    else
                    {
                        noteLookup.Add((row + noteLookupOctaveTransposition) * 12 + column, new Note(qwertyLookup[row * 12 + column]));
                    }
                }
            }
            noteLookup.Add(455, new Note(' ', true));
        }

        private void Bar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }
    }
}
