﻿using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using EmployeesManagementSystem.Data;
using EmployeesManagementSystem.Models;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;

namespace EmployeesManagementSystem
{
    public partial class Statistic : Form
    {
        // Context variables
        private UserContext userContext = new UserContext();
        private ShiftContext shiftContext = new ShiftContext();
        private DepartmentContext departmentContext = new DepartmentContext();

        private User[] users;
        private List<Shift> shifts;
        private Department[] departments;

        // Counters keep track for each of the x-axis position the number of employees for each kind 
        private int[] counterAttended = new int[1000];
        private int[] counterAbsent = new int[1000];
        private int[] counterScheduled = new int[1000];

        private double[] banet = new double[1000];

        // Keeps  track of the current loggedUser
        private User loggedUser;

        private float money = 0;
        private DateTime today = DateTime.Today;


        public Statistic(User user)
        {
            InitializeComponent();
            this.users = this.userContext.GetAllUsers();
            this.departments = departmentContext.GetAllDepartments();
            this.loggedUser = user;
            int[] cevaAttended = { 0 };
            int[] cevaAbsent = { 0 };
            int[] cevaScheduled = { 0 };
            double[] banet = { 0 };

            displayMonths();
        }
        private void Statistic_Load(object sender, EventArgs e)
        { 

        }

        // Add months from the current day
        private void displayMonths()
        {
            DateTime now = DateTime.Now;
            for (int i = 0; i < 12; i++)
            {
                this.cbMonth.Items.Add(now.ToString("MMMM"));
                now = now.AddMonths(1);
            }
        }


        private void picBack_Click(object sender, EventArgs e)
        {
            this.Hide();
            // Show Dashboard
            Dashboard dashboard = new Dashboard( this.loggedUser);
            dashboard.Closed += (s, args) => this.Close();
            dashboard.Show();
        }

        private void exit_Click(object sender, EventArgs e)
        {
            this.Close();
            if (Application.MessageLoop)
            {
                Application.Exit();
            }
        }

        private void AttendancePerEmployee()
        {
            try
            {
                this.shifts = this.shiftContext.GetShiftsByUserId(Convert.ToInt32(cbEmployee.Text)); // try catch
            }
            catch (Exception)
            {
                throw new Exception("Can't take employee's shifts");
            }

            User user = userContext.GetUserByID(Convert.ToInt32(cbEmployee.Text));

            // Switches the min and max to the earliest and latest a shift can be
            this.chart1.ChartAreas[0].AxisY.Maximum = 23;
            this.chart1.ChartAreas[0].AxisY.Minimum= 9;
            this.chart1.ChartAreas[0].AxisY.Interval = 1;

            // Attendance per employee
            foreach (Shift shift in shifts)
            {
                // Counts only the shift in the month equal to the one selected
                if (this.cbMonth.Text == shift.ShiftDate.ToString("MMMM"))
                {
                    // Marks attended
                    // Convert the months into days for the x axis and the minutes into hours for the y axis
                    if (shift.Attended == true)
                        this.chart1.Series["Present"].Points.Add(new DataPoint() { AxisLabel = Convert.ToString(shift.ShiftDate.Day) + "/" + Convert.ToString(shift.ShiftDate.Month), XValue = 31 * shift.ShiftDate.Month + shift.ShiftDate.Day, YValues = new double[] { shift.StartTime.Hour + shift.StartTime.Minute / 60, shift.EndTime.Hour + shift.EndTime.Minute / 60 } });
                    // Marks absent
                    else if (shift.Attended == false && shift.ShiftDate < this.today)
                        this.chart1.Series["Absent"].Points.Add(new DataPoint() { AxisLabel = Convert.ToString(shift.ShiftDate.Day) + "/" + Convert.ToString(shift.ShiftDate.Month), XValue = 31 * shift.ShiftDate.Month + shift.ShiftDate.Day, YValues = new double[] { shift.StartTime.Hour + shift.StartTime.Minute / 60, shift.EndTime.Hour + shift.EndTime.Minute / 60 } });
                    // Marks future scheduled shifts
                    else if (shift.Attended == false && shift.ShiftDate > this.today)
                    {
                        this.chart1.Series["Scheduled"].Points.Add(new DataPoint() { AxisLabel = Convert.ToString(shift.ShiftDate.Day) + "/" + Convert.ToString(shift.ShiftDate.Month), XValue = 31 * shift.ShiftDate.Month + shift.ShiftDate.Day, YValues = new double[] { shift.StartTime.Hour + shift.StartTime.Minute / 60, shift.EndTime.Hour + shift.EndTime.Minute / 60 } });
                    }
                }
            }
        }

