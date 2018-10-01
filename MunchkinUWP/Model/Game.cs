using MunchkinUWP.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TAPPLICATION.IO;
using TAPPLICATION.Model;
using TLIB.IO;
using TLIB.PlatformHelper;

namespace MunchkinUWP.Model
{
    public enum MunchkinOrder
    {
        undef = 0,
        ABC = 10,
        ABC_Reverse = 11,
        LvL = 20,
        LvL_Reverse = 21,
        Pwr = 30,
        Pwr_Reverse = 31,
        Reihe = 40,
    }
    public class Game : INotifyPropertyChanged, IMainType
    {
        public event PropertyChangedEventHandler PropertyChanged;
        internal event PropertyChangedEventHandler OrderChanged;
        internal event PropertyChangedEventHandler CurrentMunchkChanged;
        //=============================================================================
        public event EventHandler SaveRequest;
        bool _HasChanges = false;
        public bool HasChanges { get => _HasChanges;  set { _HasChanges = value; } }

        System.Threading.Timer Tim;

        void AnyPropertyChanged()
        {
            HasChanges = true;
            Tim.Change(Constants.STD_AUTOSAVE_INTERVAL, System.Threading.Timeout.Infinite);
        }
        //=============================================================================

        void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            AnyPropertyChanged();
        }
        void NotifyOrderChanged([CallerMemberName] string propertyName = "")
        {
            OrderChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        void NotifyCurrentMunchkChanged([CallerMemberName] string propertyName = "")
        {
            CurrentMunchkChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //=============================================================================
        public ObservableCollection<Munchkin> Munchkin;
        public ObservableCollection<RandomResult> Statistics;

        Munchkin _CurrentMunchkin;
        public Munchkin CurrentMunchkin
        {
            get { return _CurrentMunchkin; }
            set
            {
                if (value == null)
                {
                    if (Munchkin.Contains(_CurrentMunchkin))
                    {
                        // element still exists, all is ok, no need to do something
                    }
                    else
                    {//element is gone, need to find a new
                        try
                        {
                            _CurrentMunchkin = Munchkin.First();
                        }
                        catch (Exception)
                        {//there are no elements left, we have to assign null
                            _CurrentMunchkin = null;
                        }
                        NotifyCurrentMunchkChanged();
                        NotifyPropertyChanged();
                    }
                }
                else
                {
                    if (value != _CurrentMunchkin)
                    {
                        _CurrentMunchkin = value;
                        NotifyCurrentMunchkChanged();
                        NotifyPropertyChanged();
                    }
                }
            }
        }

        MunchkinOrder _CurrentOrder;
        public MunchkinOrder CurrentOrder
        {
            get
            {
                return _CurrentOrder;
            }
            set
            {
                //Order(_eCurrentOrder);
                if (_CurrentOrder != value)
                {
                    _CurrentOrder = value;
                    NotifyOrderChanged();
                }
            }
        }
        
        //=============================================================================
        int _MonsterPower;
        public int MonsterPower
        {
            get
            {
                return _MonsterPower;
            }
            set
            {
                _MonsterPower = value;
                NotifyPropertyChanged();
            }
        }
        int _MunchkinPower;
        public int MunchkinPower
        {
            get
            {
                return _MunchkinPower;
            }
            set
            {
                _MunchkinPower = value;
                NotifyPropertyChanged();
            }
        }
        int _MunchkinPowerMod;
        public int MunchkinPowerMod
        {
            get
            {
                return _MunchkinPowerMod;
            }
            set
            {
                _MunchkinPowerMod = value;
                NotifyPropertyChanged();
                CalculateBattle();
            }
        }
        bool? _UseLevel = true;
        public bool? UseLevel
        {
            get
            {
                return _UseLevel;
            }
            set
            {
                _UseLevel = value;
                NotifyPropertyChanged();
                CalculateBattle();
            }
        }
        bool? _UseGear = true;
        public bool? UseGear
        {
            get
            {
                return _UseGear;
            }
            set
            {
                _UseGear = value;
                CalculateBattle();
                NotifyPropertyChanged();
            }
        }
        //=============================================================================
        uint _RandomMax = Constants.STD_RANDOM_MAX;
        [Newtonsoft.Json.JsonIgnore]
        internal int RandomMax
        {
            get
            {
                return (int)_RandomMax;
            }
            set
            {
                if (value < 2)
                {
                    _RandomMax = 2;
                }
                else
                {
                    _RandomMax = (uint)value;
                }
                NotifyPropertyChanged();
            }
        }
        //SOUNDBOARD ##########################################################
        [Newtonsoft.Json.JsonIgnore]
        internal ObservableCollection<Sound> SoundList = new ObservableCollection<Sound>();

        //FILE HANDLING #######################################################
        public string APP_VERSION_NUMBER => Constants.APP_VERSION;

        public string FILE_VERSION_NUMBER => Constants.FILE_VERSION;

        [Newtonsoft.Json.JsonIgnore]
        public FileInfoClass FileInfo { get; set; } = AppModel.SaveGamePlace;

        public string MakeName()
        {
            return Constants.SAVE_GAME;
        }

        //=============================================================================
        public Game()
        {
            SoundList.Add(new Sound() { Name = StringHelper.GetString("Sound_Badum"), SoundName = Sound.eSoundName.badumtshh, Description = "" });
            SoundList.Add(new Sound() { Name = StringHelper.GetString("Sound_Wilhelm"), SoundName = Sound.eSoundName.WilhelmScream, Description = "" });
            //SoundList.Add(new Sound() { Name = StringHelper.GetString("Sound_Chord"), SoundName = Sound.eSoundName.Chord, Description = "" });

            Tim = new System.Threading.Timer((x)=> {
                SaveRequest?.Invoke(x, new EventArgs()); HasChanges = false; },this, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            Munchkin = new ObservableCollection<Munchkin>();
            Munchkin.CollectionChanged += Munchkin_CollectionChanged;
            Statistics = new ObservableCollection<RandomResult>();
            Statistics.CollectionChanged += (x, y) => AnyPropertyChanged();
        }

        //=============================================================================
        internal void AddMunchkin()
        {
            string strStdName = StringHelper.GetString("NewPlayer");
            string strNewName = strStdName;
            int count = 0;
            while (MunchkinNameExists(strNewName))
            {
                count++;
                strNewName = strStdName + "" + count;
            }
            Munchkin.Add(new Munchkin(strNewName, 1, 0, MunchkinMaxCount()+1, Model.Munchkin.GenderTyp.s));
        }
        bool MunchkinNameExists(string strName)
        {
            foreach (Munchkin item in Munchkin)
            {
                if (strName == item.Name)
                {
                    return true;
                }
            }
            return false;
        }

        internal void RemoveMunchkin(Munchkin rem)
        {
            Munchkin.Remove(rem);
        }

        /// <summary>  
        ///  returns the highes used order-value at all Munchkins
        /// </summary>  
        int MunchkinMaxCount()
        {
            int nTemp = int.MinValue;
            foreach (Munchkin item in Munchkin)
            {
                if (nTemp < item.Order)
                {
                    nTemp = item.Order;
                }
            }
            if (nTemp <= Munchkin.Count - 1)
            {
                nTemp = Munchkin.Count - 1;
            }
            return nTemp;
        }

        // Order things ===============================================================
        /// <summary>  
        ///  Re-orders the Munchkin-list by the given order, each element is touched multiple times
        /// </summary>  
        /// <note>  
        ///  The Order Property of the Elements is not changed, but get's refreshed, when ordering after the munchkin order proprty
        /// </note>  
        internal IEnumerable<Munchkin> SetNewOrder(MunchkinOrder eOrder)
        {
            IEnumerable<Munchkin> sortQuery = null; 
            switch (eOrder)
            {
                case MunchkinOrder.ABC:
                    sortQuery =
                        from x in Munchkin
                        orderby x.Name ascending
                        select x;
                    break;
                case MunchkinOrder.LvL:
                    sortQuery =
                        from x in Munchkin
                        orderby x.Level ascending
                        select x;
                    break;
                case MunchkinOrder.Pwr:
                    sortQuery =
                        from x in Munchkin
                        orderby x.Power descending
                        select x;
                    break;
                case MunchkinOrder.Reihe:
                    SetCurrentOrderasMainOrder();
                    sortQuery = Munchkin;
                    break;
                case MunchkinOrder.ABC_Reverse:
                    sortQuery =
                        from x in Munchkin
                        orderby x.Name descending
                        select x;
                    break;
                case MunchkinOrder.LvL_Reverse:
                    sortQuery =
                        from x in Munchkin
                        orderby x.Level descending
                        select x;
                    break;
                case MunchkinOrder.Pwr_Reverse:
                    sortQuery =
                        from x in Munchkin
                        orderby x.Power ascending
                        select x;
                    break;
                default:
                    break;
            }
            this.CurrentOrder = eOrder;
            return sortQuery;
        }
        /// <summary>  
        ///  refreshes, the ordering of each munchkin element at the list to avoid spacing between the order properties
        /// </summary>  
        void SetCurrentOrderasMainOrder()
        {
            int i = 0;
            foreach (Munchkin item in Munchkin)
            {
                item.Order = i;
                i++;
            }
        }

        // Battel Methods =============================================================
        /// <summary>  
        ///  listen for changes of the 
        /// </summary>  
        void Munchkin_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (Munchkin item in Munchkin)
            {
                item.PropertyChanged -= MunchkinChanged;
                item.PropertyChanged += MunchkinChanged;
            }
            AnyPropertyChanged();
        }

        void MunchkinChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Model.Munchkin.Level) || e.PropertyName == nameof(Model.Munchkin.Gear) || e.PropertyName == nameof(Model.Munchkin.IsBattle))
            {
                CalculateBattle();
            }
            if (e.PropertyName == nameof(Model.Munchkin.Level) || e.PropertyName == nameof(Model.Munchkin.Power))
            {
                NotifyOrderChanged();
            }
            AnyPropertyChanged();
        }

        void CalculateBattle()
        {
            MunchkinPower = 0;
            foreach (Munchkin item in Munchkin)
            {
                if (item.IsBattle)
                {
                    if (true == UseLevel)
                    {
                        MunchkinPower = MunchkinPower + item.Level;
                    }
                    if (true == UseGear)
                    {
                        MunchkinPower = MunchkinPower + item.Gear;
                    }
                }
            }
            MunchkinPower = MunchkinPower + MunchkinPowerMod;
        }
        //=============================================================================

        // Reset things =======================================================

        internal void ResetGame()
        {
            foreach (Munchkin item in Munchkin)
            {
                item.ClearForNewGame();
            }
            try
            {
                this.CurrentMunchkin = this.Munchkin.First();
            }
            catch (Exception)
            {
                this.CurrentMunchkin = null;
            }
            this.CurrentOrder = MunchkinOrder.Reihe;
        }
        internal void ResetAppModel()
        {
            ResetBattle();
            ResetMunchkins();
            ResetStatistics();
        }
        internal void ResetMunchkins()
        {
            this.Munchkin.Clear();
            this.CurrentMunchkin = null;
            this.CurrentOrder = MunchkinOrder.Reihe;
        }
        internal void ResetBattle()
        {
            this.UseGear = true;
            this.UseLevel = true;
            this.MonsterPower = 0;
            this.MunchkinPower = 0;
            this.MunchkinPowerMod = 0;
            foreach (Munchkin item in Munchkin)
            {
                item.IsBattle = false;
            }
        }
        internal void ResetStatistics()
        {
            Statistics.Clear();
        }
        // Handle the current munchkin ========================================
        internal void NextMunchkin()
        {
            MunchkinOrder eTemp = CurrentOrder;
            CurrentOrder = MunchkinOrder.Reihe;

            CurrentMunchkin = Munchkin[(Munchkin.IndexOf(CurrentMunchkin)+1)% Munchkin.Count];

            CurrentOrder = eTemp;           
        }

        internal void PrevMunchkin()
        {
            MunchkinOrder eOldOrder = CurrentOrder;
            CurrentOrder = MunchkinOrder.Reihe;
            int nIndextemp = (Munchkin.IndexOf(CurrentMunchkin) - 1);
            while (nIndextemp<0)
            {
                nIndextemp = nIndextemp + Munchkin.Count;
            }

            CurrentMunchkin = Munchkin[nIndextemp];

            CurrentOrder = eOldOrder;
            }

    }
}
