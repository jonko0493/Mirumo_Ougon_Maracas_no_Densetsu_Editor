using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace Mirumo_Ougon_Maracas_no_Densetsu_Editor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Dictionary<int, Message> _messagesMap = new();
        private List<MessageSection> _messageSections = new();
        private string _currentFile;

        private const int FIRST_MESSAGE_SECTION_POINTER = 0x0011265C;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void FileMenuOpenButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "GBA ROM file|*.gba"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                using FileStream stream = new FileStream(openFileDialog.FileName, FileMode.Open);
                stream.Seek(FIRST_MESSAGE_SECTION_POINTER, SeekOrigin.Begin);
                do
                {
                    _messageSections.Add(MessageSection.ParseFromStream(stream, _messagesMap));
                } while (_messageSections.Last().MessageBoxes.Count > 0);
                _messageSections.RemoveAt(_messageSections.Count - 1);

                messageSectionListBox.ItemsSource = _messageSections;
                _currentFile = openFileDialog.FileName;
            }
        }

        private void FileMenuSaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentFile))
            {
                using FileStream stream = new FileStream(_currentFile, FileMode.Open);
                foreach (Message message in _messagesMap.Values)
                {
                    stream.Seek(message.Pointer, SeekOrigin.Begin);
                    stream.Write(message.GetBytes());
                }
                foreach (MessageSection messageSection in _messageSections)
                {
                    messageSection.WriteMessageBoxesToStream(stream);
                }
            }
        }

        private void FileMenuExtractButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "GBA ROM file|*.gba"
            };
            if (openFileDialog.ShowDialog() == true)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Text file|*.txt"
                };
                if (saveFileDialog.ShowDialog() == true)
                {
                    byte[] file = File.ReadAllBytes(openFileDialog.FileName);
                    List<string> translation = new();

                    for (int i = 0; i < file.Length - 1; i += 2)
                    {
                        ushort character = BitConverter.ToUInt16(file.Skip(i).Take(2).ToArray());
                        if (Message.CharMap.ContainsKey(character))
                        {
                            translation.Add(Message.CharMap[character]);
                        }
                        else
                        {
                            translation.Add(" ");
                        }
                    }

                    using StreamWriter streamWriter = new(saveFileDialog.FileName);
                    streamWriter.WriteLine($"Offset    00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F\n");
                    for (int i = 0; i < file.Length - 15; i += 16)
                    {
                        streamWriter.Write($"{i:X8}  {file[i]:X2}  {file[i + 1]:X2}  {file[i + 2]:X2}  {file[i + 3]:X2}  {file[i + 4]:X2}  {file[i + 5]:X2}" +
                            $"  {file[i + 6]:X2}  {file[i + 7]:X2}  {file[i + 8]:X2}  {file[i + 9]:X2}  {file[i + 10]:X2}  {file[i + 11]:X2}  {file[i + 12]:X2}" +
                            $"  {file[i + 13]:X2}  {file[i + 14]:X2}  {file[i + 15]:X2}");
                        streamWriter.Write("  |  ");
                        streamWriter.WriteLine($"{translation[i / 2]}{translation[i / 2 + 1]}{translation[i / 2 + 2]}{translation[i / 2 + 3]}{translation[i / 2 + 4]}" +
                            $"{translation[i / 2 + 5]}{translation[i / 2 + 6]}{translation[i / 2 + 7]}");
                    }
                }
            }
        }

        private void MessageSectionListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (messageSectionListBox.SelectedIndex == -1)
            {
                messageBoxListBox.Items.Clear();
            }
            else
            {
                messageBoxListBox.ItemsSource = ((MessageSection)messageSectionListBox.SelectedItem).MessageBoxes;
            }
        }

        private void MessageBoxListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            messageStackPanel.Children.Clear();
            if (messageBoxListBox.SelectedIndex >= 0)
            {
                foreach (Message message in ((MessageBox)messageBoxListBox.SelectedItem).Messages)
                {
                    var messageTextBox = new MessageTextBox { Text = message.Value, Message = message, MaxLength = 0x16 };
                    messageTextBox.TextChanged += MessageTextBox_TextChanged;
                    messageStackPanel.Children.Add(messageTextBox);
                }
            }
        }

        private void MessageTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (MessageTextBox)sender;
            if (textBox.Text.Length < 0x17)
            {
                textBox.Message.Value = textBox.Text;
            }
            messageBoxListBox.Items.Refresh();
        }
    }
}
