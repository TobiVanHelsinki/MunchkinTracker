using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TLIB.PlatformHelper;

namespace MunchkinUWP.Model
{
    public class Munchkin : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return this._Name + " " + this._Level + " " + this._Power;
        }

        internal Munchkin(string Name, int Level, int Gear, int ordering, GenderTyp Gender = 0, string Notes = "")
        {
            this.Name = Name;
            this.Level = Level;
            this.Gear = Gear;
            this.Notes = Notes;
            Order = ordering;
            this.Gender = Gender;
        }
        internal Munchkin()
        {

        }

        int _Level = 1;
        public int Level
        {
            get { return _Level; }
            set
            {
                if (value != _Level)
                {
                    if (SettingsModel.I.GAMEWARNINGS && _Level < value && value >= SettingsModel.I.GAMEWARNINGS_LEVEL)
                    {
                        ;
                        AppModel.Instance.NewNotification(StringHelper.GetString("Attention") +", " + _Name + " "+ StringHelper.GetString("is")+" "+StringHelper.GetString("Munchkin_Level/Text") +" " + value + "!");
                    }
                    _Level = value;
                    Power = Level + Gear;
                    HasWon = _Level >= 10 ? true : false;
                    NotifyPropertyChanged();
                }
            }
        }
        int _Gear = 0;
        public int Gear
        {
            get { return _Gear; }
            set
            {
                if (value != _Gear)
                {
                    _Gear = value;
                    Power = Level + Gear;
                    NotifyPropertyChanged();
                }
            }
        }
        int _Power = 0;
        public int Power
        {
            get { _Power = Level + Gear;  return _Power; }
            set
            {
                if (value != _Power)
                {
                    _Power = value;
                    NotifyPropertyChanged();
                }
            }
        }

        string _Name = "";
        public string Name
        {
            get { return _Name; }
            set
            {
                if (value != _Name)
                {
                    _Name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public enum GenderTyp {
            m = 1,
            w = 2,
            s = 3
        }

        GenderTyp _Gender = GenderTyp.s;
        public GenderTyp Gender
        {
            get { return _Gender; }
            set
            {
                if (value != _Gender)
                {
                    _Gender = value;
                    NotifyPropertyChanged();
                }
            }
        }
        int _Order;
        public int Order
        {
            get { return _Order; }
            set
            {
                if (value != _Order)
                {
                    _Order = value;
                    NotifyPropertyChanged();
                }
            }
        }
        string _Notes = "";
        public string Notes
        {
            get { return _Notes; }
            set
            {
                if (value != _Notes)
                {
                    _Notes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        bool _IsBattle = false;
        public bool IsBattle
        {
            get { return _IsBattle; }
            set
            {
                if (value != _IsBattle)
                {
                    _IsBattle = value;
                    NotifyPropertyChanged();
                }
            }
        }

        bool _HasWon = false;
        public bool HasWon
        {
            get { return _HasWon; }
            set
            {
                if (value != _HasWon)
                {
                    _HasWon = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        internal void ClearForNewGame()
        {
            this.IsBattle = false;
            this.Notes = "";
            this.Gear = 0;
            this.Level = 1;
            this.Power = 1;
        }
        internal void Clear()
        {
            ClearForNewGame();
            this.Name = "";
            this.Gender = GenderTyp.s;
        }
    }
}
