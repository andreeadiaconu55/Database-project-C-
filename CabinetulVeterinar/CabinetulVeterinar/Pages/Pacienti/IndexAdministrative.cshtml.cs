using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

namespace CabinetulVeterinar.Pages.Pacienti
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public List<PacientInfo> ListaPacienti { get; set; } = new List<PacientInfo>();
        public List<DoctorInfo> ListaDoctori { get; set; } = new List<DoctorInfo>();

        [BindProperty]
        public PacientInfo Pacient { get; set; }

        [BindProperty]
        public DoctorInfo Doctor { get; set; }

        public bool IsEditing { get; set; }


        public string Message { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Încărcare pacienți
                    string sqlPacienti = "SELECT PacientID, Nume, Specie, Rasa, Sex, DataNasterii, Greutate FROM Pacienti";
                    using (SqlCommand command = new SqlCommand(sqlPacienti, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            ListaPacienti.Clear();
                            while (reader.Read())
                            {
                                ListaPacienti.Add(new PacientInfo
                                {
                                    Id = reader.GetInt32(0).ToString(),
                                    Name = reader.GetString(1),
                                    Specie = reader.GetString(2),
                                    Rasa = reader.GetString(3),
                                    Sex = reader.GetString(4),
                                    DataNasterii = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                    Greutate = reader.GetDouble(6)
                                });
                            }
                        }
                    }

                    // Încărcare doctori
                    string sqlDoctori = "SELECT DoctorID, Nume, Prenume, Specializare FROM Doctori";
                    using (SqlCommand command = new SqlCommand(sqlDoctori, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            ListaDoctori.Clear();
                            while (reader.Read())
                            {
                                ListaDoctori.Add(new DoctorInfo
                                {
                                    Id = reader.GetInt32(0).ToString(),
                                    Nume = reader.GetString(1),
                                    Prenume = reader.GetString(2),
                                    Specializare = reader.GetString(3)
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea datelor: {ex.Message}");
                Message = "Eroare la încărcarea datelor.";
            }
        }

        public IActionResult OnPostDeleteDoctor()
        {
            if (string.IsNullOrEmpty(Doctor?.Id))
            {
                Message = "ID-ul doctorului este necesar pentru ștergere.";
                return Page();
            }

            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "DELETE FROM Doctori WHERE DoctorID = @Id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", int.Parse(Doctor.Id));

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Message = "Doctorul a fost șters cu succes.";
                        }
                        else
                        {
                            Message = "Doctorul nu a fost găsit.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la ștergerea doctorului: {ex.Message}");
                Message = $"Eroare la ștergerea doctorului: {ex.Message}";
            }

            OnGet(); // Reîncarcă lista de doctori și pacienți
            return Page();
        }


        public IActionResult OnPostDelete()
        {
            if (string.IsNullOrEmpty(Pacient?.Id))
            {
                Message = "ID-ul pacientului este necesar pentru ștergere.";
                return Page();
            }

            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "DELETE FROM Pacienti WHERE PacientID = @Id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", int.Parse(Pacient.Id));

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Message = "Pacientul a fost șters cu succes.";
                        }
                        else
                        {
                            Message = "Pacientul nu a fost găsit.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la ștergerea pacientului: {ex.Message}");
                Message = $"Eroare la ștergerea pacientului: {ex.Message}";
            }

            Pacient = null; // Resetează modelul pentru a preveni afișarea formularului de actualizare
            OnGet(); // Reîncarcă lista de pacienți
            return Page();
        }

        private void SetEditMode(bool isEditMode)
        {
            ViewData["EditMode"] = isEditMode;
        }



        public IActionResult OnGetEdit(string id)
        {
            Console.WriteLine($"OnGetEdit a fost apelată pentru ID-ul: {id}");

            if (string.IsNullOrEmpty(id))
            {
                Message = "ID-ul pacientului nu este valid.";
                SetEditMode(false); // Dezactivează EditMode dacă nu este valid
                return RedirectToPage();
            }

            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string sql = "SELECT PacientID, Nume, Specie, Rasa, Sex, DataNasterii, Greutate FROM Pacienti WHERE PacientID = @Id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", int.Parse(id));

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Pacient = new PacientInfo
                                {
                                    Id = reader.GetInt32(0).ToString(),
                                    Name = reader.GetString(1),
                                    Specie = reader.GetString(2),
                                    Rasa = reader.GetString(3),
                                    Sex = reader.GetString(4),
                                    DataNasterii = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5),
                                    Greutate = reader.GetDouble(6)
                                };
                            }
                        }
                    }
                }

                SetEditMode(true); // Activează EditMode
                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la încărcarea pacientului pentru editare: {ex.Message}");
                Message = "Eroare la încărcarea datelor pacientului.";
                SetEditMode(false); // Dezactivează EditMode în caz de eroare
                return RedirectToPage();
            }
        }





        public IActionResult OnPostUpdate()
        {
            if (!ModelState.IsValid)
            {
                Message = "Datele introduse nu sunt valide.";
                return Page();
            }

            try
            {
                string connectionString = "Data Source=DESKTOP-CGPUI8P\\SQLEXPRESS;Initial Catalog=Cabinet Veterinar;Integrated Security=True";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string sql = @"
            UPDATE Pacienti
            SET Nume = @Name, Specie = @Specie, Rasa = @Rasa, Sex = @Sex, DataNasterii = @DataNasterii, Greutate = @Greutate
            WHERE PacientID = @Id";

                    using (SqlCommand command = new SqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@Id", int.Parse(Pacient.Id));
                        command.Parameters.AddWithValue("@Name", Pacient.Name);
                        command.Parameters.AddWithValue("@Specie", Pacient.Specie);
                        command.Parameters.AddWithValue("@Rasa", Pacient.Rasa);
                        command.Parameters.AddWithValue("@Sex", Pacient.Sex);
                        command.Parameters.AddWithValue("@DataNasterii", Pacient.DataNasterii ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Greutate", Pacient.Greutate);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Message = "Pacientul a fost actualizat cu succes!";
                        }
                        else
                        {
                            Message = "Nu s-a găsit pacientul cu ID-ul specificat.";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Eroare la actualizarea pacientului: {ex.Message}");
                Message = "Eroare la actualizarea pacientului.";
            }

            OnGet(); // Reîncarcă lista de pacienți
            return Page();
        }



    }

    public class PacientInfo
    {
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
        public DateTime? DataNasterii { get; set; }

        [Required(ErrorMessage = "Greutatea este obligatorie.")]
        [Range(0.1, 500.0, ErrorMessage = "Greutatea trebuie să fie între 0.1 și 500.")]
        public double Greutate { get; set; }
    }

    public class DoctorInfo
    {
        public string Id { get; set; }

        [Required(ErrorMessage = "Numele este obligatoriu.")]
        public string Nume { get; set; }

        [Required(ErrorMessage = "Prenumele este obligatoriu.")]
        public string Prenume { get; set; }

        [Required(ErrorMessage = "Specializarea este obligatorie.")]
        public string Specializare { get; set; }
    }
}