        private void AttendancePerDepartment()
        {
            try
            {
                // Gets only the shifts for the selected department
                this.shifts = this.shiftContext.GetShiftsByDepId(Convert.ToInt32(this.cbEmployee.Text)); // try catch
            }
            catch (Exception)
            {
                throw new Exception("Can't take department's shifts");
            }

            // Switches the min and max to 0 and auto
            this.chart1.ChartAreas[0].AxisY.Minimum = 0;
            this.chart1.ChartAreas[0].AxisY.Maximum = Double.NaN;
            this.chart1.ChartAreas[0].AxisY.Interval = 1;

            Department department = this.departmentContext.GetDepartmentById(Convert.ToInt32(this.cbEmployee.Text));
            
            foreach (Shift shift in shifts)
            {
                if (this.cbMonth.Text == shift.ShiftDate.ToString("MMMM"))
                {
                    // Converts months into days and counts how many people work on each day
                    int ics = 31 * shift.ShiftDate.Month + shift.ShiftDate.Day;
                    // Marks attended
                    if (shift.Attended == true)
                    {
                        this.counterAttended[ics]++;
                    }
                    // Marks absent
                    else if (shift.Attended == false && shift.ShiftDate <= this.today)
                    {
                        this.counterAbsent[ics]++;
                    }
                    // Mark scheduled
                    else if (shift.Attended == false && shift.ShiftDate > this.today)
                    {
                        this.counterScheduled[ics]++;
                    }
                }
            }

            //goes through each possible ics which is 31 days * 12 months
            for (int i = 0; i <= 31 * 12; i++)
            {
                if (this.counterAttended[i] != 0)
                    this.chart1.Series["Present"].Points.Add(new DataPoint() { AxisLabel = i / 31 + "/" + i % 31, XValue = i, YValues = new double[] { 0, counterAttended[i] } });
                if (this.counterAbsent[i] != 0)
                    this.chart1.Series["Absent"].Points.Add(new DataPoint() { AxisLabel = i / 31 + "/" + i % 31, XValue = i, YValues = new double[] { counterAttended[i], counterAttended[i] + counterAbsent[i] } });
                if (this.counterScheduled[i] != 0)
                    this.chart1.Series["Scheduled"].Points.Add(new DataPoint() { AxisLabel = i / 31 + "/" + i % 31, XValue = i, YValues = new double[] { counterAttended[i] + counterAbsent[i], counterAttended[i] + counterAbsent[i] + counterScheduled[i] } });
                this.counterAttended[i] = this.counterAbsent[i] = this.counterScheduled[i] = 0;
            }

        }

        private void WagePerEmployee()
        {
            try
            {
                this.shifts = this.shiftContext.GetShiftsByUserId(Convert.ToInt32(this.cbEmployee.Text)); 
            }
            catch (Exception)
            {
                throw new Exception("Can't take employee's shifts");
            }

            User user = userContext.GetUserByID(Convert.ToInt32(this.cbEmployee.Text));

            this.chart1.ChartAreas[0].AxisY.Minimum = 0;
            this.chart1.ChartAreas[0].AxisY.Maximum = Double.NaN;

            foreach (Shift shift in shifts)
            {
                if (this.cbMonth.Text == shift.ShiftDate.ToString("MMMM")) 
                {
                    this.money = 0;
                    if (shift.Attended)
                    {
                        {
                            //money = hrsWorked * wage
                            this.money = (shift.EndTime - shift.StartTime).Hours * user.HourlyRate;
                            this.banet[GetWeekOfYear(shift.ShiftDate)] += this.money;
                           
                        }
                    }
                    
                }
            }

            this.chart1.ChartAreas[0].AxisY.Interval = 10;
            for (int i = 0; i < this.banet.Length; i++)
            {
                if (this.banet[i] != 0)
                {
                    this.chart1.Series["Money"].Points.Add(new DataPoint() { AxisLabel = "Week " + Convert.ToString(i), XValue = i, YValues = new double[] { banet[i] } });
                }

                //chart1.Series["Money"].Label = Convert.ToString(banet[i]);
                this.banet[i] = 0;
            }
        }

