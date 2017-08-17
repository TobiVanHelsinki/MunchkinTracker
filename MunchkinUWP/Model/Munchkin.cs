using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace MunchkinUWP.Model
{
    public class Munchkin : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return this._strName + " " + this._nLevel + " " + this._nPower;
        }

        public Munchkin(string Name, int Level, int Gear, int ordering, eGenderTyp Gender = 0, string Notes = "")
        {
            strName = Name;
            nLevel = Level;
            nGear = Gear;
            strNotes = Notes;
            nOrder = ordering;
            eGender = Gender;
        }
        public Munchkin()
        {

        }

        private int _nLevel = 1;
        public int nLevel
        {
            get { return _nLevel; }
            set
            {
                if (value != _nLevel)
                {
                    if (_nLevel < value && value >= 7 && _strName == "Luca")
                    {
                        AppModel.Instance.NewNotification("Achtung, " + _strName + " ist Level " + value + "!");
                    }
                    _nLevel = value;
                    nPower = nLevel + nGear;
                    bHasWon = _nLevel >= 10 ? true : false;
                    NotifyPropertyChanged();
                }
            }
        }
        private int _nGear = 0;
        public int nGear
        {
            get { return _nGear; }
            set
            {
                if (value != _nGear)
                {
                    _nGear = value;
                    nPower = nLevel + nGear;
                    NotifyPropertyChanged();
                }
            }
        }
        private int _nPower = 0;
        public int nPower
        {
            get { _nPower = nLevel + nGear;  return _nPower; }
            set
            {
                if (value != _nPower)
                {
                    _nPower = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _strName = "";
        public string strName
        {
            get { return _strName; }
            set
            {
                if (value != _strName)
                {
                    _strName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public enum eGenderTyp {
            m = 1,
            w = 2,
            s = 3
        }

        private eGenderTyp _eGender = eGenderTyp.s;
        public eGenderTyp eGender
        {
            get { return _eGender; }
            set
            {
                if (value != _eGender)
                {
                    _eGender = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private int _nOrder;
        public int nOrder
        {
            get { return _nOrder; }
            set
            {
                if (value != _nOrder)
                {
                    _nOrder = value;
                    NotifyPropertyChanged();
                }
            }
        }
        private string _strNotes = "";
        public string strNotes
        {
            get { return _strNotes; }
            set
            {
                if (value != _strNotes)
                {
                    _strNotes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _bIsBattle = false;
        public bool bIsBattle
        {
            get { return _bIsBattle; }
            set
            {
                if (value != _bIsBattle)
                {
                    _bIsBattle = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool _bHasWon = false;
        public bool bHasWon
        {
            get { return _bHasWon; }
            set
            {
                if (value != _bHasWon)
                {
                    _bHasWon = value;
                    NotifyPropertyChanged();
                }
            }
        }
        
        public void ClearForNewGame()
        {
            this.bIsBattle = false;
            this.strNotes = "";
            this.nGear = 0;
            this.nLevel = 1;
            this.nPower = 1;
        }
        public void Clear()
        {
            ClearForNewGame();
            this.strName = "";
            this.eGender = eGenderTyp.s;
        }
    }
}
