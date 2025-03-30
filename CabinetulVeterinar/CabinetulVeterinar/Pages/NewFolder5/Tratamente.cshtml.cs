using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace CabinetulVeterinar.Pages.NewFolder5
{
    public class TratamenteModel : PageModel
    {
        private readonly ILogger<TratamenteModel> _logger;

        [BindProperty]
        public string NumePacientCautat { get; set; }

        public List<TratamentDetails> TratamentDetails { get; set; } = new List<TratamentDetails>();
        public List<TratamentStatisticaInfo> TratamenteStatistici { get; set; } = new List<TratamentStatisticaInfo>();

        public TratamenteModel(ILogger<TratamenteModel> logger)
        {
            _logger = logger;
        }

        public void OnPost()
        {
            string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

            if (string.IsNullOrEmpty(NumePacientCautat))
            {
                ModelState.AddModelError(string.Empty, "Introduceți numele pacientului.");
                return;
            }

            // Interogare JOIN cu parametru variabil
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlJoin = @"
                        SELECT 
                            D.Nume AS NumeDoctor,
                            D.Specializare AS Specializare,
                            T.Medicament AS MedicamentPrescris,
                            T.Doza AS DozaRecomandata,
                            T.modAdministrare AS ModAdministrare
                        FROM Tratament T
                        INNER JOIN Consultatie C ON T.ConsultatieID = C.ConsultatieID
                        INNER JOIN Doctori D ON C.DoctorID = D.DoctorID
                        WHERE C.ProgramareID IN (
                            SELECT PR.ProgramareID
                            FROM Programare PR
                            INNER JOIN Pacienti P ON PR.PacientID = P.PacientID
                            WHERE P.Nume LIKE '%' + @NumePacientCautat + '%'
                        )
                        ORDER BY D.Nume, T.Medicament;";

                    using (SqlCommand command = new SqlCommand(sqlJoin, connection))
                    {
                        command.Parameters.AddWithValue("@NumePacientCautat", NumePacientCautat);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TratamentDetails.Add(new TratamentDetails
                                {
                                    NumeDoctor = reader.GetString(0),
                                    Specializare = reader.GetString(1),
                                    MedicamentPrescris = reader.GetString(2),
                                    DozaRecomandata = reader.GetString(3),
                                    ModAdministrare = reader.GetString(4)
                                });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea detaliilor tratamentelor: {ex.Message}");
            }

            // Interogare complexă cu subcerere și parametru variabil
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sqlSubquery = @"
                        SELECT 
                            P.Nume AS NumePacient,
                            P.Rasa AS RasaPacient,
                            (SELECT COUNT(*) 
                             FROM Tratament T 
                             WHERE T.ConsultatieID IN (
                                 SELECT C.ConsultatieID
                                 FROM Consultatie C
                                 INNER JOIN Programare PR ON C.ProgramareID = PR.ProgramareID
                                 WHERE PR.PacientID = P.PacientID
                             )) AS NumarTratamente,
                            (SELECT STUFF(
                                (SELECT ', ' + T.Medicament
                                 FROM Tratament T
                                 WHERE T.ConsultatieID IN (
                                     SELECT C.ConsultatieID
                                     FROM Consultatie C
                                     INNER JOIN Programare PR ON C.ProgramareID = PR.ProgramareID
                                     WHERE PR.PacientID = P.PacientID
                                 )
                                 FOR XML PATH('')), 1, 2, '')
                            ) AS MedicamentePrescrise
                        FROM Pacienti P
                        WHERE P.Nume LIKE '%' + @NumePacientCautat + '%'
                        ORDER BY NumarTratamente DESC;";

                    using (SqlCommand command = new SqlCommand(sqlSubquery, connection))
                    {
                        command.Parameters.AddWithValue("@NumePacientCautat", NumePacientCautat);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                TratamenteStatistici.Add(new TratamentStatisticaInfo
                                {
                                    NumePacient = reader.GetString(0),
                                    RasaPacient = reader.GetString(1),
                                    NumarTratamente = reader.GetInt32(2),
                                    MedicamentePrescrise = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea statisticilor tratamentelor: {ex.Message}");
            }
        }
    }

    public class TratamentDetails
    {
        public string NumeDoctor { get; set; }
        public string Specializare { get; set; }
        public string MedicamentPrescris { get; set; }
        public string DozaRecomandata { get; set; }
        public string ModAdministrare { get; set; }
    }

    public class TratamentStatisticaInfo
    {
        public string NumePacient { get; set; }
        public string RasaPacient { get; set; }
        public int NumarTratamente { get; set; }
        public string MedicamentePrescrise { get; set; }
    }
}
