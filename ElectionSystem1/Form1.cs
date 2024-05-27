using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ElectionSystem1
{
    public partial class Form1 : Form
    {
        const string connectionString = "Data Source=MSI\\SQLEXPRESS;Initial Catalog=ElectionSystem01;Integrated Security=True;Encrypt=False";
        private bool isRegistered = false;
        private bool hasMarkedAttendance = false;

        public Form1()
        {
            InitializeComponent();
            BtnCastVote.Enabled = BtnMarkAttendance.Enabled = false;
            BtnRegister.Click += BtnRegister_Click;
            BtnMarkAttendance.Click += BtnMarkAttendance_Click;
            BtnCastVote.Click += BtnCastVote_Click;
        }

        private bool ValidateRegistration()
        {
            if (string.IsNullOrEmpty(txtName.Text) || string.IsNullOrEmpty(txtAddress.Text) || string.IsNullOrEmpty(txtVotersID.Text))
            {
                MessageBox.Show("Please fill in all required fields.");
                return false;
            }

            if (txtVotersID.Text.Length < 8)
            {
                MessageBox.Show("Voter ID must be at least 8 characters long.");
                return false;
            }

            return true;
        }

        private bool ValidateMarkAttendance()
        {
            if (!isRegistered)
            {
                MessageBox.Show("Please register first.");
                return false;
            }

            return true;
        }

        private bool ValidateCastVote()
        {
            if (!isRegistered || !hasMarkedAttendance)
            {
                MessageBox.Show("Please register and mark attendance first.");
                return false;
            }

            if (string.IsNullOrEmpty(txtPresident.Text) || string.IsNullOrEmpty(txtVicePresident.Text) ||
                string.IsNullOrEmpty(txtSecretary.Text) || string.IsNullOrEmpty(txtTreasurer.Text) ||
                string.IsNullOrEmpty(txtAuditor.Text) || string.IsNullOrEmpty(txtPIO.Text) ||
                string.IsNullOrEmpty(txtDisciplinaryOfficer.Text))
            {
                MessageBox.Show("Please fill in all required fields for casting vote.");
                return false;
            }

            return true;
        }

        private void ClearFormFields()
        {
            txtName.Clear();
            txtAddress.Clear();
            txtVotersID.Clear();
            txtPresident.Clear();
            txtVicePresident.Clear();
            txtSecretary.Clear();
            txtTreasurer.Clear();
            txtAuditor.Clear();
            txtPIO.Clear();
            txtDisciplinaryOfficer.Clear();
        }

        private void BtnRegister_Click(object sender, EventArgs e)
        {
            if (ValidateRegistration())
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string commandText = "INSERT INTO Voters (Name, Address, VotersID) VALUES (@Name, @Address, @VotersID)";
                        SqlCommand command = new SqlCommand(commandText, connection);
                        command.Parameters.AddWithValue("@Name", txtName.Text);
                        command.Parameters.AddWithValue("@Address", txtAddress.Text);
                        command.Parameters.AddWithValue("@VotersID", txtVotersID.Text);
                        command.ExecuteNonQuery();

                        isRegistered = true;
                        BtnMarkAttendance.Enabled = true;
                        MessageBox.Show("Registration successful!");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error during registration: " + ex.Message);
                }
            }
        }

        private void BtnMarkAttendance_Click(object sender, EventArgs e)
        {
            if (ValidateMarkAttendance())
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        if (IsRegistered(txtVotersID.Text))
                        {
                            if (IsAttendanceMarked(txtVotersID.Text))
                            {
                                MessageBox.Show("Attendance has already been marked for this voter.");
                            }
                            else
                            {
                                string commandText = "INSERT INTO Attendance (VotersID) VALUES (@VotersID)";
                                SqlCommand command = new SqlCommand(commandText, connection);
                                command.Parameters.AddWithValue("@VotersID", txtVotersID.Text);
                                command.ExecuteNonQuery();

                                hasMarkedAttendance = true;
                                BtnCastVote.Enabled = true;
                                MessageBox.Show("Attendance successfully marked!");
                            }
                        }
                        else
                        {
                            MessageBox.Show("Only registered voters can mark attendance.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error marking attendance: " + ex.Message);
                }
            }
        }

        private bool IsRegistered(string voterID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Voters WHERE VotersID = @VotersID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@VotersID", voterID);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private bool IsAttendanceMarked(string voterID)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = "SELECT COUNT(*) FROM Attendance WHERE VotersID = @VotersID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@VotersID", voterID);
                int count = (int)command.ExecuteScalar();
                return count > 0;
            }
        }

        private void BtnCastVote_Click(object sender, EventArgs e)
        {
            if (ValidateCastVote())
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string commandText = "INSERT INTO Votes (VotersID, President, VicePresident, Secretary, Treasurer, Auditor, PIO, DisciplinaryOfficer) VALUES (@VotersID, @President, @VicePresident, @Secretary, @Treasurer, @Auditor, @PIO, @DisciplinaryOfficer)";
                        SqlCommand command = new SqlCommand(commandText, connection);
                        command.Parameters.AddWithValue("@VotersID", txtVotersID.Text);
                        command.Parameters.AddWithValue("@President", txtPresident.Text);
                        command.Parameters.AddWithValue("@VicePresident", txtVicePresident.Text);
                        command.Parameters.AddWithValue("@Secretary", txtSecretary.Text);
                        command.Parameters.AddWithValue("@Treasurer", txtTreasurer.Text);
                        command.Parameters.AddWithValue("@Auditor", txtAuditor.Text);
                        command.Parameters.AddWithValue("@PIO", txtPIO.Text);
                        command.Parameters.AddWithValue("@DisciplinaryOfficer", txtDisciplinaryOfficer.Text);
                        command.ExecuteNonQuery();

                        SqlCommand updateVoteCountsCommand = new SqlCommand("UpdateVoteCounts", connection)
                        {
                            CommandType = CommandType.StoredProcedure
                        };
                        updateVoteCountsCommand.Parameters.AddWithValue("@VotersID", txtVotersID.Text);
                        updateVoteCountsCommand.Parameters.AddWithValue("@President", txtPresident.Text);
                        updateVoteCountsCommand.Parameters.AddWithValue("@VicePresident", txtVicePresident.Text);
                        updateVoteCountsCommand.Parameters.AddWithValue("@Secretary", txtSecretary.Text);
                        updateVoteCountsCommand.Parameters.AddWithValue("@Treasurer", txtTreasurer.Text);
                        updateVoteCountsCommand.Parameters.AddWithValue("@Auditor", txtAuditor.Text);
                        updateVoteCountsCommand.Parameters.AddWithValue("@PIO", txtPIO.Text);
                        updateVoteCountsCommand.Parameters.AddWithValue("@DisciplinaryOfficer", txtDisciplinaryOfficer.Text);
                        updateVoteCountsCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show("Vote cast successfully!");
                    ClearFormFields();
                    ResetState();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error casting vote: " + ex.Message);
                }
            }
        }

        private void ResetState()
        {
            BtnCastVote.Enabled = false;
            BtnMarkAttendance.Enabled = false;
            isRegistered = false;
            hasMarkedAttendance = false;
        }
    }
}
