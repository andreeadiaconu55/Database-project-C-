using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CabinetulVeterinar.Pages.NewFolder5
{
    public class DoctoriCautareModel : PageModel
    {
        private readonly ILogger<DoctoriCautareModel> _logger;

        [BindProperty]
        public string Specializare { get; set; }
        public List<Doctor> Doctori { get; set; } = new List<Doctor>();
        public string SelectedSpecializare { get; set; }

        public DoctoriCautareModel(ILogger<DoctoriCautareModel> logger)
        {
            _logger = logger;
        }

        public void OnPost()
        {
            SelectedSpecializare = Specializare;

            string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                        SELECT 
                            D.DoctorID, 
                            D.Nume, 
                            D.Prenume, 
                            D.Email, 
                            D.Specializare
                        FROM Doctori D
                        INNER JOIN Programare P ON D.DoctorID = P.StapanID
                        WHERE D.Specializare = @Specializare
                        GROUP BY D.DoctorID, D.Nume, D.Prenume, D.Email, D.Specializare
                        ORDER BY D.Nume;";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Specializare", Specializare);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Doctori.Add(new Doctor
                                {
                                    DoctorID = reader.GetInt32(0),
                                    Nume = reader.GetString(1),
                                    Prenume = reader.GetString(2),
                                    Email = reader.GetString(3),
                                    Specializare = reader.GetString(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea doctorilor: {ex.Message}");
            }
        }
    }

    public class Doctor
    {
        public int DoctorID { get; set; }
        public string Nume { get; set; }
        public string Prenume { get; set; }
        public string Email { get; set; }
        public string Specializare { get; set; }
    }
}
