﻿using System;

namespace BusinessApp.Domain
{
    /// <summary>
    /// Represents the Id of an entity
    /// </summary>
    public interface IEntityId : IConvertible
    {
        bool IConvertible.ToBoolean(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        byte IConvertible.ToByte(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        char IConvertible.ToChar(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        DateTime IConvertible.ToDateTime(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        decimal IConvertible.ToDecimal(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        double IConvertible.ToDouble(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        short IConvertible.ToInt16(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        int IConvertible.ToInt32(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        long IConvertible.ToInt64(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        sbyte IConvertible.ToSByte(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        float IConvertible.ToSingle(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        string IConvertible.ToString(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        object IConvertible.ToType(Type conversionType, IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        ushort IConvertible.ToUInt16(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        uint IConvertible.ToUInt32(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }

        ulong IConvertible.ToUInt64(IFormatProvider? provider)
        {
            throw new InvalidCastException();
        }
    }
}
