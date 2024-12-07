using System;

namespace EMRNext.Core.Domain.ValueObjects
{
    /// <summary>
    /// Represents a postal address as a value object
    /// </summary>
    public class Address : IEquatable<Address>
    {
        public string StreetLine1 { get; }
        public string StreetLine2 { get; }
        public string City { get; }
        public string State { get; }
        public string PostalCode { get; }
        public string Country { get; }

        public Address(
            string streetLine1, 
            string city, 
            string state, 
            string postalCode, 
            string country, 
            string streetLine2 = null)
        {
            StreetLine1 = streetLine1 ?? throw new ArgumentNullException(nameof(streetLine1));
            City = city ?? throw new ArgumentNullException(nameof(city));
            State = state ?? throw new ArgumentNullException(nameof(state));
            PostalCode = postalCode ?? throw new ArgumentNullException(nameof(postalCode));
            Country = country ?? throw new ArgumentNullException(nameof(country));
            StreetLine2 = streetLine2;
        }

        public bool Equals(Address other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return StreetLine1 == other.StreetLine1 &&
                   StreetLine2 == other.StreetLine2 &&
                   City == other.City &&
                   State == other.State &&
                   PostalCode == other.PostalCode &&
                   Country == other.Country;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Address)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StreetLine1, StreetLine2, City, State, PostalCode, Country);
        }

        public override string ToString()
        {
            return $"{StreetLine1} {StreetLine2}, {City}, {State} {PostalCode}, {Country}";
        }
    }
}
