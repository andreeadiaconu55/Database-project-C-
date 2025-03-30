using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CabinetulVeterinar.Pages.Interogari
{
    public class InterogariModel : PageModel
    {
        private readonly ILogger<InterogariModel> _logger;

        [BindProperty]
        public string NumeDoctor { get; set; } // Parametru variabil

        public List<InterogareResult> InterogariResults { get; set; } = new List<InterogareResult>();
        public List<DoctorConsultatiiResult> DoctorConsultatiiResults { get; set; } = new List<DoctorConsultatiiResult>();

        public InterogariModel(ILogger<InterogariModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
        }

        public void OnPost()
        {
            if (string.IsNullOrEmpty(NumeDoctor))
            {
                _logger.LogWarning("Nume doctor gol. Interogările nu pot fi realizate.");
                return;
            }

            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Interogare consultații pe baza numelui doctorului
                    string sql1 = @"
                    SELECT 
                        C.ConsultatieID, 
                        D.Nume AS NumeDoctor, 
                        P.Nume AS NumePacient, 
                        C.Simptome
                    FROM Consultatie C
                    INNER JOIN Doctori D ON C.DoctorID = D.DoctorID
                    INNER JOIN Programare PR ON C.ProgramareID = PR.ProgramareID
                    INNER JOIN Pacienti P ON PR.PacientID = P.PacientID
                    WHERE D.Nume = @NumeDoctor;";

                    using (SqlCommand command = new SqlCommand(sql1, connection))
                    {
                        command.Parameters.AddWithValue("@NumeDoctor", NumeDoctor);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                InterogariResults.Add(new InterogareResult
                                {
                                    ConsultatieID = reader.GetInt32(0),
                                    NumeDoctor = reader.GetString(1),
                                    NumePacient = reader.GetString(2),
                                    Simptome = reader.GetString(3)
                                });
                            }
                        }
                    }

                    // Interogare numărul consultațiilor pe baza numelui doctorului
                    string sql2 = @"
                    SELECT 
                        D.Nume AS NumeDoctor, 
                        COUNT(C.ConsultatieID) AS NumarConsultatii
                    FROM Doctori D
                    LEFT JOIN Consultatie C ON D.DoctorID = C.DoctorID
                    WHERE D.Nume = @NumeDoctor
                    GROUP BY D.Nume;";

                    using (SqlCommand command = new SqlCommand(sql2, connection))
                    {
                        command.Parameters.AddWithValue("@NumeDoctor", NumeDoctor);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                DoctorConsultatiiResults.Add(new DoctorConsultatiiResult
                                {
                                    NumeDoctor = reader.GetString(0),
                                    NumarConsultatii = reader.GetInt32(1)
                                });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea interogărilor: {ex.Message}");
            }
        }
    }

    public class DoctorConsultatiiResult
    {
        public string NumeDoctor { get; set; }
        public int NumarConsultatii { get; set; }
    }

    public class InterogareResult
    {
        public int ConsultatieID { get; set; }
        public string NumeDoctor { get; set; }
        public string NumePacient { get; set; }
        public string Simptome { get; set; }
    }
}
