using System;
using CloudWallet_GTK.ViewModels;
using Gtk;
using System.Linq;

public partial class MainWindow : Gtk.Window
{

    private WalletVM _myVM;
    private ListStore listStore;

    public WalletVM DataContext { get; private set; }

    private void SetMyVM(WalletVM vm)
    {
        _myVM = vm;
        _myVM.ResetChanges();
        this.DataContext = _myVM;

        if (_myVM.SelectedItem == null)
        {
            txtTitle.Text = "";
            txtContent.Buffer.Text = "";
        }
        else
        {
            txtTitle.Text = _myVM.SelectedItem.Title;
            txtContent.Buffer.Text = _myVM.SelectedItem.Content;
        }

        updateTreeview();
    }

    private void updateTreeview()
    {
        listStore.Clear();
        var position = 0;
        foreach (var item in _myVM.FilteredItems)
        {
            listStore.InsertWithValues(position++, item.Title);
        }
    }

    public MainWindow() : base(Gtk.WindowType.Toplevel)
    {
        Build();

        treeview1.HeadersVisible = false;

        var cellView = new CellRendererText();
        var column = new TreeViewColumn("Title", cellView);
        column.AddAttribute(cellView, "text", 0);
        treeview1.AppendColumn(column);

        listStore = new ListStore(typeof(string));
        treeview1.Model = listStore;

        SetMyVM(new WalletVM());
    }

    protected void OnDeleteEvent(object sender, DeleteEventArgs a)
    {
        if (!_myVM.CloseCommand())
        {
            return;
        }

        Application.Quit();
        a.RetVal = true;
    }

    protected void btnRemove_Clicked(object sender, EventArgs e)
    {
        _myVM.DeleteCommand();
        updateTreeview();

        txtTitle.Text = "";
        txtContent.Buffer.Text = "";
    }

    protected void btnAdd_Clicked(object sender, EventArgs e)
    {
        _myVM.AddCommand();
        updateTreeview();
    }

    protected void onFileOpen(object sender, EventArgs e)
    {
        var password = passwordBox.Text;
        if (_myVM.CloseCommand())
        {
            WalletVM wallet = WalletVM.OpenCommand(password);
            if (wallet != null)
                SetMyVM(wallet);
        }
    }

    protected void txtSearchChanged(object sender, EventArgs e)
    {
        _myVM.SearchText = txtSearch.Text;
        updateTreeview();
    }

    protected void treeview_selectCursuorParent(object o, SelectCursorParentArgs args)
    {

    }

    protected void tree_selectionrecieved(object o, SelectionReceivedArgs args)
    {

    }

    protected void tree_CursorChanged(object sender, EventArgs e)
    {
        var obj = sender as TreeView;
        TreeIter myselection;
        obj.Selection.GetSelected(out myselection);
        var item = Convert.ToString(listStore.GetValue(myselection, 0));
        if (!string.IsNullOrEmpty( item))        {
            _myVM.SelectedItem = _myVM.Items.FirstOrDefault(x => x.Title == item);
            if (_myVM.SelectedItem != null)
            {
                txtTitle.Text = _myVM.SelectedItem.Title;
                txtContent.Buffer.Text = _myVM.SelectedItem.Content;
            }
        }
    }

    protected void onFileNew(object sender, EventArgs e)
    {
        if (_myVM.CloseCommand())
            SetMyVM(new WalletVM());
    }

    protected void OnFileSave(object sender, EventArgs e)
    {
        _myVM.Password = passwordBox.Text;
        _myVM.SaveCommand();
    }

    protected void onFileSaveAs(object sender, EventArgs e)
    {
        _myVM.Password = passwordBox.Text;
        _myVM.SaveAsCommand();
    }

    protected void onFileExit(object sender, EventArgs e)
    {
        _myVM.Password = passwordBox.Text;
        _myVM.CloseCommand();
        Gtk.Application.Quit();
    }

    protected void OnContentInsert(object o, InsertAtCursorArgs args)
    {

    }

    protected void OnContentDelete(object o, DeleteFromCursorArgs args)
    {

    }

    protected void txtTitle_keyrelease(object o, KeyReleaseEventArgs args)
    {
        if (_myVM == null)
            return;

        if (_myVM.SelectedItem == null)
            return;

        _myVM.SelectedItem.Title = (o as Entry).Text;
        updateTreeview();
    }

    protected void content_keyup(object o, KeyReleaseEventArgs args)
    {
        if (_myVM == null)
            return;

        if (_myVM.SelectedItem == null)
            return;

        _myVM.SelectedItem.Content = (o as TextView).Buffer.Text;
    }
}
