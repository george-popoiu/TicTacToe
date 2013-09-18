using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Multiplayer_Tic_Tac_Toe {
    public class Player : INotifyPropertyChanged {
        string playerName;

        public string PlayerName {
            get { return playerName; }
            set { SetProperty<string>(ref playerName, value); }
        }

        int won, lost;

        public int Lost {
            get { return lost; }
            set { SetProperty<int>(ref lost, value); }
        }

        public int Won {
            get { return won; }
            set { SetProperty<int>(ref won, value); }
        }

        public Player(string playerName = "", int won = 0, int lost = 0) {
            PlayerName = playerName;
            Won = won;
            Lost = lost;
        }

        public override string ToString() {
            return PlayerName + " -> " + Won + "," + Lost;
        }

        bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null) {
            if (object.Equals(storage, value)) {
                return false;
            }
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
