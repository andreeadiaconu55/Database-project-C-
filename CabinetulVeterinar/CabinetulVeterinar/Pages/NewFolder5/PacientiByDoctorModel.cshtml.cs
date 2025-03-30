using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CabinetulVeterinar.Pages.NewFolder5
{
    public class PacientiByDoctorModel : PageModel
    {
        private readonly ILogger<PacientiByDoctorModel> _logger;

        [BindProperty]
        public string DoctorName { get; set; }

        public List<PacientDetail> PacientiByDoctor { get; set; } = new List<PacientDetail>();

        public PacientiByDoctorModel(ILogger<PacientiByDoctorModel> logger)
        {
            _logger = logger;
        }

        public void OnPost()
        {
            if (string.IsNullOrEmpty(DoctorName))
            {
                ModelState.AddModelError(string.Empty, "Numele doctorului este obligatoriu.");
                return;
            }

            string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                    SELECT DISTINCT 
                        P.PacientID, 
                        P.Nume AS NumePacient, 
                        P.Rasa AS RasaPacient, 
                        PR.DataProgramare, 
                        PR.Motiv
                    FROM Pacienti P
                    INNER JOIN Programare PR ON P.PacientID = PR.PacientID
                    WHERE PR.ProgramareID IN (
                        SELECT C.ProgramareID
                        FROM Consultatie C
                        WHERE C.DoctorID = (
                            SELECT DoctorID
                            FROM Doctori
                            WHERE Nume LIKE '%' + @DoctorName + '%'
                        )
                    )
                    ORDER BY P.Nume;";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@DoctorName", DoctorName);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                PacientiByDoctor.Add(new PacientDetail
                                {
                                    PacientID = reader.GetInt32(0),
                                    NumePacient = reader.GetString(1),
                                    RasaPacient = reader.GetString(2),
                                    DataProgramare = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                    Motiv = reader.GetString(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea pacienților: {ex.Message}");
            }
        }
    }

    public class PacientDetail
    {
        public int PacientID { get; set; }
        public string NumePacient { get; set; }
        public string RasaPacient { get; set; }
        public DateTime? DataProgramare { get; set; }
        public string Motiv { get; set; }
    }
}
