using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PippyInvoker
{
    public partial class UserComboForm : Form
    {

        public static List<string> CheckedList = new List<string>();

        public UserComboForm()
        {
            InitializeComponent();
        }

        private void MoveUp(object sender, EventArgs e)
        {
            MoveItemBox(-1);
        }

        private void MoveDown(object sender, EventArgs e)
        {
            MoveItemBox(1);
        }

        private void MoveItemBox(int UpOrDown)
        {
            if (checkedListBox1.SelectedItem == null || checkedListBox1.SelectedIndex < 0)
            {
                return;
            }

            int newIndex = checkedListBox1.SelectedIndex + UpOrDown;

            if (newIndex < 0 || newIndex >= checkedListBox1.Items.Count)
            {
                return;
            }

            object selectedSpell = checkedListBox1.SelectedItem;
            bool spellActive = checkedListBox1.GetItemChecked(checkedListBox1.SelectedIndex);

            checkedListBox1.Items.Remove(selectedSpell);
            checkedListBox1.Items.Insert(newIndex, selectedSpell);
            checkedListBox1.SetSelected(newIndex, true);

            checkedListBox1.SetItemChecked(newIndex, spellActive);
        }

        private void ApplyClick(object sender, EventArgs e)
        {

            CheckedList.Clear();

            foreach (object checkedSpell in checkedListBox1.CheckedItems)
            {
                CheckedList.Add(checkedSpell.ToString());
            }
        }
    }
}
