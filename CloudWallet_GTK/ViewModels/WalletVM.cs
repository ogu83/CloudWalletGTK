using CloudWallet_GTK.Crypto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using System.Windows.Forms;
using Gtk;

namespace CloudWallet_GTK.ViewModels
{
    [Serializable]
    [DataContract]
    public class WalletVM : VMBase
    {
        public WalletVM()
        {
            Items = new ObservableCollection<ItemVM>();
        }

        #region Properties
        private ObservableCollection<ItemVM> _items;
        [DataMember]
        [XmlArray]
        public ObservableCollection<ItemVM> Items
        {
            get { return _items; }
            set
            {
                _items = value;
                NotifyPropertyChanged("Items");
                NotifyPropertyChanged("FilteredItems");
            }
        }
        public IEnumerable<ItemVM> FilteredItems
        {
            get
            {
                IEnumerable<ItemVM> filteredItems;
                if (!string.IsNullOrEmpty(_searchText))
                    filteredItems = from x in _items
                                    where x.Title.ToLower().Contains(_searchText.ToLower())
                                    orderby x.Title
                                    select x;
                else
                    filteredItems = from x in _items
                                    orderby x.Title
                                    select x;
                return filteredItems;
            }
        }

        [NonSerialized]
        private string _searchText;
        [IgnoreDataMember, XmlIgnore]
        public string SearchText
        {
            get { return _searchText; }
            set
            {
                _searchText = value;
                NotifyPropertyChanged("SearchText");
                NotifyPropertyChanged("FilteredItems");
            }
        }

