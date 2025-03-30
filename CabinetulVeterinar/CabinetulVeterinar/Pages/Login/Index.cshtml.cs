using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Collections.Generic;

namespace CabinetulVeterinar.Pages.Login
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public LoginInfo LoginData { get; set; } = new LoginInfo();

        public string ErrorMessage { get; set; }
        public string SuccessMessage { get; set; }
        public List<Appointment> DoctorAppointments { get; set; } = new List<Appointment>();

        public void OnGet(string doctorId = null)
        {
            if (!string.IsNullOrEmpty(doctorId))
            {
                // Setăm DoctorID în sesiune
                HttpContext.Session.SetString("DoctorID", doctorId);
            }

            string savedDoctorId = HttpContext.Session.GetString("DoctorID");
            string savedDoctorName = HttpContext.Session.GetString("DoctorName");

            if (!string.IsNullOrEmpty(savedDoctorId))
            {
                SuccessMessage = $"Bună ziua, {savedDoctorName}!";
                LoadAppointments(savedDoctorId); // Încărcăm programările doctorului
            }
            else
            {
                // Dacă sesiunea a expirat sau nu există DoctorID, redirecționăm la login
                RedirectToPage("/Login/Index");
            }
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(LoginData.Email) || string.IsNullOrEmpty(LoginData.Password))
            {
                ErrorMessage = "Toate câmpurile trebuie completate.";
                return Page();
            }

            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=\"Cabinet veterinar\";Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = "SELECT DoctorID, Nume, Prenume FROM Doctori WHERE Email = @Email AND Password = @Password";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Email", LoginData.Email);
                        command.Parameters.AddWithValue("@Password", LoginData.Password);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string doctorId = reader.GetInt32(0).ToString(); // DoctorID
                                string doctorName = reader.GetString(1) + " " + reader.GetString(2); // Numele intreg

                                // Setarea DoctorID și DoctorName în sesiune
                                HttpContext.Session.SetString("DoctorID", doctorId);
                                HttpContext.Session.SetString("DoctorName", doctorName); // Salvăm DoctorName în sesiune
                                Console.WriteLine($"DoctorID setat în sesiune: {doctorId}");
                                SuccessMessage = $"Bun venit, {doctorName}!";

                                LoadAppointments(doctorId); // Se incarcă programările doctorului

                                return Page();
                            }
                            else
                            {
                                ErrorMessage = "Email sau parolă incorecte.";
                                return Page();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Eroare la conectare: " + ex.Message;
                return Page();
            }
        }

        private void LoadAppointments(string doctorId)
        {
            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=\"Cabinet veterinar\";Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                SELECT 
                    PR.DataProgramare,
                    PR.OraProgramare,
                    PR.Motiv,
                    P.Nume AS PatientName
                FROM 
                    Programare PR
                INNER JOIN 
                    Pacienti P ON PR.PacientID = P.PacientID
                WHERE 
                    PR.StapanID = @DoctorID
                ORDER BY 
                    PR.DataProgramare, PR.OraProgramare";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorID", doctorId);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            DoctorAppointments.Clear();
                            while (reader.Read())
                            {
                                DoctorAppointments.Add(new Appointment
                                {
                                    AppointmentDateTime = reader.GetDateTime(0).Add(reader.GetTimeSpan(1)), // Data și ora programării
                                    ConsultationType = reader.GetString(2),  // Motivul programării
                                    PatientName = reader.GetString(3)        // Numele pacientului
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "Eroare la încărcarea programărilor: " + ex.Message;
            }
        }

        public IActionResult OnPostLogout()
        {
            // Curățăm sesiunea
            HttpContext.Session.Clear();

            // Redirecționăm la formularul de autentificare
            return RedirectToPage("/Login/Index");
        }
    }

    public class LoginInfo
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class Appointment
    {
        public DateTime AppointmentDateTime { get; set; }
        public string PatientName { get; set; }
        public string ConsultationType { get; set; }
    }
}
