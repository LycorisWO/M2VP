using WindowsInput;
using WindowsInput.Native;
using System.Windows.Input;

namespace M2VP
{
    public class Note
    {
        public readonly char NoteCharacter;
        public readonly VirtualKeyCode KeyCode;
        public readonly bool IsShiftedKey;
        private InputSimulator inputSim = new InputSimulator();

        private static readonly KeyConverter converter = new KeyConverter();
        private static readonly VirtualKeyCode[] shiftedKeyCodes = new VirtualKeyCode[]
        {
            VirtualKeyCode.OEM_1,    // ;:
            VirtualKeyCode.OEM_PLUS, // =+
            VirtualKeyCode.OEM_COMMA,// ,<
            VirtualKeyCode.OEM_MINUS,// -_
            VirtualKeyCode.OEM_PERIOD,// .>
            VirtualKeyCode.OEM_2,    // /?
            VirtualKeyCode.OEM_3,    // `~
            VirtualKeyCode.OEM_4,    // [{
            VirtualKeyCode.OEM_5,    // \|
            VirtualKeyCode.OEM_6,    // ]}
            VirtualKeyCode.OEM_7,    // '"
        };

        public Note(char noteCharacter, bool isShifted = false)
        {
            NoteCharacter = noteCharacter;
            IsShiftedKey = isShifted;

            if (isShifted)
            {
                KeyCode = GetShiftedKeyCode(noteCharacter);
            }
            else
            {
                Key key = (Key)converter.ConvertFromString(noteCharacter.ToString());
                KeyCode = (VirtualKeyCode)KeyInterop.VirtualKeyFromKey(key);
            }
        }

        private VirtualKeyCode GetShiftedKeyCode(char noteCharacter)
        {
            int index = noteCharacter - '!';
            if (index >= 0 && index < shiftedKeyCodes.Length)
            {
                return shiftedKeyCodes[index];
            }
            return VirtualKeyCode.SPACE; // Default to space if no shifted key found
        }

        public void Play()
        {
            if (!IsShiftedKey)
            {
                inputSim.Keyboard.KeyUp(KeyCode);
                inputSim.Keyboard.KeyDown(KeyCode);
            }
            else
            {
                inputSim.Keyboard.KeyDown(VirtualKeyCode.LSHIFT);
                inputSim.Keyboard.KeyUp(KeyCode);
                inputSim.Keyboard.KeyDown(KeyCode);
                inputSim.Keyboard.KeyUp(VirtualKeyCode.LSHIFT);
            }
        }

        public void Stop()
        {
            inputSim.Keyboard.KeyUp(KeyCode);
        }
    }
}
