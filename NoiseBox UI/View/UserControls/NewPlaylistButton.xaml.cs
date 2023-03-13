using NoiseBox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NoiseBox_UI.View.UserControls
{
    public partial class NewPlaylistButton : UserControl
    {
        public NewPlaylistButton()
        {
            InitializeComponent();
        }

        private void AddButtonClick(object sender, RoutedEventArgs e) {
            EnterNamePopup.IsOpen = true;
            PopupTextBox.Focus();
        }

        private void PopupTextBox_KeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.Enter) {
                if (!string.IsNullOrEmpty(PopupTextBox.Text)) {
                    MainWindow win = (MainWindow)Window.GetWindow(this);

                    string popupTextBoxText = PopupTextBox.Text.Trim();

                    if (!win.PlaylistList.List.Items.Contains(popupTextBoxText)) {
                        win.PlaylistList.List.Items.Add(popupTextBoxText);

                        var playlistToAdd = new Playlist { Name = popupTextBoxText };

                        MusicLibrary.AddPlaylist(playlistToAdd);

                        win.SelectPlaylistByName(popupTextBoxText);
                    }

                    PopupTextBox.Text = "";
                    EnterNamePopup.IsOpen = false;
                }
            }
        }
    }
}