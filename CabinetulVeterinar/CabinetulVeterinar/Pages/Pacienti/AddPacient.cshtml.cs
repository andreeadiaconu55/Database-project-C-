using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CabinetulVeterinar.Pages.Pacienti
{
    public class AddPacientModel : PageModel
    {
        private readonly ILogger<AddPacientModel> _logger;

        [BindProperty]
        public PacientDetails Pacient { get; set; }

        public string Message { get; set; }

        public AddPacientModel(ILogger<AddPacientModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            // Inițializare pentru orice logică de pregătire a paginii (dacă este necesar).
        }

        public IActionResult OnPost()
        {
            ModelState.Remove("Pacient.Id");
            Console.WriteLine("OnPost a fost apelată pentru adăugarea pacientului.");
            _logger.LogInformation("OnPost a fost apelată pentru adăugarea pacientului.");

            if (!ModelState.IsValid)
            {
                var errorMessages = new List<string>();
                foreach (var state in ModelState)
                {
                    foreach (var error in state.Value.Errors)
                    {
                        errorMessages.Add($"Câmp: {state.Key}, Eroare: {error.ErrorMessage}");
                    }
                }

                Message = "Datele introduse nu sunt valide. Verifică câmpurile.\n" + string.Join("\n", errorMessages);
                return Page();
            }

            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
                    INSERT INTO Pacienti (Nume, Specie, Rasa, Sex, DataNasterii, Greutate)
                    VALUES (@Name, @Specie, @Rasa, @Sex, @DataNasterii, @Greutate)";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Name", Pacient.Name);
                        command.Parameters.AddWithValue("@Specie", Pacient.Specie);
                        command.Parameters.AddWithValue("@Rasa", Pacient.Rasa);
                        command.Parameters.AddWithValue("@Sex", Pacient.Sex);
                        command.Parameters.AddWithValue("@DataNasterii", Pacient.DataNasterii);
                        command.Parameters.AddWithValue("@Greutate", Pacient.Greutate);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Message = "Pacientul a fost adăugat cu succes!";
                            Console.WriteLine("Pacient adăugat cu succes!");
                        }
                        else
                        {
                            Message = "A apărut o problemă la adăugarea pacientului.";
                            Console.WriteLine("Pacientul nu a fost adăugat.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la adăugarea pacientului: {ex.Message}");
                Message = "Eroare la adăugarea pacientului.";
            }

            return RedirectToPage("/Pacienti/Index");
        }
    }

    public class PacientDetails
    {
        [BindNever] // Exclude acest câmp din binding și validare
        public string Id { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Specia este obligatorie.")]
        public string Specie { get; set; }

        [Required(ErrorMessage = "Rasa este obligatorie.")]
        public string Rasa { get; set; }

        [Required(ErrorMessage = "Sexul este obligatoriu.")]
        public string Sex { get; set; }

        [Required(ErrorMessage = "Data nașterii este obligatorie.")]
        public DateTime DataNasterii { get; set; }

        [Required(ErrorMessage = "Greutatea este obligatorie.")]
        [Range(0.1, 500.0, ErrorMessage = "Greutatea trebuie să fie între 0.1 și 500 kg.")]
        public double Greutate { get; set; }
    }
}
