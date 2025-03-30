using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace CabinetulVeterinar.Pages.NewFolder2
{
    public class AddProgramareModel : PageModel
    {
        private readonly ILogger<AddProgramareModel> _logger;

        [BindProperty]
        public ProgramareDetails ProgramareDetails { get; set; } = new ProgramareDetails();

        public string Message { get; set; }

        public AddProgramareModel(ILogger<AddProgramareModel> logger)
        {
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            // Verificare sesiune și preluare DoctorID
            string doctorId = HttpContext.Session.GetString("DoctorID");

            if (string.IsNullOrEmpty(doctorId))
            {
                Console.WriteLine("DoctorID lipsește din sesiune. Redirecționare către login.");
                return RedirectToPage("/Login/Login");
            }

            ProgramareDetails.DoctorId = doctorId; // Setare DoctorId pentru adăugare
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                Message = "Datele introduse nu sunt valide.";
                return Page();
            }

            try
            {
                // Preluare DoctorID din sesiune
                string doctorId = HttpContext.Session.GetString("DoctorID");
                if (string.IsNullOrEmpty(doctorId))
                {
                    return RedirectToPage("/Login/Index");
                }

                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=\"Cabinet veterinar\";Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                        INSERT INTO Programare (PacientID, StapanID, DataProgramare, OraProgramare, Motiv)
                        VALUES (@PacientId, @DoctorId, @DataProgramare, @OraProgramare, @Motiv);";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@PacientId", ProgramareDetails.PacientId);
                        command.Parameters.AddWithValue("@DoctorId", doctorId);
                        command.Parameters.AddWithValue("@DataProgramare", ProgramareDetails.DataProgramare);
                        command.Parameters.AddWithValue("@OraProgramare", ProgramareDetails.OraProgramare);
                        command.Parameters.AddWithValue("@Motiv", ProgramareDetails.Motiv);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            return RedirectToPage("/Login/Index");
                        }
                        else
                        {
                            Message = "Inserarea programării a eșuat.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la adăugarea programării: {ex.Message}");
                Message = $"Eroare: {ex.Message}";
            }

            return Page();
        }
    }

    public class ProgramareDetails
    {
        [Required]
        public int PacientId { get; set; }

        [Required]
        public string DoctorId { get; set; }

        [Required]
        public DateTime DataProgramare { get; set; }

        [Required]
        public TimeSpan OraProgramare { get; set; }

        [Required]
        public string Motiv { get; set; }
    }
}
