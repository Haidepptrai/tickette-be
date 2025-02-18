using Microsoft.ML;
using System.Globalization;
using System.Text.RegularExpressions;
using Tickette.Application.Common.Interfaces.Prediction;
using Tickette.Infrastructure.Prediction.Models;

namespace Tickette.Infrastructure.Prediction;

public class TrainingModelService : ITrainingModelService
{
    private readonly MLContext _mlContext = new();
    private readonly string _modelPath = "recommendation_model.zip";

    public void TrainModel()
    {
        // Get data from database
        var eventDetailData = GenerateMockEvents();

        // Preprocess text data
        var processedData = PreprocessEvents(eventDetailData);

        // Load into ML.NET IDataView
        IDataView data = _mlContext.Data.LoadFromEnumerable(processedData);

        // Split into training (80%) and testing (20%)
        var trainTestSplit = _mlContext.Data.TrainTestSplit(data, testFraction: 0.2);
        var trainingData = trainTestSplit.TrainSet;
        var testData = trainTestSplit.TestSet;

        // Print Training Data
        Console.WriteLine("\n🔹 TRAINING DATA:");
        var trainingList = _mlContext.Data.CreateEnumerable<PreprocessedEventModel>(trainingData, reuseRowObject: false).ToList();
        foreach (var ev in trainingList)
        {
            Console.WriteLine($"{ev.Id} | {ev.ProcessedText}");
        }

        // Print Testing Data
        Console.WriteLine("\n🔹 TESTING DATA:");
        var testingList = _mlContext.Data.CreateEnumerable<PreprocessedEventModel>(testData, reuseRowObject: false).ToList();
        foreach (var ev in testingList)
        {
            Console.WriteLine($"{ev.Id} | {ev.ProcessedText}");
        }

        // Define TF-IDF transformation pipeline
        var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", "ProcessedText");

        // Fit the TF-IDF model
        var transformer = pipeline.Fit(trainingData);
        var transformedTrainingData = transformer.Transform(trainingData);
        var transformedTestData = transformer.Transform(testData);

        // Convert transformed data into List
        var trainingVectors = _mlContext.Data.CreateEnumerable<PreprocessedEventModelVector>(transformedTrainingData, reuseRowObject: false)
            .Where(e => e.Features.Any(v => v != 0))  // Remove empty vectors
            .ToList();
        var testVectors = _mlContext.Data.CreateEnumerable<PreprocessedEventModelVector>(transformedTestData, reuseRowObject: false).ToList();

        // Print TF-IDF Features (Training Data)
        Console.WriteLine("\n🔹 TRAINING DATA TF-IDF VECTORS:");
        foreach (var ev in trainingVectors.Take(5))  // Show first 5 for brevity
        {
            Console.WriteLine($"{ev.Id} | Features: [{string.Join(", ", ev.Features.Take(5))}...]");
        }

        // Print TF-IDF Features (Testing Data)
        Console.WriteLine("\n🔹 TESTING DATA TF-IDF VECTORS:");
        foreach (var ev in testVectors.Take(5))
        {
            Console.WriteLine($"{ev.Id} | Features: [{string.Join(", ", ev.Features.Take(5))}...]");
        }

        // Compute and print similar events for a sample event
        var sampleEventId = trainingVectors[0].Id; // Take first training event
        var similarEvents = GetSimilarEvents(sampleEventId, trainingVectors, eventDetailData, 5);

        // Print recommendations
        Console.WriteLine($"\n🔹 Top recommendations for Event ID: {sampleEventId}");
        foreach (var ev in similarEvents)
        {
            Console.WriteLine($"{ev.Title} | {ev.Category} | {ev.Location}");
        }

        // Save TF-IDF model for future use
        _mlContext.Model.Save(transformer, data.Schema, "TFIDFModel.zip");
        Console.WriteLine("\n✅ Model training complete and saved.");
    }

    public List<EventDetailsModel> GetSimilarEvents(string eventId, List<PreprocessedEventModelVector> processedEvents, List<EventDetailsModel> originalEvents, int topN = 5)
    {
        var targetEvent = processedEvents.FirstOrDefault(e => e.Id == eventId);
        if (targetEvent == null) return new List<EventDetailsModel>();

        var targetVector = targetEvent.Features.ToArray();
        var targetCategory = originalEvents.FirstOrDefault(e => e.Id.ToString() == eventId)?.Category;

        var similarityScores = new List<(string, float)>();

        foreach (var eventItem in processedEvents)
        {
            if (eventItem.Id != targetEvent.Id)
            {
                float similarity = CosineSimilarity(targetVector, eventItem.Features.ToArray());

                // Boost similarity if category matches
                var eventCategory = originalEvents.FirstOrDefault(e => e.Id.ToString() == eventItem.Id)?.Category;
                if (eventCategory == targetCategory)
                {
                    similarity *= 1.2f;  // Increase similarity by 20% if category is the same
                }

                similarityScores.Add((eventItem.Id, similarity));
            }
        }

        similarityScores.Sort((a, b) => b.Item2.CompareTo(a.Item2));
        return similarityScores.Take(topN).Select(s => originalEvents.FirstOrDefault(e => e.Id.ToString() == s.Item1)).ToList();
    }


