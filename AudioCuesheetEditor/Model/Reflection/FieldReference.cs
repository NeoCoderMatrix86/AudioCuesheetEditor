//This file is part of AudioCuesheetEditor.

//AudioCuesheetEditor is free software: you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation, either version 3 of the License, or
//(at your option) any later version.

//AudioCuesheetEditor is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with Foobar.  If not, see
//<http: //www.gnu.org/licenses />.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Model.Reflection
{
    public class FieldReference : IEquatable<FieldReference>
    {
        private readonly PropertyInfo propertyInfo;

        public static FieldReference Create(object ownerObject, String property)
        {
            return new FieldReference(ownerObject, property);
        }

        #region IEquatable

        public override bool Equals(object obj) => (obj is FieldReference other) && this.Equals(other);

        public bool Equals(FieldReference other)
        {
            if (other is null)
            {
                return false;
            }

            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (this.GetType() != other.GetType())
            {
                return false;
            }

            return (Owner == other.Owner) && (Property == other.Property) && (CompleteName == other.CompleteName);
        }

        public static bool operator ==(FieldReference lhs, FieldReference rhs)
        {
            if (lhs is null)
            {
                if (rhs is null)
                {
                    return true;
                }

                return false;
            }
            return lhs.Equals(rhs);
        }

        public static bool operator !=(FieldReference lhs, FieldReference rhs)
        {
            return !(lhs == rhs);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Owner, Property, CompleteName);
        }

        #endregion

        private FieldReference(object ownerObject, String property)
        {
            if (String.IsNullOrEmpty(property) == true)
            {
                throw new ArgumentNullException(nameof(property));
            }
            Owner = ownerObject;
            propertyInfo = Owner.GetType().GetProperty(property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (propertyInfo == null)
            {
                throw new ArgumentException(String.Format("Property {0} could not be found!", property),nameof(property));
            }
        }

        public String CompleteName { get { return String.Format("{0}.{1}", Owner.GetType().Name, propertyInfo.Name); } }
        public object Owner { get; private set; }
        public String Property { get { return propertyInfo.Name; } }
    }
}
