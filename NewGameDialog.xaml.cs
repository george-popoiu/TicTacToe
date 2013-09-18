using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Multiplayer_Tic_Tac_Toe {
    public sealed partial class NewGameDialog : UserControl{

        public NewGameDialog() {
            this.InitializeComponent();
            Player1TB = player1Text;
            Player2TB = player2Text;
            PlayButton = playButton;
            ErrorText = errorText;
        }

        public TextBox Player1TB, Player2TB;
        public Button PlayButton;
        public TextBlock ErrorText;
    }
}
