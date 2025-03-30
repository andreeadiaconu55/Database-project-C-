using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CabinetulVeterinar.Pages.NewFolder4
{
    public class DetailsModel : PageModel
    {
        private readonly ILogger<DetailsModel> _logger;

        [BindProperty]
        public string TipVaccinCautat { get; set; } // Parametru variabil

        public List<VaccinDetail> VaccinDetails { get; set; } = new List<VaccinDetail>();
        public List<VaccinPacientDetail> VaccinPacientDetails { get; set; } = new List<VaccinPacientDetail>();

        public DetailsModel(ILogger<DetailsModel> logger)
        {
            _logger = logger;
        }

        public void OnPost()
        {
            string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

            if (string.IsNullOrEmpty(TipVaccinCautat))
            {
                ModelState.AddModelError(string.Empty, "Introduceți un tip de vaccin pentru a filtra.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Interogare filtrată cu subcerere
                    string sqlVaccinuri = @"
                        SELECT 
                            DoctorID,
                            (SELECT Nume 
                             FROM Doctori 
                             WHERE Doctori.DoctorID = Vaccin.DoctorID) AS NumeDoctor,
                            COUNT(VaccinID) AS NumarVaccinuri,
                            (SELECT COUNT(DISTINCT ProgramareID) 
                             FROM Vaccin V2 
                             WHERE V2.DoctorID = Vaccin.DoctorID) AS ProgramariUnice
                        FROM Vaccin
                        WHERE Tip = @TipVaccinCautat
                        GROUP BY DoctorID
                        ORDER BY NumarVaccinuri DESC;";

                    using (SqlCommand command = new SqlCommand(sqlVaccinuri, connection))
                    {
                        command.Parameters.AddWithValue("@TipVaccinCautat", TipVaccinCautat);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            VaccinDetails.Clear();
                            while (reader.Read())
                            {
                                VaccinDetails.Add(new VaccinDetail
                                {
                                    DoctorID = reader.GetInt32(0),
                                    NumeDoctor = reader.GetString(1),
                                    NumarVaccinuri = reader.GetInt32(2),
                                    ProgramariUnice = reader.GetInt32(3)
                                });
                            }
                        }
                    }

                    // Interogare cu JOIN pentru detalii vaccinuri și pacienți
                    string sqlVaccinPacienti = @"
                        SELECT 
                            P.PacientID,
                            P.Nume AS NumePacient,
                            V.Tip AS TipVaccin
                        FROM Vaccin V
                        INNER JOIN Programare PR ON V.ProgramareID = PR.ProgramareID
                        INNER JOIN Pacienti P ON PR.PacientID = P.PacientID
                        WHERE V.Tip = @TipVaccinCautat
                        ORDER BY P.Nume;";

                    using (SqlCommand command = new SqlCommand(sqlVaccinPacienti, connection))
                    {
                        command.Parameters.AddWithValue("@TipVaccinCautat", TipVaccinCautat);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            VaccinPacientDetails.Clear();
                            while (reader.Read())
                            {
                                VaccinPacientDetails.Add(new VaccinPacientDetail
                                {
                                    PacientID = reader.GetInt32(0),
                                    NumePacient = reader.GetString(1),
                                    TipVaccin = reader.GetString(2)
                                });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea detaliilor despre vaccinare: {ex.Message}");
            }
        }
    }

    public class VaccinDetail
    {
        public int DoctorID { get; set; }
        public string NumeDoctor { get; set; }
        public int NumarVaccinuri { get; set; }
        public int ProgramariUnice { get; set; }
    }

    public class VaccinPacientDetail
    {
        public int PacientID { get; set; }
        public string NumePacient { get; set; }
        public string TipVaccin { get; set; }
    }
}
