﻿using System;
using System.Data;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using EmployeesManagementSystem.Models;

namespace EmployeesManagementSystem
{
    public partial class Dashboard : Form
    {

        private User[] users;
        private DataTable table;
        private DbContext databaseContext = new DbContext();

        public Dashboard()
        {
            InitializeComponent();
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
            try
            {
                this.users = this.databaseContext.GetAllUsers();
                this.table = this.databaseContext.GetUsers();
                
                showInformation(this.users);
            }
            catch (Exception)
            {
                throw new Exception("Can't display info correctly!");
            }
        }

        public void UpdateDashboard()
        {
            this.dataGridView.Rows.Clear();
            this.table = this.databaseContext.GetUsers();

            User[] users = databaseContext.GetAllUsers();
            showInformation(users);
        }

        private void searchField_KeyPress(object sender, KeyPressEventArgs e)
        {
        
            // Pressed enter
            if (e.KeyChar == (char)13)
            {

                DataView dv = this.table.DefaultView;
         
                // Filter the rows
                dv.RowFilter = string.Format("FullName Like '%{0}%'", RemoveWhiteSpaces(this.searchField.Text));
                if (dv.ToTable().Rows.Count > 0)
                {
                    User[] users = databaseContext.GetAllFilteredUsers(dv.ToTable());
                    showInformation(users);
                }
                else
                {
                    User[] users = databaseContext.GetAllUsers();
                    showInformation(users);
                }        

            }
                       
        }

        // Remove the WhiteSpaces
        private string RemoveWhiteSpaces(string text)
        {
            return Regex.Replace(text, @"\s+|\t|\n|\r", String.Empty);
        }

        private void showInformation(User[] users)
        {
            // Clean the dataGrid
            this.dataGridView.Rows.Clear();

            foreach (User user in users)
            {
                this.dataGridView.Rows.Add(user.GetInfo());
            }
            this.searchField.Text = String.Empty;
        }

        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int Details = 4;
            int Delete = 5;
            if (dataGridView.CurrentCell.ColumnIndex.Equals(Details))
            {
                int index = dataGridView.CurrentCell.RowIndex;
                int id = Convert.ToInt32(dataGridView.Rows[index].Cells[0].Value);

                this.Hide();
                Details details = new Details(id);
                details.Show();
               
            }

            if (dataGridView.CurrentCell.ColumnIndex.Equals(Delete))
            {
               
                    
             // Ask if you want to delete and proccess
             int index = dataGridView.CurrentCell.RowIndex;
            
                // Find the role
                if (!Convert.ToString(dataGridView.Rows[index].Cells[3].Value).Contains("Administrator"))
                {
                    int id = Convert.ToInt32(dataGridView.Rows[index].Cells[0].Value);
                    
                    Delete delete = new Delete(id, this);
                    delete.Show();
                }
                else
                {
                    Warning warning = new Warning(this);
                    warning.Show();
                }
            }

        }


        private void exit_Click(object sender, EventArgs e)
        {
            // Closing the db connection 
            databaseContext.Dispose(true);
            this.Close();

            // exiting properly the application
            if (Application.MessageLoop)
            {
                Application.Exit();
            }

        }

        // Shifts
        private void btnShift_Click(object sender, EventArgs e)
        {
            databaseContext.Dispose(true);
            this.Hide();
            // Show Dashboard
            Shifts shifts = new Shifts();
            shifts.Closed += (s, args) => this.Close();
            shifts.Show();
        }

        // Create Accounts
        private void btnCreate_MouseClick(object sender, MouseEventArgs e)
        {
            CreateAccounts createAccounts = new CreateAccounts(this);
            createAccounts.Show();
        }
        private void lbCreate_Click(object sender, EventArgs e)
        {
            CreateAccounts createAccounts = new CreateAccounts(this);
            createAccounts.Show();
        }


        // Hovering onn the the images
        private void btnExit_MouseEnter(object sender, EventArgs e)
        {
            this.btnExit.BackColor = Color.LightGray;
        }
        private void btnExit_MouseLeave(object sender, EventArgs e)
        {
            this.btnExit.BackColor = Color.White;
        }
        private void btnEmployees_MouseEnter(object sender, EventArgs e)
        {
            Color color = Color.DarkGray;
            this.btnEmployees.BackColor = color;
        }
        private void btnEmployees_MouseLeave(object sender, EventArgs e)
        {
            Color color = Color.LightGray;
            this.btnEmployees.BackColor = color;
        }
        private void btnCreate_MouseEnter(object sender, EventArgs e)
        {
            Color color = Color.DarkGray;
            this.btnCreate.BackColor = color;
        }
        private void btnCreate_MouseLeave(object sender, EventArgs e)
        {
            Color color = Color.LightGray;
            this.btnCreate.BackColor = color;
        }      
       
        private void btnShift_MouseEnter(object sender, EventArgs e)
        {

            Color color = Color.DarkGray;
            this.btnShift.BackColor = color;
        }

        private void btnShift_MouseLeave(object sender, EventArgs e)
        {

            Color color = Color.LightGray;
            this.btnShift.BackColor = color;
        }

        private void lblLogOut_Click(object sender, EventArgs e)
        {
            databaseContext.Dispose(true);
            this.Hide();
            // Show Log In
            Login login = new Login();
            login.Closed += (s, args) => this.Close();
            login.Show();
        }

        private void btnSettings_Click(object sender, EventArgs e)
        {
            settingsPanel.Visible = !settingsPanel.Visible;
        }

        private void lbSettings_Click(object sender, EventArgs e)
        {

        }

        private void editAccount_Click(object sender, EventArgs e)
        {

        }

        private void btnCancellations_Click(object sender, EventArgs e)
        {
            databaseContext.Dispose(true);
            this.Hide();
            // Show Dashboard
            Cancellations cncl = new Cancellations();
            cncl.Closed += (s, args) => this.Close();
            cncl.Show();
        }

       
        private void btnStocks_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void btnDepartments_Click(object sender, EventArgs e)
        {
            databaseContext.Dispose(true);
            this.Hide();
            // Show Dashboard
            Departments dep = new Departments();
            dep.Closed += (s, args) => this.Close();
            dep.Show();

        }
        private void btnStocks_Click(object sender, EventArgs e)
        {
            databaseContext.Dispose(true);
            this.Hide();
            // Show Dashboard
            Stocks stock = new Stocks();
            stock.Closed += (s, args) => this.Close();
            stock.Show();
        }
    }
}
