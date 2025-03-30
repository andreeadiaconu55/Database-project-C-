using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CabinetulVeterinar.Pages.NewFolder5
{
    public class ProgramariBySpecializationModel : PageModel
    {
        private readonly ILogger<ProgramariBySpecializationModel> _logger;

        public List<ProgramareDetail> ProgramariDetails { get; set; } = new List<ProgramareDetail>();
        [BindProperty]
        public string Specializare { get; set; }

        public ProgramariBySpecializationModel(ILogger<ProgramariBySpecializationModel> logger)
        {
            _logger = logger;
        }

        public void OnPost()
        {
            string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                    SELECT 
                        PR.ProgramareID,
                        PR.DataProgramare,
                        PR.OraProgramare,
                        P.Nume AS NumePacient,
                        P.Rasa AS RasaPacient,
                        D.Nume AS NumeDoctor,
                        D.Specializare,
                        PR.Motiv
                    FROM Programare PR
                    INNER JOIN Pacienti P ON PR.PacientID = P.PacientID
                    INNER JOIN Consultatie C ON PR.ProgramareID = C.ProgramareID
                    INNER JOIN Doctori D ON C.DoctorID = D.DoctorID
                    WHERE D.Specializare = @Specializare
                    ORDER BY PR.DataProgramare;";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Specializare", Specializare);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ProgramariDetails.Add(new ProgramareDetail
                                {
                                    ProgramareID = reader.GetInt32(0),
                                    DataProgramare = reader.GetDateTime(1).ToString("yyyy-MM-dd"),
                                    OraProgramare = reader.GetTimeSpan(2).ToString(@"hh\:mm"),
                                    NumePacient = reader.GetString(3),
                                    RasaPacient = reader.GetString(4),
                                    NumeDoctor = reader.GetString(5),
                                    Specializare = reader.GetString(6),
                                    Motiv = reader.GetString(7)
                                });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea programărilor: {ex.Message}");
            }
        }
    }

    public class ProgramareDetail
    {
        public int ProgramareID { get; set; }
        public string DataProgramare { get; set; }
        public string OraProgramare { get; set; }
        public string NumePacient { get; set; }
        public string RasaPacient { get; set; }
        public string NumeDoctor { get; set; }
        public string Specializare { get; set; }
        public string Motiv { get; set; }
    }
}