    private float CosineSimilarity(float[] vectorA, float[] vectorB)
    {
        float dotProduct = 0, magnitudeA = 0, magnitudeB = 0;
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            magnitudeA += vectorA[i] * vectorA[i];
            magnitudeB += vectorB[i] * vectorB[i];
        }
        return dotProduct / (float)(Math.Sqrt(magnitudeA) * Math.Sqrt(magnitudeB));
    }


    private List<PreprocessedEventModel> PreprocessEvents(List<EventDetailsModel> events)
    {
        var processedEvents = new List<PreprocessedEventModel>();

        foreach (var evt in events)
        {
            var cleanedTitle = CleanText(evt.Title);
            var cleanedDescription = CleanText(evt.EventDescription);

            var combinedText = $"{cleanedTitle} {cleanedTitle} {cleanedDescription} {evt.Category} {evt.Organizer}";

            processedEvents.Add(new PreprocessedEventModel
            {
                Id = evt.Id.ToString(),
                ProcessedText = combinedText
            });
        }

        return processedEvents;
    }


    private static string CleanText(string text)
    {
        text = text.ToLower();
        text = Regex.Replace(text, @"[^\w\s]", ""); // Remove punctuation
        text = Regex.Replace(text, @"\s+", " ").Trim(); // Remove extra spaces

        // Remove common stop words (You can expand this list)
        string[] stopWords = { "the", "for", "with", "and", "in", "on", "a", "an", "to", "of", "us" };
        text = string.Join(" ", text.Split(' ').Where(word => !stopWords.Contains(word)));

        return text;
    }

    private (float, float) ExtractPriceRange(string priceRange)
    {
        var matches = Regex.Matches(priceRange, @"\d+");
        if (matches.Count == 2)
        {
            float minPrice = float.Parse(matches[0].Value, CultureInfo.InvariantCulture);
            float maxPrice = float.Parse(matches[1].Value, CultureInfo.InvariantCulture);
            return (minPrice, maxPrice);
        }
        return (0, 0); // Default if parsing fails
    }

    private List<EventDetailsModel> GenerateMockEvents()
    {
        return
        [
            new EventDetailsModel(
                "AI & Future Tech Summit",
                "Join us for an exclusive discussion on AI's future in industries like healthcare, finance, and automation.",
                "Tech Conference",
                "FutureTech Inc.",
                "New York, USA",
                new DateTime(2025, 5, 10, 14, 00, 00),
                "$20 - $150"
            ),

            new EventDetailsModel(
                "Live Jazz Night with Grammy Artists",
                "A night of spectacular live jazz performances featuring Grammy-winning artists and emerging talent.",
                "Music Concert",
                "Jazz & Blues Events",
                "Paris, France",
                new DateTime(2025, 6, 18, 19, 30, 00),
                "$35 - $180"
            ),

            new EventDetailsModel(
                "Startup Growth Conference",
                "Network with top investors and startup founders to learn about growth hacking and funding strategies.",
                "Tech Conference",
                "StartupWorld",
                "San Francisco, USA",
                new DateTime(2025, 4, 20, 10, 00, 00),
                "$50 - $200"
            ),

            new EventDetailsModel(
                "International Comedy Festival",
                "Get ready for a laughter-packed evening with world-renowned comedians.",
                "Comedy Show",
                "Comedy Central",
                "London, UK",
                new DateTime(2025, 8, 5, 18, 00, 00),
                "$25 - $120"
            ),

            new EventDetailsModel(
                "Football Championship Finals",
                "Watch the most anticipated football championship final between top teams!",
                "Sports Event",
                "Global Sports League",
                "Berlin, Germany",
                new DateTime(2025, 7, 15, 20, 00, 00),
                "$30 - $250"
            ),

            new EventDetailsModel(
                "Python for Data Science Workshop",
                "Learn Python and data science fundamentals from industry experts in this interactive workshop.",
                "Workshop",
                "DataScience Academy",
                "Tokyo, Japan",
                new DateTime(2025, 7, 5, 10, 00, 00),
                "$15 - $99"
            ),

            new EventDetailsModel(
                "The Phantom of the Opera - Broadway Special",
                "Experience the magic of Broadway with this timeless musical classic!",
                "Theater Performance",
                "Broadway Productions",
                "Toronto, Canada",
                new DateTime(2025, 9, 1, 19, 00, 00),
                "$40 - $220"
            )
        ];
    }

    private void TestPrediction(ITransformer model)
    {
        var predictionEngine = _mlContext.Model.CreatePredictionEngine<PreprocessedEventModel, EventRecommendationPrediction>(model);

        var sampleEvent = new PreprocessedEventModel
        {
            ProcessedText = "AI & Future Tech Summit discussion on AI and automation in industries"
        };

        var prediction = predictionEngine.Predict(sampleEvent);
        Console.WriteLine($"Predicted Similar Event: {prediction.RecommendedEventId}");
    }
}

public class EventRecommendationPrediction
{
    public string RecommendedEventId { get; set; }
}

public class PreprocessedEventModelVector
{
    public string Id { get; set; }  // Store as string to avoid Guid errors
    public float[] Features { get; set; }  // TF-IDF vector representation
}
