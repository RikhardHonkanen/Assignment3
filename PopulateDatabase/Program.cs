using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Assignment3;

namespace PopulateDatabase
{
    public class Program
    {
        // An EF connection that we will keep open for the entire program.
        private static AppDbContext database = new AppDbContext();

        public static void Main()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            // Clear the database.
            database.Movies.RemoveRange(database.Movies);
            database.Cinemas.RemoveRange(database.Cinemas);
            database.Screenings.RemoveRange(database.Screenings);
            database.SaveChanges();

            // Load movies.
            string[] movieLines = File.ReadAllLines("SampleMovies.csv");
            foreach (string line in movieLines)
            {
                string[] values = line.Split(',').Select(v => v.Trim()).ToArray();
                Movie newMovie = new Movie();
                newMovie.Title = values[0];
                newMovie.ReleaseDate = DateTime.Parse(values[1]);
                newMovie.Runtime = Int16.Parse(values[2]);
                newMovie.PosterPath = values[3];
                database.Add(newMovie);
            }
            database.SaveChanges();

            // Load cinemas.
            string[] cinemaLines = File.ReadAllLines("SampleCinemasWithPositions.csv");
            foreach (string line in cinemaLines)
            {
                string[] values = line.Split(',').Select(v => v.Trim()).ToArray();
                Cinema newCinema = new Cinema();
                newCinema.City = values[0];
                newCinema.Name = values[1];
                Coordinate cinemaCoordinate = new Coordinate();
                cinemaCoordinate.Latitude = double.Parse(values[2]);
                cinemaCoordinate.Longitude = double.Parse(values[3]);
                cinemaCoordinate.Altitude = 0;
                newCinema.Coordinate = cinemaCoordinate;
                database.Add(newCinema);
            }
            database.SaveChanges();

            // Generate random screenings.
            // Get all cinema IDs.
            var cinemaIDs = new List<int>();
            var cinema = database.Cinemas.Select(c => c.ID);
            foreach (var i in cinema)
            {
                var id = Convert.ToInt32(i);
                cinemaIDs.Add(id);
            }

            // Get all movie IDs.
            var movieIDs = new List<int>();
            var movie = database.Movies.Select(m => m.ID);
            foreach (var i in movie)
            {
                var id = Convert.ToInt32(i);
                movieIDs.Add(id);
            }

            // Create random screenings for each cinema.
            var random = new Random();
            foreach (int cinemaID in cinemaIDs)
            {
                // Choose a random number of screenings.
                int numberOfScreenings = random.Next(2, 6);
                foreach (int n in Enumerable.Range(0, numberOfScreenings))
                {
                    // Pick a random movie.
                    int movieID = movieIDs[random.Next(movieIDs.Count)];

                    // Pick a random hour and minute.
                    int hour = random.Next(24);
                    double[] minuteOptions = { 0, 10, 15, 20, 30, 40, 45, 50 };
                    double minute = minuteOptions[random.Next(minuteOptions.Length)];
                    var time = TimeSpan.FromHours(hour) + TimeSpan.FromMinutes(minute);

                    Screening screening = new Screening();
                    screening.Time = time;
                    screening.Movie = database.Movies.First(m => m.ID == movieID);
                    screening.Cinema = database.Cinemas.First(c => c.ID == cinemaID);
                    database.Add(screening);
                    database.SaveChanges();
                }
            }
        }
    }
}
