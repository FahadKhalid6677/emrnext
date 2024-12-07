using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMRNext.Core.Interfaces;
using EMRNext.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EMRNext.Infrastructure.Services
{
    public class ResourceManagementService : IResourceManagementService
    {
        private readonly EMRNextDbContext _context;

        public ResourceManagementService(EMRNextDbContext context)
        {
            _context = context;
        }

        public async Task<bool> CheckResourceAvailabilityAsync(int resourceId, DateTime startTime, DateTime endTime)
        {
            var resource = await _context.Resources.FindAsync(resourceId);
            if (resource == null)
                throw new ArgumentException("Resource not found");

            // Check if resource is active
            if (!resource.IsActive)
                return false;

            // Check if resource is available during the specified time
            var isAvailable = await _context.ResourceSchedules
                .Where(s => s.ResourceId == resourceId)
                .AnyAsync(s => s.StartTime <= startTime && s.EndTime >= endTime && s.IsAvailable);

            if (!isAvailable)
                return false;

            // Check for existing bookings
            var hasConflict = await _context.ResourceBookings
                .Where(b => b.ResourceId == resourceId)
                .AnyAsync(b => b.StartTime < endTime && b.EndTime > startTime);

            return !hasConflict;
        }

        public async Task<ResourceBooking> BookResourceAsync(int resourceId, DateTime startTime, DateTime endTime, string purpose)
        {
            var isAvailable = await CheckResourceAvailabilityAsync(resourceId, startTime, endTime);
            if (!isAvailable)
                throw new InvalidOperationException("Resource is not available for the specified time period");

            var booking = new ResourceBooking
            {
                ResourceId = resourceId,
                StartTime = startTime,
                EndTime = endTime,
                Purpose = purpose,
                Status = "Confirmed",
                BookedAt = DateTime.UtcNow
            };

            _context.ResourceBookings.Add(booking);
            await _context.SaveChangesAsync();

            return booking;
        }

        public async Task CancelResourceBookingAsync(int bookingId)
        {
            var booking = await _context.ResourceBookings.FindAsync(bookingId);
            if (booking == null)
                throw new ArgumentException("Booking not found");

            booking.Status = "Cancelled";
            booking.CancelledAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task<List<Resource>> GetAvailableResourcesAsync(DateTime startTime, DateTime endTime, string resourceType = null)
        {
            var query = _context.Resources
                .Where(r => r.IsActive);

            if (!string.IsNullOrEmpty(resourceType))
                query = query.Where(r => r.Type == resourceType);

            var resources = await query.ToListAsync();
            var availableResources = new List<Resource>();

            foreach (var resource in resources)
            {
                var isAvailable = await CheckResourceAvailabilityAsync(resource.Id, startTime, endTime);
                if (isAvailable)
                    availableResources.Add(resource);
            }

            return availableResources;
        }

        public async Task<ResourceSchedule> UpdateResourceScheduleAsync(int resourceId, List<ResourceScheduleEntry> schedule)
        {
            var resource = await _context.Resources.FindAsync(resourceId);
            if (resource == null)
                throw new ArgumentException("Resource not found");

            // Remove existing schedule
            var existingSchedule = await _context.ResourceSchedules
                .Where(s => s.ResourceId == resourceId)
                .ToListAsync();

            _context.ResourceSchedules.RemoveRange(existingSchedule);

            // Add new schedule
            foreach (var entry in schedule)
            {
                _context.ResourceSchedules.Add(new ResourceSchedule
                {
                    ResourceId = resourceId,
                    StartTime = entry.StartTime,
                    EndTime = entry.EndTime,
                    IsAvailable = entry.IsAvailable
                });
            }

            await _context.SaveChangesAsync();

            return await _context.ResourceSchedules
                .Where(s => s.ResourceId == resourceId)
                .OrderBy(s => s.StartTime)
                .FirstOrDefaultAsync();
        }

        public async Task<Resource> AddResourceAsync(Resource resource)
        {
            _context.Resources.Add(resource);
            await _context.SaveChangesAsync();
            return resource;
        }

        public async Task<Resource> UpdateResourceAsync(Resource resource)
        {
            var existingResource = await _context.Resources.FindAsync(resource.Id);
            if (existingResource == null)
                throw new ArgumentException("Resource not found");

            existingResource.Name = resource.Name;
            existingResource.Type = resource.Type;
            existingResource.Description = resource.Description;
            existingResource.IsActive = resource.IsActive;

            await _context.SaveChangesAsync();
            return existingResource;
        }

        public async Task<List<ResourceBooking>> GetResourceBookingsAsync(int resourceId, DateTime startDate, DateTime endDate)
        {
            return await _context.ResourceBookings
                .Where(b => b.ResourceId == resourceId &&
                           b.StartTime >= startDate &&
                           b.EndTime <= endDate &&
                           b.Status != "Cancelled")
                .OrderBy(b => b.StartTime)
                .ToListAsync();
        }
    }

    public class Resource
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
    }

    public class ResourceBooking
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Purpose { get; set; }
        public string Status { get; set; }
        public DateTime BookedAt { get; set; }
        public DateTime? CancelledAt { get; set; }
    }

    public class ResourceSchedule
    {
        public int Id { get; set; }
        public int ResourceId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }

    public class ResourceScheduleEntry
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsAvailable { get; set; }
    }
}