        private void cbStatistic_SelectedIndexChanged(object sender, EventArgs e)
        {
            foreach (var series in chart1.Series)
            {
                series.Points.Clear();
            }
            if (this.cbStatistic.Text.Contains("employee"))
            {
                this.cbEmployee.Items.Clear();
                this.lbEmployee.Text = "Select an employee";
                            
                foreach (User item in users)
                {
                    this.cbEmployee.Items.Add(item.ID);
                }

            }
            else if (this.cbStatistic.Text.Contains("department"))
            {
                this.cbEmployee.Items.Clear();
                this.lbEmployee.Text = "Select a department";
                foreach (Department item in departments)
                {
                    this.cbEmployee.Items.Add(item.ID);
                }
            }
            
            if (this.cbStatistic.Text == "Attendance per employee")
            {
                //labels for y axis hours
                CustomLabel label1 = new CustomLabel
                {
                    FromPosition = 8.5,
                    ToPosition = 9.5,
                    Text = "09:00"
                };

                CustomLabel label2 = new CustomLabel
                {
                    FromPosition = 9.5,
                    ToPosition = 10.5,
                    Text = "10:00"
                };

                CustomLabel label3 = new CustomLabel
                {
                    FromPosition = 10.5,
                    ToPosition = 11.5,
                    Text = "11:00"
                };

                CustomLabel label4 = new CustomLabel
                {
                    FromPosition = 11.5,
                    ToPosition = 12.5,
                    Text = "12:00"
                };

                CustomLabel label5 = new CustomLabel
                {
                    FromPosition = 12.5,
                    ToPosition = 13.5,
                    Text = "13:00"
                };

                CustomLabel label6 = new CustomLabel
                {
                    FromPosition = 13.5,
                    ToPosition = 14.5,
                    Text = "14:00"
                };

                CustomLabel label7 = new CustomLabel
                {
                    FromPosition = 14.5,
                    ToPosition = 15.5,
                    Text = "15:00"
                };

                CustomLabel label8 = new CustomLabel
                {
                    FromPosition = 15.5,
                    ToPosition = 16.5,
                    Text = "16:00"
                };

                CustomLabel label9 = new CustomLabel
                {
                    FromPosition = 16.5,
                    ToPosition = 17.5,
                    Text = "17:00"
                };

                CustomLabel label10 = new CustomLabel
                {
                    FromPosition = 17.5,
                    ToPosition = 18.5,
                    Text = "18:00"
                };

                CustomLabel label11 = new CustomLabel
                {
                    FromPosition = 18.5,
                    ToPosition = 19.5,
                    Text = "19:00"
                };

                CustomLabel label12 = new CustomLabel
                {
                    FromPosition = 19.5,
                    ToPosition = 20.5,
                    Text = "20:00"
                };

                CustomLabel label13 = new CustomLabel
                {
                    FromPosition = 20.5,
                    ToPosition = 21.5,
                    Text = "21:00"
                };

                CustomLabel label14 = new CustomLabel
                {
                    FromPosition = 21.5,
                    ToPosition = 22.5,
                    Text = "22:00"
                };

                CustomLabel label15 = new CustomLabel
                {
                    FromPosition = 22.5,
                    ToPosition = 23.5,
                    Text = "23:00"
                };

                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label1);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label2);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label3);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label4);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label5);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label6);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label7);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label8);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label9);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label10);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label11);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label12);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label13);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label14);
                chart1.ChartAreas[0].AxisY.CustomLabels.Add(label15);

            }
            else
            {
                chart1.ChartAreas[0].AxisY.CustomLabels.Clear();
            }


            if (cbMonth.Text.Length > 0 && cbEmployee.Text.Length > 0)
            {
                   showAttendance();
            }
            
        }

        // Clears the chart
        private void clearChart()
        {
            foreach (var series in this.chart1.Series)
            {
                series.Points.Clear();
            }
        }

        // Helper method
        private void showAttendance()
        {
            if (cbStatistic.Text == "Attendance per employee")
                AttendancePerEmployee();

            if (cbStatistic.Text == "Attendance per department")
                AttendancePerDepartment();

            if (cbStatistic.Text == "Wage per employee")
                WagePerEmployee();
        }

        private void cbMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearChart();

            if (cbStatistic.Text.Length > 0 && cbEmployee.Text.Length > 0)
            {
                showAttendance();
            }
        }

        
        private void cbEmployee_SelectedIndexChanged(object sender, EventArgs e)
        {
            clearChart();

            if (cbMonth.Text.Length > 0 && cbStatistic.Text.Length > 0)
            {
                showAttendance();
            }
        }

        public static int GetWeekOfYear(DateTime date)
        {
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(date);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                date = date.AddDays(3);
            }

            // Return the week of the day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        // Hovering
        private void exit_MouseEnter(object sender, EventArgs e)
        {
            Color color = Color.DarkGray;
            this.exit.BackColor = color;
        }

        private void exit_MouseLeave(object sender, EventArgs e)
        {
            Color color = Color.White;
            this.exit.BackColor = color;
        }
    }
}
