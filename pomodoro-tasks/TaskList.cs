﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SQLite;
using System.Diagnostics;

namespace pomodoro_tasks
{
    public partial class TaskList : Form
    {
        public TaskList()
        {
            InitializeComponent();
        }

        //private void TaskList_Activate(object sender, EventArgs e)
        //{
        //    RefreshTaskList();
        //}

        public void RefreshTaskList()
        {
            List<Activity> tasks = new List<Activity>();
            SQLiteCommand cmd = new SQLiteCommand("select * from activities where is_active_task = 1 and is_completed = 0", Program.connection);
            Program.connection.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                var vals = reader.GetValues();
                tasks.Add(new Activity {
                    Id = int.Parse(vals["id"]),
                    Description = vals["activity_desc"].ToString(),
                    EstimatedPomodoros = int.Parse(vals["estimated"]),
                    ActualPomodoros = int.Parse(vals["actual"]),
                    Interruptions = int.Parse(vals["interruptions"]),
                    IsActiveTask = int.Parse(vals["is_active_task"]) == 1 ? true : false,
                    IsCompleted = int.Parse(vals["is_completed"]) == 1 ? true : false
                });
            }
            addTasksToListView(tasks);
            Program.connection.Close();
        }

        private void addTasksToListView(List<Activity> tasks)
        {
            dg_tasks.Rows.Clear();
            tasks.ForEach(task => {
                dg_tasks.Rows.Add(new object[] { task.Id,
                    task.Description,
                    task.EstimatedPomodoros.ToString(),
                    task.ActualPomodoros.ToString(),
                    task.Interruptions.ToString(),
                    task.IsCompleted.ToString()}
                    );
            });

        }

        private void btn_activity_list_Click(object sender, EventArgs e)
        {
            this.Hide();
            Program.activityDialog.ReloadListBoxes();
            Program.activityDialog.Show();
        }

        private void btn_exit_Click(object sender, EventArgs e)
        {
            Program.activityDialog.Close();
        }

        private void dg_tasks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            var row = ((DataGridView)sender).Rows[e.RowIndex];
            var thisActivity = new Activity {
                Id = int.Parse(row.Cells["Id"].Value.ToString()),
                Description = row.Cells["Description"].Value.ToString(),
                EstimatedPomodoros = int.Parse(row.Cells["Estimated"].Value.ToString()),
                ActualPomodoros = int.Parse(row.Cells["Actual"].Value.ToString()),
                Interruptions = int.Parse(row.Cells["Interruptions"].Value.ToString()),
                IsActiveTask = true,
                IsCompleted = false
            };

            switch (e.ColumnIndex) {
                case 3:
                    thisActivity.ActualPomodoros += 1;
                    break;
                case 4:
                    thisActivity.Interruptions += 1;
                    break;
                case 5:
                    thisActivity.IsCompleted = !thisActivity.IsCompleted;
                    break;
            }

            SQLiteCommand updateCmd = new SQLiteCommand(thisActivity.MakeUpdateStatement(), Program.connection);
            Program.connection.Open();
            updateCmd.ExecuteNonQuery();
            Program.connection.Close();

            RefreshTaskList();

        }
    }
}
