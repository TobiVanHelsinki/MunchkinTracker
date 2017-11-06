using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using TLIB;
using TLIB.IO;
using TLIB.Model;

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
    public class Game : INotifyPropertyChanged, TLIB.Model.IMainType
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler OrderChanged;
        public event PropertyChangedEventHandler CurrentMunchkChanged;
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
        private void Game_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Contains("MainObject"))
            {
                this.SaveRequest -= async (x, y) => await TLIB.IO.SharedIO<Game>.SaveAtOriginPlace(this, eUD: TLIB.IO.UserDecision.ThrowError);
                this.SaveRequest += async (x, y) => await TLIB.IO.SharedIO<Game>.SaveAtOriginPlace(this, eUD: TLIB.IO.UserDecision.ThrowError);
            }
        }

        //=============================================================================

        private void NotifyPropertyChanged([CallerMemberName] System.String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void NotifyOrderChanged([CallerMemberName] System.String propertyName = "")
        {
            OrderChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        private void NotifyCurrentMunchkChanged([CallerMemberName] System.String propertyName = "")
        {
            CurrentMunchkChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //=============================================================================
        public ObservableCollection<Munchkin> lstMunchkin;
        public ObservableCollection<RandomResult> lstStatistics;

        private Munchkin _oCurrentMunchkin;
        public Munchkin oCurrentMunchkin
        {
            get { return _oCurrentMunchkin; }
            set
            {
                if (value == null)
                {
                    if (lstMunchkin.Contains(_oCurrentMunchkin))
                    {
                        // element still exists, all is ok, no need to do something
                    }
                    else
                    {//element is gone, need to find a new
                        try
                        {
                            _oCurrentMunchkin = lstMunchkin.First();
                        }
                        catch (Exception)
                        {//there are no elements left, we have to assign null
                            _oCurrentMunchkin = null;
                        }
                        NotifyCurrentMunchkChanged();
                        NotifyPropertyChanged();
                    }
                }
                else
                {
                    if (value != _oCurrentMunchkin)
                    {
                        _oCurrentMunchkin = value;
                        NotifyCurrentMunchkChanged();
                        NotifyPropertyChanged();
                    }
                }
            }
        }

        private MunchkinOrder _eCurrentOrder;
        public MunchkinOrder eCurrentOrder
        {
            get
            {
                return _eCurrentOrder;
            }
            set
            {
                //Order(_eCurrentOrder);
                if (_eCurrentOrder != value)
                {
                    _eCurrentOrder = value;
                    NotifyOrderChanged();
                }
            }
        }
        
        //=============================================================================
        private int _nMonsterPower;
        public int nMonsterPower
        {
            get
            {
                return _nMonsterPower;
            }
            set
            {
                _nMonsterPower = value;
                NotifyPropertyChanged();
            }
        }
        private int _nMunchkinPower;
        public int nMunchkinPower
        {
            get
            {
                return _nMunchkinPower;
            }
            set
            {
                _nMunchkinPower = value;
                NotifyPropertyChanged();
            }
        }
        private int _nMunchkinPowerMod;
        public int nMunchkinPowerMod
        {
            get
            {
                return _nMunchkinPowerMod;
            }
            set
            {
                _nMunchkinPowerMod = value;
                NotifyPropertyChanged();
                CalculateBattle();
            }
        }
        private bool? _bUseLevel = true;
        public bool? bUseLevel
        {
            get
            {
                return _bUseLevel;
            }
            set
            {
                _bUseLevel = value;
                NotifyPropertyChanged();
                CalculateBattle();
            }
        }
        private bool? _bUseGear = true;
        public bool? bUseGear
        {
            get
            {
                return _bUseGear;
            }
            set
            {
                _bUseGear = value;
                CalculateBattle();
                NotifyPropertyChanged();
            }
        }
        //=============================================================================
        private uint _nRandomMax = Constants.STD_RANDOM_MAX;
        public int nRandomMax
        {
            get
            {
                return (int)_nRandomMax;
            }
            set
            {
                if (value < 2)
                {
                    _nRandomMax = 2;
                }
                else
                {
                    _nRandomMax = (uint)value;
                }
                NotifyPropertyChanged();
            }
        }
        //SOUNDBOARD ##########################################################
        [Newtonsoft.Json.JsonIgnore]
        public ObservableCollection<Sound> SoundList = new ObservableCollection<Sound>();

        //FILE HANDLING #######################################################
        public string APP_VERSION_NUMBER => Constants.APP_VERSION;

        public string FILE_VERSION_NUMBER => Constants.FILE_VERSION;

        [Newtonsoft.Json.JsonIgnore]
        public FileInfoClass FileInfo { get; set; } = new FileInfoClass();

        [Newtonsoft.Json.JsonIgnore]
        Func<string, string, string, IMainType> IMainType.Converter => null;


        public string MakeName()
        {
            return Constants.SAVE_GAME;
        }

        //=============================================================================
        public Game()
        {
            SoundList.Add(new Sound() { Name = CrossPlatformHelper.GetString("Sound_Badum"), SoundName = Sound.eSoundName.badumtshh, Description = "" });
            SoundList.Add(new Sound() { Name = CrossPlatformHelper.GetString("Sound_Wilhelm"), SoundName = Sound.eSoundName.WilhelmScream, Description = "" });
            //SoundList.Add(new Sound() { Name = CrossPlatformHelper.GetString("Sound_Chord"), SoundName = Sound.eSoundName.Chord, Description = "" });

            Tim = new System.Threading.Timer((x)=> { SaveRequest?.Invoke(x, new EventArgs()); HasChanges = false; },this, System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            PropertyChanged += Game_PropertyChanged;
            lstMunchkin = new ObservableCollection<Munchkin>();
            lstStatistics = new ObservableCollection<RandomResult>();
            lstMunchkin.CollectionChanged += LstMunchkin_CollectionChanged;
        }

        //=============================================================================
        public void AddMunchkin()
        {
            string strStdName = "Neuer Spieler";
            string strNewName = strStdName;
            int count = 0;
            while (MunchkinNameExists(strNewName))
            {
                count++;
                strNewName = strStdName + "" + count;
            }
            lstMunchkin.Add(new Munchkin(strNewName, 1, 0, lstMunchkinMaxCount()+1, Munchkin.eGenderTyp.s));
        }
        private bool MunchkinNameExists(string strName)
        {
            foreach (Munchkin item in lstMunchkin)
            {
                if (strName == item.strName)
                {
                    return true;
                }
            }
            return false;
        }

        public void RemoveMunchkin(Munchkin rem)
        {
            lstMunchkin.Remove(rem);
        }

        /// <summary>  
        ///  returns the highes used order-value at all Munchkins
        /// </summary>  
        private int lstMunchkinMaxCount()
        {
            int nTemp = int.MinValue;
            foreach (Munchkin item in lstMunchkin)
            {
                if (nTemp < item.nOrder)
                {
                    nTemp = item.nOrder;
                }
            }
            if (nTemp <= lstMunchkin.Count - 1)
            {
                nTemp = lstMunchkin.Count - 1;
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
        public IEnumerable<Munchkin> SetNewOrder(MunchkinOrder eOrder)
        {
            IEnumerable<Munchkin> sortQuery = null; 
            switch (eOrder)
            {
                case MunchkinOrder.ABC:
                    sortQuery =
                        from x in lstMunchkin
                        orderby x.strName ascending
                        select x;
                    break;
                case MunchkinOrder.LvL:
                    sortQuery =
                        from x in lstMunchkin
                        orderby x.nLevel ascending
                        select x;
                    break;
                case MunchkinOrder.Pwr:
                    sortQuery =
                        from x in lstMunchkin
                        orderby x.nPower descending
                        select x;
                    break;
                case MunchkinOrder.Reihe:
                    SetCurrentOrderasMainOrder();
                    sortQuery = lstMunchkin;
                    break;
                case MunchkinOrder.ABC_Reverse:
                    sortQuery =
                        from x in lstMunchkin
                        orderby x.strName descending
                        select x;
                    break;
                case MunchkinOrder.LvL_Reverse:
                    sortQuery =
                        from x in lstMunchkin
                        orderby x.nLevel descending
                        select x;
                    break;
                case MunchkinOrder.Pwr_Reverse:
                    sortQuery =
                        from x in lstMunchkin
                        orderby x.nPower ascending
                        select x;
                    break;
                default:
                    break;
            }
            this.eCurrentOrder = eOrder;

            return sortQuery;

            //switch (eOrder)
            //{
            //    case MunchkinOrder.ABC:
            //        for (int i = 0; i < lst.Count - 1; i++)
            //        {
            //            for (int j = 0; j < lst.Count - 1; j++)
            //            {
            //                if (0 < lst[j].strName.CompareTo(lst[j + 1].strName))
            //                {
            //                    lst.Move(j, j + 1);
            //                }
            //            }
            //        }
            //        break;
            //    case MunchkinOrder.LvL:
            //        for (int i = 0; i < lst.Count - 1; i++)
            //        {
            //            for (int j = 0; j < lst.Count - 1; j++)
            //            {
            //                if (lst[j].nLevel > lst[j + 1].nLevel)
            //                {
            //                    lst.Move(j, j + 1);
            //                }
            //            }
            //        }
            //        break;
            //    case MunchkinOrder.Pwr:
            //        for (int i = 0; i < lst.Count - 1; i++)
            //        {
            //            for (int j = 0; j < lst.Count - 1; j++)
            //            {
            //                if (lst[j].nPower < lst[j + 1].nPower)
            //                {
            //                    lst.Move(j, j + 1);
            //                }
            //            }
            //        }
            //        break;
            //    case MunchkinOrder.Reihe:
            //        for (int i = 0; i < lst.Count - 1; i++)
            //        {
            //            for (int j = 0; j < lst.Count - 1; j++)
            //            {
            //                if (lst[j].nOrder > lst[j + 1].nOrder)
            //                {
            //                    lst.Move(j, j + 1);
            //                }
            //            }
            //        }
            //        SetCurrentOrderasMainOrder(lst);
            //        break;
            //    default:
            //        break;
            //}
        }
        /// <summary>  
        ///  refreshes, the ordering of each munchkin element at the list to avoid spacing between the order properties
        /// </summary>  
        private void SetCurrentOrderasMainOrder()
        {
            int i = 0;
            foreach (Munchkin item in lstMunchkin)
            {
                item.nOrder = i;
                i++;
            }
        }

        // Battel Methods =============================================================
        /// <summary>  
        ///  listen for changes of the 
        /// </summary>  
        private void LstMunchkin_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (Munchkin item in lstMunchkin)
            {
                item.PropertyChanged -= MunchkinChanged;
                item.PropertyChanged += MunchkinChanged;
            }
        }

        private void MunchkinChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "nLevel" || e.PropertyName == "nGear" || e.PropertyName == "bIsBattle")
            {
                CalculateBattle();
            }
            if (e.PropertyName == "nLevel" || e.PropertyName == "nPower")
            {
                NotifyOrderChanged();
            }

        }

        private void CalculateBattle()
        {
            nMunchkinPower = 0;
            foreach (Munchkin item in lstMunchkin)
            {
                if (item.bIsBattle)
                {
                    if (true == bUseLevel)
                    {
                        nMunchkinPower = nMunchkinPower + item.nLevel;
                    }
                    if (true == bUseGear)
                    {
                        nMunchkinPower = nMunchkinPower + item.nGear;
                    }
                }
            }
            nMunchkinPower = nMunchkinPower + nMunchkinPowerMod;
        }
        //=============================================================================

        // Reset things =======================================================

        public void ResetGame()
        {
            foreach (Munchkin item in lstMunchkin)
            {
                item.ClearForNewGame();
            }
            try
            {
                this.oCurrentMunchkin = this.lstMunchkin.First();
            }
            catch (Exception)
            {
                this.oCurrentMunchkin = null;
            }
            this.eCurrentOrder = MunchkinOrder.Reihe;
        }
        public void ResetAppModel()
        {
            ResetBattle();
            ResetMunchkins();
            ResetStatistics();
        }
        public void ResetMunchkins()
        {
            this.lstMunchkin.Clear();
            this.oCurrentMunchkin = null;
            this.eCurrentOrder = MunchkinOrder.Reihe;
        }
        public void ResetBattle()
        {
            this.bUseGear = true;
            this.bUseLevel = true;
            this.nMonsterPower = 0;
            this.nMunchkinPower = 0;
            this.nMunchkinPowerMod = 0;
            foreach (Munchkin item in lstMunchkin)
            {
                item.bIsBattle = false;
            }
        }
        public void ResetStatistics()
        {
            lstStatistics.Clear();
        }
        // Handle the current munchkin ========================================
        public void NextMunchkin()
        {
            MunchkinOrder eTemp = eCurrentOrder;
            eCurrentOrder = MunchkinOrder.Reihe;

            oCurrentMunchkin = lstMunchkin[(lstMunchkin.IndexOf(oCurrentMunchkin)+1)% lstMunchkin.Count];

            eCurrentOrder = eTemp;           
        }

        public void PrevMunchkin()
        {
            MunchkinOrder eOldOrder = eCurrentOrder;
            eCurrentOrder = MunchkinOrder.Reihe;
            int nIndextemp = (lstMunchkin.IndexOf(oCurrentMunchkin) - 1);
            while (nIndextemp<0)
            {
                nIndextemp = nIndextemp + lstMunchkin.Count;
            }

            oCurrentMunchkin = lstMunchkin[nIndextemp];

            eCurrentOrder = eOldOrder;
            }

    }
}
