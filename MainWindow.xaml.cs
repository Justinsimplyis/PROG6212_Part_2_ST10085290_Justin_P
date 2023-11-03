using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;//
using Newtonsoft.Json;
using TimeManagementPart2Library;//Custom Class Library added


namespace PROG6212_Part_2_ST10085290_Justin_P
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Connection to database/server
        private const string connectionString = "Data Source=lab000000\\SQLEXPRESS;Initial Catalog=DbTimeManagmentApp;Integrated Security=True";
        private List<Modules> modules;

        private CalcStudyHours calcStudyHours;
        public MainWindow()
        {
            InitializeComponent();
            InitializeCalcStudyHours();
            DisplayModules();

        }
        private void InitializeCalcStudyHours()
        {
            // Initialize calcStudyHours object
            calcStudyHours = new CalcStudyHours
            {
                Weeks = 16,
                StartDate = new DateTime(2023, 8, 1), 
                Modules = new List<Modules>(), 
                StudyHours = new Dictionary<string, List<int>>(), 
                ModuleStudyInfoList = new List<ModuleStudyInfo>() 
            };
        }

        private void DisplayModules()
        {
            modules = new List<Modules>(); // Initialize the modules list
            // Fetch and display modules from the database
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT ModuleCourse, ModuleCode, ModuleName, ModuleCredits, ClassHoursPerWeek FROM Modules";
                    SqlCommand command = new SqlCommand(query, connection);

                    List<Modules> modules = new List<Modules>();

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            modules.Add(new Modules
                            {
                                ModuleCourse = reader["ModuleCourse"].ToString(),
                                ModuleCode = reader["ModuleCode"].ToString(),
                                ModuleName = reader["ModuleName"].ToString(),
                                ModuleCredits = Convert.ToInt32(reader["ModuleCredits"]),
                                ClassHoursPerWeek = Convert.ToInt32(reader["ClassHoursPerWeek"])
                            });
                        }
                    }

                    lstModules.ItemsSource = modules;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void AddModuleButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Insert a new module into the database
                    string query = "INSERT INTO Modules (ModuleCourse, ModuleCode, ModuleName, ModuleCredits, ClassHoursPerWeek) " +
                                   "VALUES (@ModuleCourse, @ModuleCode, @ModuleName, @ModuleCredits, @ClassHoursPerWeek)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("ModuleCourse", txtModuleCourse.Text);
                    command.Parameters.AddWithValue("@ModuleCode", txtModuleCode.Text);
                    command.Parameters.AddWithValue("@ModuleName", txtModuleName.Text);
                    command.Parameters.AddWithValue("@ModuleCredits", int.Parse(txtModuleCredits.Text));
                    command.Parameters.AddWithValue("@ClassHoursPerWeek", int.Parse(txtClassHoursPerWeek.Text));

                    command.ExecuteNonQuery();
                }

                // Clear the input fields after data has been enetered
                txtModuleCourse.Text = "";
                txtModuleCode.Text = "";
                txtModuleName.Text = "";
                txtModuleCredits.Text = "";
                txtClassHoursPerWeek.Text = "";

                // Refresh the displayed modules
                DisplayModules();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void RecordStudyHoursButton_Click(object sender, RoutedEventArgs e)
        {
            int hours;
            if (lstModules.SelectedItem != null && int.TryParse(txtStudyHoursPerWeek.Text, out hours))
            {
                var selectedModule = calcStudyHours.Modules[lstModules.SelectedIndex];
                var moduleName = selectedModule.ModuleName;
                var currentWeek = (DateTime.Now - calcStudyHours.StartDate).Days / 7 + 1;
                var remainingHours = selectedModule.ModuleCredits * 10 / calcStudyHours.Weeks - selectedModule.ClassHoursPerWeek;

                if (!calcStudyHours.StudyHours.ContainsKey(moduleName))
                {
                    calcStudyHours.StudyHours[moduleName] = new List<int>();
                }

                if (calcStudyHours.StudyHours[moduleName].Count(week => week == currentWeek) > 0)
                {
                    remainingHours -= calcStudyHours.StudyHours[moduleName].Where(week => week == currentWeek).Sum();
                }
                if (remainingHours >= hours)
                {
                    calcStudyHours.StudyHours[moduleName].Add(hours);

                    // Update ModuleStudyInfoList
                    calcStudyHours.ModuleStudyInfoList.Add(new ModuleStudyInfo
                    {
                        ModuleName = moduleName,
                        RequiredStudyHPW = selectedModule.ClassHoursPerWeek
                    });

                    // Binds the updated data to the ListView
                    lstModuleStudyHours.ItemsSource = null;
                    lstModuleStudyHours.ItemsSource = calcStudyHours.ModuleStudyInfoList;

                    txtblockRemainingHours.Text = $"Your remaining self-study hours for {moduleName} this week: {remainingHours - hours}";

                    // the creates the database operations
                    using (SqlConnection connection = new SqlConnection("your_connection_string_here"))
                    {
                        connection.Open();

                        // Inserts the  study hours data into the database
                        string insertQuery = "INSERT INTO CalcStudyHourss (ModuleName, StartDate, RemainingStudyTime, RequiredStudyHoursPerWeek) " +
                                             "VALUES (@ModuleName, @StartDate, @RemainingStudyTime, @RequiredStudyHoursPerWeek)";
                        using (SqlCommand command = new SqlCommand(insertQuery, connection))
                        {
                            command.Parameters.AddWithValue("@ModuleName", moduleName);
                            command.Parameters.AddWithValue("@StartDate", calcStudyHours.StartDate);
                            command.Parameters.AddWithValue("@RemainingStudyTime", remainingHours - hours);
                            command.Parameters.AddWithValue("@RequiredStudyHoursPerWeek", selectedModule.ClassHoursPerWeek);

                            command.ExecuteNonQuery();
                        }
                    }
                }
                else
                {
                   txtblockRemainingHours.Text = $"You do not have enough remaining self-study hours for {moduleName} this week.";

                }
            }
            else
            {
                MessageBox.Show("Please select a MODULE and enter VALID study hours.");
            }
        }
    }
}
    