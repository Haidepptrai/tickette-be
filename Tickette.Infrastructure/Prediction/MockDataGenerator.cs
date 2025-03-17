using Microsoft.ML.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using Tickette.Infrastructure.Prediction.Models;

namespace Tickette.Infrastructure.Prediction;

public class MockDataGenerator
{
    private readonly Random _random = new Random(42); // Fixed seed for reproducibility
    private readonly List<Guid> _userIds = new List<Guid>();
    private readonly List<Guid> _eventIds = new List<Guid>();
    private readonly Dictionary<Guid, HashSet<Guid>> _userEventInteractions = new Dictionary<Guid, HashSet<Guid>>();
    
    public MockDataGenerator(int numUsers = 100, int numEvents = 200)
    {
        // Generate user IDs
        for (int i = 0; i < numUsers; i++)
        {
            _userIds.Add(Guid.NewGuid());
            _userEventInteractions[_userIds[i]] = new HashSet<Guid>();
        }
        
        // Generate event IDs
        for (int i = 0; i < numEvents; i++)
        {
            _eventIds.Add(Guid.NewGuid());
        }
    }
    
    public List<EventRating> GenerateEventRatings(int numInteractions = 2000)
    {
        var ratings = new List<EventRating>();
        
        // Generate random interactions (biased toward certain patterns)
        for (int i = 0; i < numInteractions; i++)
        {
            // Select user (weighted toward some users being more active)
            var userIndex = (int)Math.Sqrt(_random.NextDouble() * _userIds.Count * _userIds.Count);
            var userId = _userIds[userIndex];
            
            // Select event (with category affinity - simulating that users like certain types of events)
            Guid eventId;
            if (_random.NextDouble() < 0.8 && _userEventInteractions[userId].Count > 0)
            {
                // 80% chance to choose an event "similar" to ones the user has already seen
                // (we'll simulate this by choosing events with IDs close to ones they've seen)
                var previousEvent = _userEventInteractions[userId].ElementAt(_random.Next(_userEventInteractions[userId].Count));
                var previousIndex = _eventIds.IndexOf(previousEvent);
                var newIndex = Math.Max(0, Math.Min(_eventIds.Count - 1, 
                    previousIndex + _random.Next(-10, 11))); // ±10 to simulate same category
                eventId = _eventIds[newIndex];
            }
            else
            {
                // 20% chance to choose a random event
                eventId = _eventIds[_random.Next(_eventIds.Count)];
            }
            
            // Skip if user already has this event
            if (_userEventInteractions[userId].Contains(eventId))
            {
                i--; // Try again
                continue;
            }
            
            // Record interaction
            _userEventInteractions[userId].Add(eventId);
            
            // Generate rating (1-5 scale, biased toward higher ratings)
            var baseRating = 3.0f + (float)(_random.NextDouble() * 2.0); // Baseline 3-5
            
            // Add some user-specific bias (some users rate higher/lower)
            var userBias = (float)(_random.NextDouble() * 0.5 - 0.25);
            
            // Add some event-specific bias (some events are generally better/worse)
            var eventIndex = _eventIds.IndexOf(eventId);
            var eventBias = (float)(Math.Sin(eventIndex * 0.1) * 0.5); // Oscillates between -0.5 and 0.5
            
            // Calculate final rating and ensure it's between 1-5
            var rating = Math.Max(1.0f, Math.Min(5.0f, baseRating + userBias + eventBias));
            
            // Normalize to 0-1 range for ML.NET
            var normalizedRating = (rating - 1) / 4.0f;
            
            // Add to dataset
            ratings.Add(new EventRating
            {
                UserId = userId.ToString(),
                EventId = eventId.ToString(),
                Label = normalizedRating
            });
        }
        
        return ratings;
    }
    
    public List<Guid> GetCandidateEventsForUser(Guid userId)
    {
        // If it's a user we know about, return events they haven't interacted with
        if (_userEventInteractions.ContainsKey(userId))
        {
            return _eventIds
                .Where(eventId => !_userEventInteractions[userId].Contains(eventId))
                .ToList();
        }
        
        // For unknown users, return all events
        return _eventIds.ToList();
    }
    
    public List<Guid> GetAllUserIds() => _userIds;
    
    public List<Guid> GetAllEventIds() => _eventIds;
} 