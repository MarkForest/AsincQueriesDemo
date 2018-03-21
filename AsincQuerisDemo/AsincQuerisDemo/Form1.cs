using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AsincQuerisDemo
{
    public partial class Form1 : Form
    {
        string connectionString = @"Data Source=CR5-00\SQLEXPRESS;Initial Catalog=Library;Integrated Security=true;";
        SqlConnection sqlConnection = null;
        SqlCommand sqlCommand = null;
        DataTable dataTable = null;
        SqlDataReader sqlDataReader = null;
        public Form1()
        {
            InitializeComponent();
            sqlConnection = new SqlConnection(connectionString);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //Модификация строки для подкоючения во вторичніх потоках
            string AsyncEnable = "Asynchronous Processing=true";
            if (!connectionString.Contains(AsyncEnable))
            {
                connectionString = String.Format("{0}{1}", connectionString, AsyncEnable);
            }

            sqlCommand = sqlConnection.CreateCommand();
           
            sqlCommand.CommandText = "waitfor delay '00:00:20';select * from authors;";
            sqlCommand.CommandType = CommandType.Text;
            try
            {
                sqlConnection.Open();
                AsyncCallback callback = new AsyncCallback(GetDataCallBack);
                sqlCommand.BeginExecuteReader(callback, sqlCommand);
                MessageBox.Show("Added thread is working...");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void GetDataCallBack(IAsyncResult asyncResult)
        {
            try
            {
                SqlCommand sqlCommand = asyncResult.AsyncState as SqlCommand;
                sqlDataReader = sqlCommand.EndExecuteReader(asyncResult);

                dataTable = new DataTable();
                int line = 0;
                do
                {
                    while (sqlDataReader.Read())
                    {
                        if (line == 0)
                        {
                            for (int i = 0; i < sqlDataReader.FieldCount; i++)

                            {
                                dataTable.Columns.Add(sqlDataReader.GetName(i));
                            }
                            line++;
                        }
                        DataRow dataRow = dataTable.NewRow();
                        for (int i = 0; i < sqlDataReader.FieldCount; i++)
                        {
                            dataRow[i] = sqlDataReader[i];
                        }
                        dataTable.Rows.Add(dataRow);

                    }
                } while (sqlDataReader.NextResult());
                DGVAction();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {

                try
                {
                    if (!sqlDataReader.IsClosed)
                    {
                        sqlDataReader.Close();
                        sqlConnection.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void DGVAction()
        {
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action(DGVAction));
                return;
            }
            dataGridView1.DataSource = dataTable;
        }
    }
}