        private ItemVM _selectedItem;
        [IgnoreDataMember, XmlIgnore]
        public ItemVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;
                NotifyPropertyChanged("SelectedItem");
                NotifyPropertyChanged("IsAnyItemSelected");
            }
        }

        public bool IsAnyItemSelected
        {
            get { return _selectedItem != null; }
        }

        private const string _defaultFileName = "NewWallet";
        private string _fileName = _defaultFileName;
        [DataMember]
        [XmlAttribute]
        public string FileName
        {
            get { return _fileName; }
            set
            {
                _fileName = value;
                NotifyPropertyChanged("FileName");
                NotifyPropertyChanged("Title");
            }
        }

        public string Title
        {
            get
            {
                return "Cloud Wallet v:"
                    + Assembly.GetExecutingAssembly().GetName().Version.ToString() +
                    " - " + _fileName;
            }
        }

        private string _fullPath;

        [DataMember]
        [XmlAttribute]
        public string Password { get; set; }

        #endregion

        #region Methods
        private bool passwordAndSave()
        {
            if (!string.IsNullOrWhiteSpace(Password))
                return saveToFile(_fullPath, Password);
            else
                return false;
        }

        private bool saveToFile(string fileName, string password)
        {
            try
            {
                File.WriteAllBytes(fileName, AES.Encrypt(SerializeToXML(), password));
                ResetChanges();
                return true;
            }
            catch (Exception ex)
            {
                Gtk.Application.Invoke(delegate
                {
                    MessageDialog d = new MessageDialog(MainClass.win, DialogFlags.Modal,
                        MessageType.Error, ButtonsType.Close,
                        "Saving File Failed");
                    d.Title = "Save";
                    d.ShowAll();
                    var response = (ResponseType)d.Run();
                    d.Destroy();
                });
                return false;
            }
        }

        private static WalletVM FromFile(string fileName, string password)
        {
            try
            {
                return DeSerializeFromXML<WalletVM>(AES.Decrypt(File.ReadAllBytes(fileName), password));
            }
            catch (Exception ex)
            {
                Gtk.Application.Invoke(delegate {
                    using (var md = new MessageDialog(MainClass.win, DialogFlags.Modal, MessageType.Error, ButtonsType.Close, "Corrupt file or wrong password"))
                    {
                        md.Title = "File Not Loaded";
                        md.Run();
                        md.Destroy();
                    }
                });
                return null;
            }
        }

        private void removeItem(ItemVM item)
        {
            item.Changed -= item_Changed;
            this.Items.Remove(item);
            NotifyPropertyChanged("FilteredItems");
            IsChanged = true;
        }
        private void addItem(ItemVM item)
        {
            item.Changed += item_Changed;
            this.Items.Add(item);
            NotifyPropertyChanged("FilteredItems");
            SelectedItem = item;
            IsChanged = true;
        }
        private void item_Changed(object sender, EventArgs e)
        {
            this.IsChanged = this.IsChanged || (sender as ItemVM).IsChanged;
        }

        internal void ResetChanges()
        {
            foreach (ItemVM item in Items)
                item.IsChanged = false;

            this.IsChanged = false;
        }

        private void BindItemEvents()
        {
            foreach (ItemVM item in _items)
                item.Changed += item_Changed;
        }
        #endregion

        #region Commands
        internal bool CloseCommand()
        {
            if (IsChanged)
            {
                MessageDialog d = new MessageDialog(MainClass.win, DialogFlags.Modal, 
                                        MessageType.Question, ButtonsType.YesNo,
                                        "Do you want to save changes before exit");
                d.Title = "Close File";
                d.ShowAll();
                var response = (ResponseType)d.Run();
                d.Destroy();

                switch (response)
                {
                    case ResponseType.Yes:
                        return SaveCommand();
                    case ResponseType.No:
                        return true;                    
                    default:
                        return false;
                }
            }
            else
                return true;
        }

        internal bool SaveCommand()
        {
            if (_fileName == _defaultFileName)
                return SaveAsCommand();
            else
                return passwordAndSave();
        }

        internal bool SaveAsCommand()
        {
            Gtk.FileChooserDialog filechooser =
                new Gtk.FileChooserDialog("Choose the file to save",
                MainClass.win,
                FileChooserAction.Save,
                "Cancel", ResponseType.Cancel,
                "Save", ResponseType.Accept);

            if (filechooser.Run() == (int)ResponseType.Accept)
            {
                _fullPath = filechooser.Filename;
                FileName = filechooser.Filename;
                filechooser.Destroy();
                return passwordAndSave();
            }
            else
            {
                filechooser.Destroy();
                return false;
            }
        }

        internal static WalletVM OpenCommand(string Password)
        {
            Gtk.FileChooserDialog filechooser =
                new Gtk.FileChooserDialog("Choose the file to open",
                MainClass.win,
                FileChooserAction.Open,
                "Cancel", ResponseType.Cancel,
                "Open", ResponseType.Accept);               

            if (filechooser.Run() == (int)ResponseType.Accept)
            {
                //System.IO.FileStream file = System.IO.File.OpenRead(filechooser.Filename);
                WalletVM wallet = FromFile(filechooser.Filename, Password);
                if (wallet != null)
                {
                    wallet._fullPath = filechooser.Filename;
                    wallet.FileName = filechooser.Filename;
                    wallet.Password = Password;
                    wallet.BindItemEvents();
                    wallet.ResetChanges();
                }
                filechooser.Destroy();
                return wallet;
                //file.Close();                
            }

            filechooser.Destroy();
            return null;
        }

        internal void AddCommand()
        {
            addItem(new ItemVM(Items.Count, "New Item", ""));
        }

        internal void DeleteCommand()
        {
            if (SelectedItem != null)
            {
                MessageDialog d = new MessageDialog(MainClass.win, DialogFlags.Modal,
                        MessageType.Question, ButtonsType.YesNo,
                        "Do you want to delete selected item?");
                d.Title = "Delete Item";
                d.ShowAll();
                var response = (ResponseType)d.Run();
                if (response == ResponseType.Yes)
                {
                    removeItem(SelectedItem);
                }
                d.Destroy();
            }                    
        }
        #endregion
    }
}
