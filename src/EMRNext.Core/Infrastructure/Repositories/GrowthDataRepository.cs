using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using EMRNext.Core.Domain.Entities;
using EMRNext.Core.Interfaces;
using EMRNext.Core.Models.Growth;
using System.Text.Json;

namespace EMRNext.Core.Infrastructure.Repositories
{
    public class GrowthDataRepository : IGrowthDataRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GrowthDataRepository> _logger;
        private static readonly TimeSpan StandardsCacheTime = TimeSpan.FromHours(24);
        private static readonly TimeSpan MeasurementsCacheTime = TimeSpan.FromMinutes(15);

        public GrowthDataRepository(
            ApplicationDbContext context,
            IMemoryCache cache,
            ILogger<GrowthDataRepository> logger)
        {
            _context = context;
            _cache = cache;
            _logger = logger;
        }

        public async Task<GrowthStandard> GetGrowthStandardAsync(GrowthStandardType type)
        {
            string cacheKey = $"growth_standard_{type}";
            
            if (!_cache.TryGetValue(cacheKey, out GrowthStandard standard))
            {
                var entity = await _context.GrowthStandards
                    .Include(x => x.PercentileData)
                    .Where(x => x.Type == type && x.IsActive)
                    .OrderByDescending(x => x.EffectiveDate)
                    .FirstOrDefaultAsync();

                if (entity == null)
                    return null;

                standard = MapToModel(entity);
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(StandardsCacheTime)
                    .SetPriority(CacheItemPriority.High);
                
                _cache.Set(cacheKey, standard, cacheOptions);
            }

            return standard;
        }

        public async Task SaveGrowthStandardAsync(GrowthStandard standard)
        {
            var entity = new GrowthStandardEntity
            {
                Type = standard.Type,
                Name = standard.Name,
                Version = standard.Version,
                Gender = standard.Gender,
                EffectiveDate = standard.EffectiveDate,
                IsActive = true,
                LastUpdated = DateTime.UtcNow,
                MetadataJson = JsonSerializer.Serialize(standard.Metadata)
            };

            _context.GrowthStandards.Add(entity);
            await _context.SaveChangesAsync();
            
            // Invalidate cache
            string cacheKey = $"growth_standard_{standard.Type}";
            _cache.Remove(cacheKey);
        }

        public async Task<List<GrowthMeasurement>> GetMeasurementsAsync(
            int patientId, 
            MeasurementType type, 
            DateTime startDate, 
            DateTime endDate)
        {
            string cacheKey = $"measurements_{patientId}_{type}_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}";
            
            if (!_cache.TryGetValue(cacheKey, out List<GrowthMeasurement> measurements))
            {
                var entities = await _context.PatientMeasurements
                    .Where(x => x.PatientId == patientId 
                           && x.Type == type
                           && x.MeasurementDate >= startDate 
                           && x.MeasurementDate <= endDate)
                    .OrderBy(x => x.MeasurementDate)
                    .ToListAsync();

                measurements = entities.Select(MapToModel).ToList();
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(MeasurementsCacheTime)
                    .SetPriority(CacheItemPriority.Normal);
                
                _cache.Set(cacheKey, measurements, cacheOptions);
            }

            return measurements;
        }

        public async Task SaveMeasurementAsync(GrowthMeasurement measurement)
        {
            var entity = new PatientMeasurementEntity
            {
                PatientId = measurement.PatientId,
                Type = measurement.Type,
                Value = measurement.Value,
                MeasurementDate = measurement.Date,
                Source = measurement.Source,
                Notes = measurement.Notes,
                ProviderId = measurement.ProviderId,
                MetadataJson = JsonSerializer.Serialize(measurement.Metadata)
            };

            _context.PatientMeasurements.Add(entity);
            await _context.SaveChangesAsync();
            
            // Invalidate relevant caches
            var cachePattern = $"measurements_{measurement.PatientId}_{measurement.Type}_*";
            InvalidateCachePattern(cachePattern);
        }

        public async Task<List<GrowthAlert>> GetAlertsAsync(
            int patientId, 
            DateTime startDate, 
            DateTime endDate)
        {
            var entities = await _context.GrowthAlerts
                .Where(x => x.PatientId == patientId
                       && x.DetectedDate >= startDate 
                       && x.DetectedDate <= endDate)
                .OrderByDescending(x => x.DetectedDate)
                .ToListAsync();

            return entities.Select(MapToModel).ToList();
        }

        public async Task SaveAlertAsync(GrowthAlert alert)
        {
            var entity = new GrowthAlertEntity
            {
                PatientId = alert.PatientId,
                Type = alert.Type,
                AlertType = alert.AlertType,
                Description = alert.Description,
                DetectedDate = alert.DetectedDate,
                IsResolved = alert.IsResolved,
                ResolvedDate = alert.ResolvedDate,
                Resolution = alert.Resolution,
                ProviderId = alert.ProviderId
            };

            _context.GrowthAlerts.Add(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<GrowthVelocity> CalculateVelocityAsync(
            int patientId, 
            MeasurementType type, 
            DateTime startDate, 
            DateTime endDate)
        {
            var measurements = await GetMeasurementsAsync(patientId, type, startDate, endDate);
            
            if (measurements.Count < 2)
                return null;

            var first = measurements.First();
            var last = measurements.Last();
            
            var timeDiff = (last.Date - first.Date).TotalDays / 365.25; // Convert to years
            var valueDiff = last.Value - first.Value;
            
            return new GrowthVelocity
            {
                PatientId = patientId,
                Type = type,
                StartDate = first.Date,
                EndDate = last.Date,
                StartValue = first.Value,
                EndValue = last.Value,
                VelocityPerYear = valueDiff / timeDiff
            };
        }

        private void InvalidateCachePattern(string pattern)
        {
            // Note: This is a simplified implementation.
            // In production, you might want to use a distributed cache
            // with proper pattern-based cache invalidation support.
            _cache.Remove(pattern);
        }

        private static GrowthStandard MapToModel(GrowthStandardEntity entity)
        {
            return new GrowthStandard
            {
                Type = entity.Type,
                Name = entity.Name,
                Version = entity.Version,
                Gender = entity.Gender,
                EffectiveDate = entity.EffectiveDate,
                Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.MetadataJson),
                PercentileData = entity.PercentileData.Select(p => new PercentileData
                {
                    MeasurementType = p.MeasurementType,
                    Age = p.Age,
                    L = p.L,
                    M = p.M,
                    S = p.S,
                    PercentileValues = JsonSerializer.Deserialize<Dictionary<int, double>>(p.PercentileValuesJson)
                }).ToList()
            };
        }

        private static GrowthMeasurement MapToModel(PatientMeasurementEntity entity)
        {
            return new GrowthMeasurement
            {
                PatientId = entity.PatientId,
                Type = entity.Type,
                Value = entity.Value,
                Date = entity.MeasurementDate,
                Source = entity.Source,
                Notes = entity.Notes,
                ProviderId = entity.ProviderId,
                Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.MetadataJson)
            };
        }

        private static GrowthAlert MapToModel(GrowthAlertEntity entity)
        {
            return new GrowthAlert
            {
                PatientId = entity.PatientId,
                Type = entity.Type,
                AlertType = entity.AlertType,
                Description = entity.Description,
                DetectedDate = entity.DetectedDate,
                IsResolved = entity.IsResolved,
                ResolvedDate = entity.ResolvedDate,
                Resolution = entity.Resolution,
                ProviderId = entity.ProviderId
            };
        }
    }
}
